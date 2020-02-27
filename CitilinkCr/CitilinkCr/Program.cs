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
                Host = "xxx",
                Port = 3306,
                UserId = "IAS14.lyamoPV",
                Password = "xxx",
                SslMode = "none",
                Database = "IAS14_lyamoPV",
                CharacterSet = "utf8"
            };
            var dbWrapper = new MySqlWrap(config);


            Console.WriteLine($"Запустить краулер ссылок - 1, запустить парсер товаров - 2");
            var decision = Console.ReadLine();

            while (decision != "exit")
            {
                switch (decision)
                {
                    case "1":
                        var crawlerWorker = new CrawlerWorker(dbWrapper);
                        crawlerWorker.ParseAndSaveCategories();
                        crawlerWorker.StartCrawlingGoods();
                        break;

                    case "2":
                        var productsParseWorker = new ProductsParseWorker(dbWrapper);
                        productsParseWorker.Run();
                        break;

                    default: break;
                }

                Console.WriteLine($"Запустить краулер ссылок - 1, запустить парсер товаров - 2, выйти - exit");
                decision = Console.ReadLine();
            }


        }
    }
}
