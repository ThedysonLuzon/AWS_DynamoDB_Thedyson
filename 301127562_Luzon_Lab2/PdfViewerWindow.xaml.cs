using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Intrinsics.X86;
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
using Amazon;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Windows.PdfViewer;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using Application = System.Windows.Application;
using File = System.IO.File;
using Table = Amazon.DynamoDBv2.DocumentModel.Table;

namespace _301127562_Luzon_Lab2
{
    /// <summary>
    /// Thedyson Luzon - Centennial College F2023
    /// Interaction logic for PdfViewerWindow.xaml
    /// </summary>
    public partial class PdfViewerWindow : Window
    {
        public string awsBucketName = "bucket4sec002-tluzon";
        public Book book;
        public BookshelfWindow bookshelfWindow;
        public string userName;
        public DDBOperations ddbOperations;
        public Book selectedBook;

        public PdfViewerWindow(Book book, string userName)
        {
            InitializeComponent();
            var bookS3key = book.S3Key;
            this.book = book;
            LoadPdfFromS3IntoViewer(awsBucketName, bookS3key);
            this.userName = userName;

            Closing += Window_Closing;

            selectedBook = book;
            //Debug.WriteLine($"selectedBook = {selectedBook}");

            //LoadPdfFromS3IntoViewer("bucket4sec002-tluzon", "06. Alice Adventures in Wonderland author Lewis Carrol.pdf");
            ddbOperations = new DDBOperations();
        }

        public async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Debug.WriteLine("Window_Closing event from pdfviewerwindow triggered.");
            try
            {
                // Calculate the LastPageOpened based on the CurrentPageIndex
                int lastPageOpened = pdfViewer.CurrentPageIndex;

                // Update the LastPageOpened and LastOpened for the selected book
                selectedBook.LastPageOpened = lastPageOpened;
                selectedBook.LastOpened = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss"); // Update the LastOpened timestamp
                selectedBook.PageCount = pdfViewer.PageCount;

                await ddbOperations.UpdateUserBooksAsync(userName, selectedBook.ISBN, selectedBook.LastPageOpened, selectedBook.LastOpened, selectedBook.PageCount);
                //Debug.WriteLine($"Updating book: ISBN={selectedBook.ISBN}, LastPageOpened={selectedBook.LastPageOpened}, LastOpened={selectedBook.LastOpened}");
                //Debug.WriteLine("Update successful from window closing.");

                //Take back to bookshelf window
                BookshelfWindow bookshelfWindow = new(userName);
                bookshelfWindow.Show();
                this.Close();


            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Error updating LastPageOpened and LastOpened attributes: {ex.Message}");
                MessageBox.Show($"Error updating LastPageOpened and LastOpened attributes: {ex.Message}");
            }
        }


        public void LoadPdfFromS3IntoViewer(string bucketName, string key)
        {
            try
            {
                using (Helper.GetS3Client());
                {
                    var request = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key
                    };

                    var response = Helper.s3Client.GetObjectAsync(request).Result;
                    var stream = response.ResponseStream;
                    var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);

                    byte[] pdfBytes = memoryStream.ToArray();

                    PdfLoadedDocument loadedDocument = new PdfLoadedDocument(pdfBytes);
                    pdfViewer.Load(loadedDocument);

                    // Check if the book has a valid LastPageOpened value
                    if (book.LastPageOpened > 0 && book.LastPageOpened <= loadedDocument.Pages.Count)
                    {
                        // Set the desired page number based on the LastPageOpened
                        pdfViewer.CurrentPage = book.LastPageOpened ; 
                    }
                    else
                    {
                        // Set the desired page number to the default
                        pdfViewer.CurrentPage = book.LastPageOpened;
                    }

                    int pageCount = pdfViewer.PageCount;
                    book.PageCount = pageCount;
                    int pageNumber = pdfViewer.CurrentPageIndex;

                    //Debug.WriteLine($"PageCount={book.PageCount}, lastpageopened={book.LastPageOpened}, pagenumber={pageNumber}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading PDF from S3: {ex.Message}");
            }
        }

        private async void Btn_Bookmark_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Calculate the LastPageOpened based on the CurrentPageIndex
                int lastPageOpened = pdfViewer.CurrentPageIndex; 

                // Update the LastPageOpened and LastOpened for the selected book
                selectedBook.LastPageOpened = lastPageOpened;
                selectedBook.LastOpened = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss"); // Update the LastOpened timestamp
                selectedBook.PageCount = pdfViewer.PageCount;

                await ddbOperations.UpdateUserBooksAsync(userName, selectedBook.ISBN, selectedBook.LastPageOpened, selectedBook.LastOpened, selectedBook.PageCount);
                //Debug.WriteLine($"Updating book: ISBN={selectedBook.ISBN}, LastPageOpened={selectedBook.LastPageOpened}, LastOpened={selectedBook.LastOpened}");
                //Debug.WriteLine("Update successful from bookmark.");

                MessageBox.Show($"Bookmarked Book:{selectedBook.Title} on Page: {selectedBook.LastPageOpened} on {selectedBook.LastOpened} for {userName}.");
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Error updating LastPageOpened and LastOpened attributes: {ex.Message}");
                MessageBox.Show($"Error updating LastPageOpened and LastOpened attributes: {ex.Message}");
            }
        }
    }
}
