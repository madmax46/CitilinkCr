using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using CitilinkCr.Classes;
using MySqlUtils;
using WebUtils;

namespace CitilinkCr
{
    public class ProductsParseWorker
    {

        private readonly MySqlWrap mySqlConnection;
        //<table class="product_features">(.*?)<\/table>

        private readonly Regex regexTableCharacteristics;
        private readonly Regex regexTr;


        public ProductsParseWorker(MySqlWrap mySqlConnection)
        {
            this.mySqlConnection = mySqlConnection;
            regexTableCharacteristics = new Regex("<table\\s*?class=\"product_features\"\\s*?>(?<rows>.*?)</table>", RegexOptions.Singleline | RegexOptions.Compiled);
            regexTr = new Regex("<tr.+?/tr>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }



        public void Run()
        {
            var products = LoadNotParsedProductsFromDb();

            ParseProducts(products);
        }



        private void ParseProducts(List<CitilinkProduct> products)
        {
            foreach (var citilinkProduct in products)
            {
                try
                {
                    ParseOneProduct(citilinkProduct);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }


        private void ParseOneProduct(CitilinkProduct product)
        {
            var request = new HttpRequestParameters()
            {
                RequestUri = product.Link
            };

            var page = Web.GetDocumentAsHttpClient(request);


            var tableCharacteristics = regexTableCharacteristics.Match(page);
            if (!tableCharacteristics.Success)
                return;

            var allTr = regexTr.Matches(tableCharacteristics.Value);


            foreach (Match match in allTr)
            {

                if (match.Value.Contains("header_row"))
                {

                }

            }
        }

        private List<CitilinkProduct> LoadNotParsedProductsFromDb()
        {

            var query = "SELECT * FROM citilink_base.products_link where status != 2";

            var table = mySqlConnection.GetDataTable(query);
            var products = new List<CitilinkProduct>();


            foreach (DataRow oneRow in table.Rows)
            {
                var oneProduct = new CitilinkProduct()
                {
                    Id = Convert.ToUInt32(oneRow["id"]),
                    CategoryProduct = Convert.ToUInt32(oneRow["idCategory"]),
                    Link = Convert.ToString(oneRow["link"]),
                    Name = Convert.ToString(oneRow["name"]),
                    Status = Convert.ToUInt32(oneRow["status"]),
                    LastUpdate = Convert.ToDateTime(oneRow["last_update"]),
                    CitilinkProductId = Convert.ToUInt32(oneRow["citilink_product_id"]),
                };

                products.Add(oneProduct);
            }

            return products;
        }
    }
}
