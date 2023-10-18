using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Table = Amazon.DynamoDBv2.DocumentModel.Table;

namespace _301127562_Luzon_Lab2
{
    /// <summary>
    /// Thedyson Luzon - Centennial College F2023
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //User DynamoDB
            //_ = DDBOperations.CreateTableUsers(Helper.dbClient);
            //_ = DDBOperations.InsertSampleUsers(Helper.dbClient);
            //_ = DDBOperations.DeleteExistingTables(Helper.dbClient);
            //_ = DDBOperations.DeleteTable(Helper.dbClient, "Users");

            //Bookshelf DynamoDB
            //_ = DDBOperations.CreateTableBookshelf(Helper.dbClient);
            //_ = DDBOperations.InsertSampleBooks(Helper.dbClient);
            //_ = DDBOperations.DeleteBookshelfTable(Helper.dbClient);
            //_ = DDBOperations.DeleteTable(Helper.dbClient, "BookShelf");
        }

        public async void Btn_Login_Click(object sender, RoutedEventArgs e)
        {
           bool userValid = await ValidateUserLoginAsync(Tb_Username.Text, Tb_Password.Password);
            if (Tb_Username.Text != string.Empty && Tb_Password.Password != string.Empty)
            {
                if (userValid == true)
                {
                    var username = Tb_Username.Text;
                    Application.Current.Properties["Username"] = username;
                    BookshelfWindow bookshelfWindow = new BookshelfWindow(username);
                    bookshelfWindow.Show();

                   /* PdfViewerWindow pdfViewerWindow = new PdfViewerWindow();
                    pdfViewerWindow.Show();*/
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Inccorect username/password.");
                }
            }
            else
            {
                MessageBox.Show("Username and/or Password are empty.");
            }
        }

        public async Task<bool> ValidateUserLoginAsync(string username, string password)
        {
            var tableName = "Users"; 

            var queryRequest = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "UserName = :val",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":val", new AttributeValue { S = username } }
        },
                ProjectionExpression = "Password" 
            };

            try
            {
                var response = await Helper.dbClient.QueryAsync(queryRequest);

                if (response.Items.Count > 0)
                {
                    var storedPassword = response.Items[0]["Password"].S;

                    bool isPasswordValid = VerifyPassword(password, storedPassword);

                    return isPasswordValid;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        // Implement a secure password verification method
        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            // Implement password hashing and verification logic here (if there's time)

            // Return bool
            return inputPassword == storedPassword;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

    }
}




