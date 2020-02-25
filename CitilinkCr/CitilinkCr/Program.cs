using System;
using MySqlUtils;

namespace CitilinkCr
{
    class Program
    {
        static void Main(string[] args)
        {
            MySqlConfig config = new MySqlConfig()
            {
                Host = "89.208.196.51",
                Port = 3306,
                UserId = "root",
                Password = "admin1234",
                SslMode = "none",
                Database = "citilink_base",
                CharacterSet = "utf8"
            };
            var mariaDbWrapper = new MySqlWrap(config);



            var crawlerWorker = new CrawlerWorker(mariaDbWrapper);
            //worker.ParseAndSaveCategories();
            //worker.StartCrawlingGoods();

            var productsParseWorker = new ProductsParseWorker(mariaDbWrapper);
            productsParseWorker.Run();


            Console.Read();
        }
    }
}
