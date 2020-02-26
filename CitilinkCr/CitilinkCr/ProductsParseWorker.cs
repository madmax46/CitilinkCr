using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        private readonly Regex regexClearHttpTags;
        private readonly Regex characteristicParser;


        private Dictionary<string, uint> characteristicsGroup;
        public ProductsParseWorker(MySqlWrap mySqlConnection)
        {
            this.mySqlConnection = mySqlConnection;
            regexTableCharacteristics = new Regex("<table\\s*?class=\"product_features\"\\s*?>(?<rows>.*?)</table>", RegexOptions.Singleline | RegexOptions.Compiled);
            regexTr = new Regex("<tr.+?/tr>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            regexClearHttpTags = new Regex("<[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            characteristicParser = new Regex("property_name.+?>(?<key>.+?)</span>.+?<td>(?<value>.+?)<", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

            characteristicsGroup = new Dictionary<string, uint>();
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
                    var characteristics = ParseOneProductCharacteristics(citilinkProduct);


                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }


        private List<ProductParsedCharacteristic> ParseOneProductCharacteristics(CitilinkProduct product)
        {
            var characteristics = new List<ProductParsedCharacteristic>();

            var request = new HttpRequestParameters()
            {
                RequestUri = product.Link
            };

            var page = Web.GetDocumentAsHttpClient(request);


            var tableCharacteristics = regexTableCharacteristics.Match(page);
            if (!tableCharacteristics.Success)
                return characteristics;

            var allTr = regexTr.Matches(tableCharacteristics.Value);



            var currentHeader = string.Empty;
            foreach (Match match in allTr)
            {

                if (match.Value.Contains("header_row"))
                {
                    currentHeader = regexClearHttpTags.Replace(match.Value, "").TrimStart().TrimEnd();
                }
                else
                {
                    var characteristicsMatch = characteristicParser.Match(match.Value);
                    if (!characteristicsMatch.Success)
                        continue;

                    var characteristic = new ProductParsedCharacteristic()
                    {
                        CharacteristicGroup = currentHeader,
                        Name = System.Web.HttpUtility.HtmlDecode(characteristicsMatch.Groups["key"].Value),
                        Value = System.Web.HttpUtility.HtmlDecode(characteristicsMatch.Groups["value"].Value)
                    };
                    characteristics.Add(characteristic);
                }
            }

            return characteristics;
        }


        private void SaveCharacteristicsToDb(List<ProductParsedCharacteristic> characteristics)
        {
            var groupsSaveToDb = new List<string>();
            foreach (var characteristic in characteristics)
            {
                if (!characteristicsGroup.ContainsKey(characteristic.CharacteristicGroup))
                    groupsSaveToDb.Add(characteristic.CharacteristicGroup);
            }

            var groupsValues = string.Join(",",
                groupsSaveToDb.Select(r => $"({MySqlWrap.ToMySqlParameters(r)})"));
            var groupsQuery = $"insert ignore into characteristics_groups (name) values {groupsValues} ;";



            var characteristicsValues = string.Join(",",
                characteristics.Select(r => $"({MySqlWrap.ToMySqlParameters(r.Name)})"));
            var characteristicsQuery = $"insert ignore into characteristics (idGroup,name) values {groupsValues} ;";

            mySqlConnection.Execute(groupsQuery);


        }

        private Dictionary<string, uint> LoadCharacteristicsGroupFromDb()
        {
            var query = $"select * from characteristics_groups";
            var table = mySqlConnection.GetDataTable(query);

            var dict = new Dictionary<string, uint>();
            foreach (DataRow tableRow in table.Rows)
            {
                var id = Convert.ToUInt32(tableRow["id"]);
                var name = Convert.ToString(tableRow["name"]);
                dict[name] = id;
            }

            return dict;
        }


        private Dictionary<string, uint> LoadCharacteristicsDb()
        {
            var query = $"select * from characteristics";
            var table = mySqlConnection.GetDataTable(query);

            var dict = new Dictionary<string, uint>();
            foreach (DataRow tableRow in table.Rows)
            {
                var id = Convert.ToUInt32(tableRow["id"]);
                var name = Convert.ToString(tableRow["name"]);
                dict[name] = id;
            }

            return dict;
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
