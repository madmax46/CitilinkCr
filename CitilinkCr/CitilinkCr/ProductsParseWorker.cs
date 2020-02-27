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
        private readonly Regex regExCharacteristic;
        private readonly Regex regexLastSeen;
        private readonly Regex regexPrice;
        private readonly Regex regexPriceInsNum;


        private Dictionary<string, uint> characteristicsGroup;
        private Dictionary<Characteristic, uint> characteristicsDict;
        public ProductsParseWorker(MySqlWrap mySqlConnection)
        {
            this.mySqlConnection = mySqlConnection;
            regexTableCharacteristics = new Regex("<table\\s*?class=\"product_features\"\\s*?>(?<rows>.*?)</table>", RegexOptions.Singleline | RegexOptions.Compiled);
            regexTr = new Regex("<tr.+?/tr>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            regexClearHttpTags = new Regex("<[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            regExCharacteristic = new Regex("property_name.+?>(?<key>.+?)</span>.+?<td>(?<value>.+?)<", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            regexLastSeen = new Regex("<div class=\"line-block standart_price out-of-stock_js\" data-last-seen=\"(?<lastseen>.*?)\".*?</div>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            regexPrice = new Regex("<div class=\"product-sidebar__line-box standart_price\">(?<price>.*?)</div>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            regexPriceInsNum = new Regex("<ins class=\"num\">(?<priceNum>.*?)</ins>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

            characteristicsGroup = new Dictionary<string, uint>();
            characteristicsDict = new Dictionary<Characteristic, uint>();
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
                Console.WriteLine($"Начинаю парсить характеристики для {citilinkProduct.Name} {citilinkProduct.Link}");

                try
                {
                    var page = LoadProductPage(citilinkProduct);

                    if (string.IsNullOrEmpty(page))
                    {
                        for (int i = 2; i <= 6; i++)
                        {
                            Console.WriteLine($"пришла пустая страница подождем {30000 * i}ms  {citilinkProduct.Link}");
                            System.Threading.Thread.Sleep(30000 * i);
                            page = LoadProductPage(citilinkProduct);

                            if (string.IsNullOrEmpty(page))
                            {
                                Console.WriteLine($"{i} попытка загрузки страницы {citilinkProduct.Link}");
                            }
                            else
                                break;
                        }
                    }

                    var characteristics = ParseOneProductCharacteristics(citilinkProduct, page);

                    Console.WriteLine($"Удалось достать {characteristics.Count} характеристик для {citilinkProduct.Name}");

                    //if (characteristics.Count == 0)
                    //{

                    //}

                    SaveCharacteristicsToDb(characteristics);

                    ParseAndSavePriceHistory(citilinkProduct, page);

                    citilinkProduct.Status = 2;
                    SaveParsedStatusToProduct(citilinkProduct);

                    Console.WriteLine($"Распарсил характеристики для {citilinkProduct.Name} {citilinkProduct.Link}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    citilinkProduct.Status = 3;
                    try
                    {
                        SaveParsedStatusToProduct(citilinkProduct);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }

                }
            }
        }

        private void ParseAndSavePriceHistory(CitilinkProduct citilinkProduct, string page)
        {
            var productPriceHistoryItem = GetPriceValue(citilinkProduct, page);

            SavePriceHistoryValue(productPriceHistoryItem);
        }

        private void SavePriceHistoryValue(ProductPriceHistoryItem item)
        {
            var query =
                $"insert into products_price_history (productId, price, isInStock, dateCheck) values ({item.ProductId},{MySqlWrap.ToMySqlParameters(item.Price)},{MySqlWrap.ToMySqlParameters(item.IsInStock)},{MySqlWrap.ToMySqlParameters(item.DateCheck)})";

            mySqlConnection.Execute(query);
        }

        private ProductPriceHistoryItem GetPriceValue(CitilinkProduct citilinkProduct, string page)
        {
            var productPriceHistoryItem = new ProductPriceHistoryItem()
            { ProductId = citilinkProduct.Id, DateCheck = DateTime.Now };
            var notContainsPrice = regexLastSeen.Match(page);
            if (notContainsPrice.Success)
            {
                productPriceHistoryItem.IsInStock = false;
                return productPriceHistoryItem;
            }

            var priceDiv = regexPrice.Match(page);
            if (priceDiv.Success)
            {
                var ins = regexPriceInsNum.Match(priceDiv.Groups["price"].Value);

                if (ins.Success)
                {
                    var price = ins.Groups["priceNum"].Value.Replace(" ", "");

                    if (!double.TryParse(price, out var priceDouble))
                        productPriceHistoryItem.IsInStock = false;

                    productPriceHistoryItem.IsInStock = true;
                    productPriceHistoryItem.Price = priceDouble;
                }
                else
                {
                    productPriceHistoryItem.IsInStock = false;
                }
            }
            else
            {
                productPriceHistoryItem.IsInStock = false;
            }

            return productPriceHistoryItem;
        }

        private static string LoadProductPage(CitilinkProduct citilinkProduct)
        {
            var request = new HttpRequestParameters()
            {
                RequestUri = citilinkProduct.Link
            };
            //return Web.LoadTest();//103.141.138.134:17091
            return Web.GetDocumentAsHttpClient(request);
        }

        private void SaveParsedStatusToProduct(CitilinkProduct citilinkProduct)
        {
            var queryUpdate = $"update products_link set status = {citilinkProduct.Status} where id = {citilinkProduct.Id}";
            mySqlConnection.Execute(queryUpdate);
        }


        private List<CharacteristicValue> ParseOneProductCharacteristics(CitilinkProduct product, string page)
        {
            var characteristics = new List<CharacteristicValue>();

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
                    if (string.IsNullOrEmpty(currentHeader))
                        currentHeader = "Дополнительная информация";
                }
                else
                {
                    var characteristicsMatch = regExCharacteristic.Match(match.Value);
                    if (!characteristicsMatch.Success)
                        continue;

                    var characteristic = new CharacteristicValue()
                    {
                        Characteristic = new Characteristic()
                        {
                            Group = currentHeader,
                            Name = System.Web.HttpUtility.HtmlDecode(characteristicsMatch.Groups["key"].Value)
                        },
                        Value = System.Web.HttpUtility.HtmlDecode(characteristicsMatch.Groups["value"].Value),
                        ProductId = product.Id
                    };
                    characteristics.Add(characteristic);
                }
            }

            return characteristics;
        }


        private void SaveCharacteristicsToDb(List<CharacteristicValue> characteristics)
        {
            SaveAndFillCharacteristicsGroups(characteristics);
            SaveAndFillCharacteristics(characteristics);

            SaveProductCharacteristics(characteristics);
        }

        private void SaveAndFillCharacteristicsGroups(List<CharacteristicValue> values)
        {
            var groupsSaveToDb = new List<string>();
            foreach (var characteristic in values)
            {
                if (!characteristicsGroup.ContainsKey(characteristic.Characteristic.Group))
                    groupsSaveToDb.Add(characteristic.Characteristic.Group);
            }

            SaveCharacteristicsGroupToDb(groupsSaveToDb);

            var newGroups = LoadCharacteristicsGroupFromDb(groupsSaveToDb);

            foreach (var newGroup in newGroups)
            {
                characteristicsGroup[newGroup.Key] = newGroup.Value;
            }

            foreach (var characteristic in values)
            {
                if (characteristicsGroup.ContainsKey(characteristic.Characteristic.Group))
                    characteristic.Characteristic.IdGroup = characteristicsGroup[characteristic.Characteristic.Group];
                else
                {
                    Console.WriteLine($"values group {characteristic.Characteristic.Group} not contains in db");
                }
            }
        }

        private void SaveCharacteristicsGroupToDb(List<string> groupsSaveToDb)
        {
            if (!groupsSaveToDb.Any())
                return;

            var groupsValues = string.Join(",",
                groupsSaveToDb.Select(r => $"({MySqlWrap.ToMySqlParameters(r)})"));
            var groupsQuery = $"insert ignore into characteristics_groups (name) values {groupsValues} ;";

            mySqlConnection.Execute(groupsQuery);
        }

        private void SaveAndFillCharacteristics(List<CharacteristicValue> values)
        {
            var saveToDb = new List<Characteristic>();
            foreach (var characteristicValue in values)
            {
                if (!characteristicsDict.ContainsKey(characteristicValue.Characteristic))
                    saveToDb.Add(characteristicValue.Characteristic);
            }

            SaveCharacteristicsToDb(saveToDb);

            var newCharacteristics = LoadCharacteristicsDb(saveToDb);



            foreach (var newCharacteristic in newCharacteristics)
            {
                characteristicsDict[newCharacteristic.Key] = newCharacteristic.Value;
            }

            foreach (var characteristic in values)
            {
                if (characteristicsDict.ContainsKey(characteristic.Characteristic))
                    characteristic.Characteristic.Id = characteristicsDict[characteristic.Characteristic];
                else
                {
                    Console.WriteLine($"Characteristic {characteristic.Characteristic.Name} and {characteristic.Characteristic.Group} not contains in db");
                }
            }
        }

        private void SaveCharacteristicsToDb(List<Characteristic> items)
        {
            if (!items.Any())
                return;

            var insertValues = string.Join(",",
            items.Select(r => $"({r.IdGroup}, {MySqlWrap.ToMySqlParameters(r.Name)})"));
            var insertQuery = $"insert ignore into characteristics (idGroup,name) values {insertValues} ;";

            mySqlConnection.Execute(insertQuery);
        }


        private void SaveProductCharacteristics(List<CharacteristicValue> values)
        {
            var insertValues = string.Join(",",
                values.Select(r => $"({r.ProductId}, {r.Characteristic.Id}, {MySqlWrap.ToMySqlParameters(r.Value)})"));

            var query =
                $"insert ignore products_characteristics (idProduct, idCharacteristic, `value`) values {insertValues};";

            mySqlConnection.Execute(query);
        }


        private Dictionary<string, uint> LoadCharacteristicsGroupFromDb(List<string> groupNames)
        {
            if (!groupNames.Any())
                return new Dictionary<string, uint>();

            var groupsValues = string.Join(",",
                groupNames.Select(r => $"{MySqlWrap.ToMySqlParameters(r)}"));
            var groupsQuery = $"select * from characteristics_groups where name in ({groupsValues}) ;";

            var table = mySqlConnection.GetDataTable(groupsQuery);

            var dict = new Dictionary<string, uint>();

            foreach (DataRow tableRow in table.Rows)
            {
                var id = Convert.ToUInt32(tableRow["id"]);
                var name = Convert.ToString(tableRow["name"]);
                dict[name] = id;
            }

            return dict;
        }

        private Dictionary<Characteristic, uint> LoadCharacteristicsDb(List<Characteristic> items)
        {
            if (!items.Any())
                return new Dictionary<Characteristic, uint>();

            var values = string.Join("OR",
                items.Select(r => $"(c.idGroup = {r.IdGroup} and c.name = {MySqlWrap.ToMySqlParameters(r.Name)})"));

            var query = $"select c.*, cg.name as groupName from characteristics c inner join characteristics_groups cg on  c.idGroup = cg.id where {values} ;";
            var table = mySqlConnection.GetDataTable(query);

            var dict = new Dictionary<Characteristic, uint>();
            foreach (DataRow tableRow in table.Rows)
            {
                var id = Convert.ToUInt32(tableRow["id"]);
                var idGroup = Convert.ToUInt32(tableRow["idGroup"]);
                var name = Convert.ToString(tableRow["name"]);
                var groupName = Convert.ToString(tableRow["groupName"]);
                var characteristic = new Characteristic()
                {
                    Id = id,
                    Name = name,
                    IdGroup = idGroup,
                    Group = groupName
                };


                dict[characteristic] = id;
            }

            return dict;
        }


        private Dictionary<string, uint> LoadAllCharacteristicsGroupFromDb()
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
            var query = $"select * from values";
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

            var query = "SELECT * FROM products_link where status != 2";

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
