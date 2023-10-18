using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement.Internal;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _301127562_Luzon_Lab2
{
    /// <summary>
    /// Thedyson Luzon - Centennial College F2023
    /// Helper.cs
    /// </summary>
    internal class Helper
    {
        public readonly static IAmazonS3 s3Client;
        public readonly static IAmazonDynamoDB dbClient;

        static Helper()
        {
            s3Client = GetS3Client();
            dbClient = GetDBClient();
        }

        public static IAmazonS3 GetS3Client()
        {
            string awsAccessKey = ConfigurationManager.AppSettings["accessId"];
            string awsSecretKey = ConfigurationManager.AppSettings["secretKey"];
            return new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.CACentral1);
        }

        public static IAmazonDynamoDB GetDBClient()
        {
            string awsAccessKey = ConfigurationManager.AppSettings["accessId"];
            string awsSecretKey = ConfigurationManager.AppSettings["secretKey"];
            return new AmazonDynamoDBClient(awsAccessKey, awsSecretKey, RegionEndpoint.CACentral1);
        }

    }
}
