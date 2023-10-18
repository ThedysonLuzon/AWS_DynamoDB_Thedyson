using Amazon.DynamoDBv2.DataModel;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _301127562_Luzon_Lab2
{
    /// <summary>
    /// Thedyson Luzon - Centennial College F2023
    /// Book.cs
    /// </summary>
    public class Book
    {
        [DynamoDBHashKey]
        public string ISBN { get; set; }
        public string Title { get; set; }

        [DynamoDBProperty("Authors")]
        public string Authors { get; set; }

        public string S3Key { get; set; }
        public string ImageUrl { get; set; }

        public string LastOpened { get; set; }
        
        public int LastPageOpened { get; set; }
        public int PageCount { get; set; }



    }
}
