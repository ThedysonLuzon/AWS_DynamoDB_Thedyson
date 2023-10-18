using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Table = Amazon.DynamoDBv2.DocumentModel.Table;

namespace _301127562_Luzon_Lab2
{
    /// <summary>
    /// Thedyson Luzon - Centennial College F2023
    /// Interaction logic for BookshelfWindow.xaml
    /// </summary>
    public partial class BookshelfWindow : Window
    {
        MainWindow mainWindow;
        ObservableCollection<User> users = new ObservableCollection<User>();
        public string loggedIn;
        public string LastOpened;
        private string username;

        public BookshelfWindow(string username)
        {
            InitializeComponent();

            Lbl_User.Content = $"Welcome, {username}!";
            loggedIn = username;

            //Display all books
            // PopulateBookshelfListView();

            //Display loggedIn user bookshelf
            _ = PopulateBookListView();
        }


        // Display loggedIn Bookshelf
        public async Task PopulateBookListView()
        {
            Table table = Table.LoadTable(Helper.dbClient, "Users");

            QueryFilter filter = new QueryFilter();
            filter.AddCondition("UserName", QueryOperator.Equal, loggedIn);

            QueryOperationConfig config = new QueryOperationConfig
            {
                Filter = filter,
                ConsistentRead = true
            };

            Search search = table.Query(config);
            List<Document> results = await search.GetRemainingAsync();

            // Create a list of User objects to store user data and books
            List<User> users = new List<User>();

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
                        }) ;
                    }
                }
                users.Add(user);
            }
            //sort user's books
            foreach (var user in users)
            {
                user.Books = user.Books.OrderByDescending(b => b.LastOpened).ToList();
            }
            bookListView.DataContext = null;
            bookListView.DataContext = users;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Btn_Logout_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void bookListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (bookListView.SelectedItem != null)
            {
                Book selectedBook = (Book)bookListView.SelectedItem;
                //string bookS3key = selectedBook.S3Key;
                //Application.Current.Properties["S3Key"] = bookS3key;
                var loggedIn = this.loggedIn;
                PdfViewerWindow pdfViewerWindow = new(selectedBook, loggedIn);
                pdfViewerWindow.Show();
                this.Hide();
            }
        }

        /*//Display all books in Bookshelf
        public void PopulateBookshelfListView()
        {
            //
            var booksTable = Table.LoadTable(Helper.dbClient, "Bookshelf");
            var books = booksTable.Scan(new ScanFilter());


            foreach (var document in books.GetRemainingAsync().Result)
            {
                var authors = document["Authors"].AsListOfString();
                var authorString = string.Join(",", authors);

                var imageUrl = document["ImageUrl"].AsString();

                bookListView.Items.Add(new
                {
                    ISBN = document["ISBN"].AsString(),
                    Title = document["Title"].AsString(),
                    Authors = authorString,
                    ImageUrl = imageUrl
                });
            }
        }*/
    }
}
