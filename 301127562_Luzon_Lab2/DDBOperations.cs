using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace _301127562_Luzon_Lab2
{
    /// <summary>
    /// Thedyson Luzon - Centennial College F2023
    /// DDBOperations.cs
    /// </summary>
    public class DDBOperations
    {
        AmazonDynamoDBClient client;
        BasicAWSCredentials credentials;
        DynamoDBContext context;

        public async Task UpdateUserBooksAsync(string userName, string bookISBN, int lastPageOpened, string lastOpened, int pageCount)
        {
            context = new(Helper.dbClient);

            Table table = Table.LoadTable(Helper.dbClient, "Users");

            QueryFilter filter = new QueryFilter();
            filter.AddCondition("UserName", QueryOperator.Equal, userName);

            QueryOperationConfig config = new QueryOperationConfig
            {
                Filter = filter,
                ConsistentRead = true
            };

            Search search = table.Query(config);
            List<Document> results = await search.GetRemainingAsync();

            foreach (var result in results)
            {
                User user = new User
                {
                    UserName = result["UserName"],
                    Email = result["Email"],
                    Password = result["Password"],
                    Books = new List<Book>()
                };

                // Add each book associated with the user to the Books list
                for (int i = 1; i <= 2; i++) // Assuming there are 2 books per user
                {
                    var bookAttribute = result[$"Book{i}"];
                    if (bookAttribute != null)
                    {
                        var bookDocument = bookAttribute.AsDocument();
                        var bookISBNValue = bookDocument["ISBN"].AsString();

                        if (bookISBNValue == bookISBN)
                        {
                            // Update the LastPageOpened and LastOpened for the specific book
                            bookDocument["LastPageOpened"] = lastPageOpened;
                            bookDocument["LastOpened"] = lastOpened;
                            bookDocument["PageCount"] = pageCount;

                            // Save the updated book back to DynamoDB
                            await table.UpdateItemAsync(result);
                        }

                        user.Books.Add(new Book
                        {
                            ISBN = bookDocument["ISBN"],
                            Title = bookDocument["Title"],
                            Authors = string.Join(", ", bookDocument["Authors"].AsListOfString()),
                            S3Key = bookDocument["S3Key"],
                            ImageUrl = bookDocument["ImageUrl"],
                            LastOpened = bookDocument["LastOpened"].AsString(),
                            PageCount = bookDocument["PageCount"].AsInt(),
                            LastPageOpened = bookDocument["LastPageOpened"].AsInt()
                        });
                    }
                }
            }
        }

        public static async Task<DeleteTableResponse?> DeleteTable(IAmazonDynamoDB client, string tableName)
        {
            try
            {
                var response = await Helper.dbClient.DeleteTableAsync(new DeleteTableRequest
                {
                    TableName = tableName,
                });

                return response;
            }
            catch (ResourceNotFoundException)
            {
                // There is no such table.
                return null;
            }
        }

        public static async Task<DescribeTableResponse> CreateTableUsers(IAmazonDynamoDB client)
        {
            string tableName = "Users";

            var response = await Helper.dbClient.CreateTableAsync(new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>()
          {
            new AttributeDefinition
            {
              AttributeName = "UserName",
              AttributeType = ScalarAttributeType.S,
            },
          },
                KeySchema = new List<KeySchemaElement>()
          {
            new KeySchemaElement
            {
              AttributeName = "UserName",
              KeyType = KeyType.HASH,
            },
          },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 1,
                },
            });

            var result = await WaitTillTableCreated(client, tableName, response);
            return result;
        }

        public static async Task<DescribeTableResponse> CreateTableBookshelf(IAmazonDynamoDB client)
        {
            string tableName = "Bookshelf";

            var response = await Helper.dbClient.CreateTableAsync(new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>()
          {
            new AttributeDefinition
            {
              AttributeName = "ISBN",
              AttributeType = ScalarAttributeType.S,
            },
          },
                KeySchema = new List<KeySchemaElement>()
          {
            new KeySchemaElement
            {
              AttributeName = "ISBN",
              KeyType = KeyType.HASH,
            },
          },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 1,
                },
            });

            var result = await WaitTillTableCreated(client, tableName, response);

            return result;
        }

        public static async Task<DescribeTableResponse> WaitTillTableCreated(
          IAmazonDynamoDB client,
          string tableName,
          CreateTableResponse response)
        {
            DescribeTableResponse resp = new();

            var tableDescription = response.TableDescription;

            string status = tableDescription.TableStatus;

            int sleepDuration = 1000; // One second
            // Don't wait more than 10 seconds.
            while ((status != "ACTIVE") && (sleepDuration < 10000))
            {
                System.Threading.Thread.Sleep(sleepDuration);

                resp = await client.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = tableName,
                });

                status = resp.Table.TableStatus;

                sleepDuration *= 2;
            }

            return resp;
        }

        public static async Task<DescribeTableResponse> WaitTillTableDeleted(
         IAmazonDynamoDB client,
         string tableName,
         DeleteTableResponse response)
        {
            DescribeTableResponse resp = new();
            var tableDescription = response.TableDescription;

            string status = tableDescription.TableStatus;

            int sleepDuration = 1000; // One second
            while ((status == "DELETING") && (sleepDuration < 10000))
            {
                System.Threading.Thread.Sleep(sleepDuration);

                resp = await client.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = tableName,
                });

                status = resp.Table.TableStatus;

                sleepDuration *= 2;
            }

            return resp;
        }

        public static async Task InsertSampleUsers(IAmazonDynamoDB client)
        {
            Table usersTable = Table.LoadTable(client, "Users", DynamoDBEntryConversion.V2);

            // Add users to Users table.

            //User 1
            Document newUser = new();
            newUser["UserName"] = "Teddy1";
            newUser["Email"] = "teddy1email@teddy.com";
            newUser["Password"] = "teddy1pw";
            Document book1 = new Document
            {
                ["ISBN"] = "1503222683",
                ["Title"] = "Alice Adventures in Wonderland",
                ["Authors"] = new List<string> { "Lewis Carol" },
                ["S3Key"] = "06. Alice Adventures in Wonderland author Lewis Carrol.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/aliceinwonderland.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1
            };
        
            Document book2 = new Document
            {
                ["ISBN"] = "9780156012195",
                ["Title"] = "The Little Prince",
                ["Authors"] = new List<string> { "Antoine de Saint-Exupéry", },
                ["S3Key"] = "07. The Little Prince author Antoine de Saint-Exupéry.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/littleprince.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1

            };

            newUser["Book1"] = book1;
            newUser["Book2"] = book2;

            await usersTable.PutItemAsync(newUser);

            // User2
            Document newUser2 = new();
            newUser2["UserName"] = "Teddy2";
            newUser2["Password"] = "teddy2pw";
            newUser2["Email"] = "teddy2email@teddy.com";
            Document book3 = new Document
            {
                ["Title"] = "The voyages and travels of Sindbad the Sailor",
                ["ISBN"] = "9780282405236",
                ["Authors"] = new List<string> { "Thomas Richardson" },
                ["S3Key"] = "08. The voyages and travels of Sindbad the Sailor author Thomas Richardson.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/voyagessinbad.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1

            };

            Document book4 = new Document
            {
                ["Title"] = "Jack and the Beanstalk",
                ["ISBN"] = "9781909115637",
                ["Authors"] = new List<string> { "Joseph Jacobs", },
                ["S3Key"] = "16. Jack and the Beanstalk author Joseph Jacobs.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/jackandbeanstalk.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1


            };

            newUser2["Book1"] = book3;
            newUser2["Book2"] = book4;

            await usersTable.PutItemAsync(newUser2);


            // User3
            Document newUser3 = new();
            newUser3["UserName"] = "Teddy3";
            newUser3["Password"] = "teddy3pw";
            newUser3["Email"] = "teddy3email@teddy.com";
            Document book5 = new Document
            {
                ["Title"] = "The Juniper-Tree",
                ["ISBN"] = "9780374339715",
                ["Authors"] = new List<string> { "Miss Mulock" },
                ["S3Key"] = "17. The Juniper-Tree author Miss Mulock.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/junipertree.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1

            };

            Document book6 = new Document
            {
                ["Title"] = "The brave tin soldier",
                ["ISBN"] = "9781848775114",
                ["Authors"] = new List<string> { "Hans Christian" },
                ["S3Key"] = "21. The brave tin soldier author Hans Christian Andersen.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/bravetin.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1


            };


            newUser3["Book1"] = book5;
            newUser3["Book2"] = book6;

            await usersTable.PutItemAsync(newUser3);

        }

        public static async Task InsertSampleBooks(IAmazonDynamoDB client)
        {
            Table booksTable = Table.LoadTable(client, "Bookshelf", DynamoDBEntryConversion.V2);

            // Add books to Books table.

            Document newBook1 = new Document
            {
                ["ISBN"] = "1503222683",
                ["Title"] = "Alice Adventures in Wonderland",
                ["Authors"] = new List<string> { "Lewis Carol" },
                ["S3Key"] = "06. Alice Adventures in Wonderland author Lewis Carrol.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/aliceinwonderland.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1

            };
            
            await booksTable.PutItemAsync(newBook1);

            Document newBook2 = new Document
            {
                ["ISBN"] = "9780156012195",
                ["Title"] = "The Little Prince",
                ["Authors"] = new List<string> { "Antoine de Saint-Exupéry", },
                ["S3Key"] = "07. The Little Prince author Antoine de Saint-Exupéry.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/littleprince.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1


            };



            await booksTable.PutItemAsync(newBook2);


            Document newBook3 = new Document
            {
                ["Title"] = "The voyages and travels of Sindbad the Sailor",
                ["ISBN"] = "9780282405236",
                ["Authors"] = new List<string> { "Thomas Richardson" },
                ["S3Key"] = "16. Jack and the Beanstalk author Joseph Jacobs.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/voyagessinbad.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1

            };

            await booksTable.PutItemAsync(newBook3);


            Document newBook4 = new Document
            {
                ["Title"] = "Jack and the Beanstalk",
                ["ISBN"] = "9781909115637",
                ["Authors"] = new List<string> { "Joseph Jacobs", },
                ["S3Key"] = "16. Jack and the Beanstalk author Joseph Jacobs.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/jackandbeanstalk.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1

            };

            await booksTable.PutItemAsync(newBook4);


            Document newBook5 = new Document
            {
                ["Title"] = "The Juniper-Tree",
                ["ISBN"] = "9780374339715",
                ["Authors"] = new List<string> { "Miss Mulock" },
                ["S3Key"] = "17. The Juniper-Tree author Miss Mulock.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/junipertree.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1

            };
            await booksTable.PutItemAsync(newBook5);

            Document newBook6 = new Document
            {
                ["Title"] = "The brave tin soldier",
                ["ISBN"] = "9781848775114",
                ["Authors"] = new List<string> { "Hans Christian" },
                ["S3Key"] = "21. The brave tin soldier author Hans Christian Andersen.pdf",
                ["ImageUrl"] = "https://bucket4sec002-tluzon.s3.ca-central-1.amazonaws.com/bravetin.png",
                ["LastOpened"] = "1996-03-07T15:07:00",
                ["PageCount"] = 1,
                ["LastPageOpened"] = 1


            };

            await booksTable.PutItemAsync(newBook6);

        }


        public static async Task DeleteExistingTables(IAmazonDynamoDB client)
        {
            Console.WriteLine("Deleting Users table");
            var deleteTable = await DeleteTable(client, "Users");

            if (deleteTable is not null && deleteTable.HttpStatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Could not delete Users table");
            }
        }

        public static async Task DeleteBookshelfTable(IAmazonDynamoDB client)
        {
            Console.WriteLine("Deleting Bookshelf table");
            var deleteTable = await DeleteTable(client, "Bookshelf");

            if (deleteTable is not null && deleteTable.HttpStatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Could not delete Users table");
            }
        }

        public static async Task CreateDataTables(IAmazonDynamoDB client)
        {
            Console.WriteLine("Creating Users table");
            var createTableResponse = await CreateTableUsers(client);

            Console.WriteLine("The status of the Users table is " + createTableResponse.Table.TableStatus);
            Console.WriteLine();
            Console.WriteLine("Press ENTER to continue");
            Console.ReadLine();
        }

        private static async Task InsertTableData(AmazonDynamoDBClient client)
        {
            Console.WriteLine("Loading data into Users table");
            await InsertSampleUsers(client);
        }

        public DDBOperations()
        {
            credentials = new BasicAWSCredentials(
                ConfigurationManager.AppSettings["accessId"],
                ConfigurationManager.AppSettings["secretKey"]);
            client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.CACentral1);
            context = new DynamoDBContext(client);
        }

    }
}
