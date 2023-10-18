using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _301127562_Luzon_Lab2
{
    /// <summary>
    /// Thedyson Luzon - Centennial College F2023
    /// User.cs
    /// </summary>
    [DynamoDBTable("Users")]
    public class User
    {

        [DynamoDBHashKey("UserName")]
        public string UserName { get; set; }

        public string Email { get; set; }

        [DynamoDBProperty]
        public string? Password { get; set; }

        public List<Book> Books { get; set; }


    }

}
