using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CitilinkCr.Classes;
using MySqlUtils;
using WebUtils;

namespace CitilinkCr
{
    public class CrawlerWorker
    {

        private readonly MySqlWrap mySqlConnection;

        private readonly Regex regexCategoryCatalog;
        private readonly Regex regexCategoryName;
        private readonly Regex regexClearHttpTags;
        private readonly Regex regexHrefTag;
        private readonly Regex regexLiTag;
        private readonly Regex regexAllGoodsFromPage;
        private readonly Regex regexPagesBlock;
        private readonly Regex regexPageLinks;
        private readonly Regex regexProductAllInfo;
        private readonly Regex regexProductInfo;


        private readonly Regex regexLink;
        private readonly Regex regexTitle;
        private readonly Regex regexCititlinkProductId;



        //?available=0&sorting=name_asc&p=1

        // <ul class=\"category-catalog__item\">.*?<\/ul> 
        // <h4>(.*?)</h4> 

        public CrawlerWorker(MySqlWrap mySqlConnection)
        {
            this.mySqlConnection = mySqlConnection;


            regexCategoryCatalog = new Regex("<ul class=\"category-catalog__item\">(.*?)</ul>", RegexOptions.Singleline);
            regexCategoryName = new Regex("<h4>(.*?)</h4>", RegexOptions.Singleline);
            regexClearHttpTags = new Regex("<[^>]*>");
            regexHrefTag = new Regex("<\\s*?a\\s*?href\\s*?=\\s*?\"(.*?)\".*?>(.*?)</a>", RegexOptions.Singleline | RegexOptions.Compiled);
            regexLiTag = new Regex("<li>(.*?)</li>", RegexOptions.Singleline);
            regexAllGoodsFromPage = new Regex("class=\"block_data__gtm-js block_data__pageevents-js listing_block_data__pageevents-js\"(.*?)<div class=\"page_listing\"", RegexOptions.Singleline | RegexOptions.Compiled);
            regexPagesBlock = new Regex("<div.*?class=\"page_listing\".*?>(.*?)</div>", RegexOptions.Multiline | RegexOptions.Compiled);
            regexPageLinks = new Regex("<li.*?class\\s*?=\\s*?\"(.*?)\".*?>(.*?)</li>", RegexOptions.Singleline | RegexOptions.Compiled);
            regexProductAllInfo = new Regex("<div class=\"product_name cms_item_panel subcategory-product-item__info\">(?<prod>.*?)<div class=\"subcategory-product-item__footer\">", RegexOptions.Singleline | RegexOptions.Compiled);
            regexProductInfo = new Regex("<a.*?class=\"link_gtm-js link_pageevents-js ddl_product_link\".*?(.*?).*?>", RegexOptions.Singleline | RegexOptions.Compiled);

            regexLink = new Regex("href=\"(?<link>.*?)\"", RegexOptions.Singleline | RegexOptions.Compiled);
            regexTitle = new Regex("title=\"(?<title>.*?)\"", RegexOptions.Singleline | RegexOptions.Compiled);
            regexCititlinkProductId = new Regex("data-product-id=\"(?<id>.*?)\"", RegexOptions.Singleline | RegexOptions.Compiled);


            //regexPageLinks = //<div class="product_name cms_item_panel subcategory-product-item__info">(?<prod>.*?)<div class="subcategory-product-item__footer">
            //var pagesPattern = "<li.*?class\\s*?=\\s*?\"(.*?)\".*?>(.*?)</li>";
            //regexCurrentPage = new Regex("<div.*?class=\"page_listing\".*?>.*?</div>", RegexOptions.Singleline | RegexOptions.Compiled);
            //regexLastPage = new Regex("<div.*?class=\"page_listing\".*?>.*?</div>", RegexOptions.Singleline | RegexOptions.Compiled);





            //ParseAndSaveCategories();
        }


        public void ParseAndSaveCategories()
        {
            var categories = ParseCitilinkCategories();

            SaveCategories(categories);
        }



        public void StartCrawlingGoods()
        {
            var categories = GetCategoriesFromDb();

            ParseAllGoodsFromCategories(categories);
        }



        private List<CitilinkCategoryGroup> ParseCitilinkCategories()
        {
            var request = new HttpRequestParameters()
            {
                RequestUri = "https://www.citilink.ru/catalog/"
            };


            var categoriesPage = Web.GetDocumentAsHttpClient(request);

            var categories = ParseCategoriesFromPage(categoriesPage);

            var categoriesList = new List<CitilinkCategoryGroup>();

            foreach (var oneCategory in categories)
            {
                var categoryName = regexCategoryName.Match(oneCategory);
                if (!categoryName.Success)
                    continue;



                var (isSuccess, tag) = ParseHrefTag(categoryName.Groups[1].Value);

                if (!isSuccess)
                    continue;


                var subCategories = ParseSubCategories(oneCategory);
                var cat = new CitilinkCategoryGroup(tag.Name, tag) { Categories = subCategories };

                categoriesList.Add(cat);
            }
            return categoriesList;
        }



        private void ParseAllGoodsFromCategories(List<CitilinkCategory> categories)
        {

            foreach (var oneCategory in categories)
            {
                ParseGoodsFromCategory(oneCategory);
            }
        }

        private void ParseGoodsFromCategory(CitilinkCategory oneCategory)
        {
            Console.WriteLine($"Проверяем категорию {oneCategory.Id} {oneCategory.Name}");
            while (oneCategory.LastParsedPage <= oneCategory.PagesNumInCategory || oneCategory.LastParsedPage == 0)
            {
                var pageRequest = oneCategory.LastParsedPage != oneCategory.PagesNumInCategory
                    ? oneCategory.LastParsedPage + 1
                    : oneCategory.LastParsedPage;
                var request = new HttpRequestParameters()
                {
                    RequestUri = $"{oneCategory.Link}?action=space=msk_cl&available=0&sorting=name_asc&p={pageRequest}",
                    Referrer = "https://www.citilink.ru/"
                };

                var pageText = Web.GetDocumentAsHttpClient(request);

                var pagesBlock = regexPagesBlock.Match(pageText);
                var pagesNum = regexPageLinks.Matches(pagesBlock.Value).Cast<Match>().ToList();
                var currentPage = pagesNum.FirstOrDefault(r => r.Groups[1].Value == "selected");
                var lastPage = pagesNum.FirstOrDefault(r => r.Groups[1].Value == "last");

                if (lastPage == null)
                {
                    var nextPages = pagesNum.Where(r => r.Groups[1].Value == "next");
                    if (nextPages.Any())
                        lastPage = pagesNum.LastOrDefault();
                    else
                        lastPage = currentPage;
                }

                if (currentPage == null || lastPage == null)
                    throw new Exception("currentPage == null || lastPage == null");

                var clearedSelected = Convert.ToUInt32(regexClearHttpTags.Replace(currentPage.Value, "").TrimStart().TrimEnd());
                var clearedLast = Convert.ToUInt32(regexClearHttpTags.Replace(lastPage.Value, "").TrimStart().TrimEnd());


                if (clearedLast != oneCategory.PagesNumInCategory)
                {
                    oneCategory.LastParsedPage = 0;
                    oneCategory.PagesNumInCategory = clearedLast;
                    UpdatePagesInfoInCategory(oneCategory);

                    if (clearedSelected != 1)
                        continue;
                }

                oneCategory.ParseStatus = 1;
                UpdateCategoryCrawlerStatus(oneCategory);


                if (oneCategory.PagesNumInCategory == clearedSelected && oneCategory.LastParsedPage == oneCategory.PagesNumInCategory)
                    break;


                var links = ParseGoodsLinks(pageText);
                links.ForEach((r) => r.CategoryProduct = oneCategory.Id);

                SaveNewProductsLinkToDb(links);


                oneCategory.LastParsedPage = clearedSelected;
                UpdatePagesInfoInCategory(oneCategory);
                Console.WriteLine($"Сохранили {links.Count} товаров на {clearedSelected} странице из категории {oneCategory.Id} {oneCategory.Name}");
            }

            oneCategory.ParseStatus = oneCategory.PagesNumInCategory == oneCategory.LastParsedPage ? (uint)2 : (uint)3;
            UpdateCategoryCrawlerStatus(oneCategory);

            Console.WriteLine($"Закончили собирать ссылки из категории {oneCategory.Id} {oneCategory.Name}");
        }

        private List<CitilinkProduct> ParseGoodsLinks(string page)
        {
            var allGoods = regexAllGoodsFromPage.Match(page);

            var goodsMatches = regexProductAllInfo.Matches(page);

            var products = new List<CitilinkProduct>();

            foreach (Match oneProduct in goodsMatches)
            {
                var refToProduct = regexProductInfo.Match(oneProduct.Value);

                if (!refToProduct.Success)
                    continue;

                var productLink = new CitilinkProduct();

                var link = regexLink.Match(refToProduct.Value);
                if (link.Success)
                    productLink.Link = link.Groups["link"].Value;
                var title = regexTitle.Match(refToProduct.Value);
                if (title.Success)
                {
                    productLink.Name = HttpUtility.HtmlDecode(title.Groups["title"].Value);
                    for (int i = 0; i < 3; i++)
                        productLink.Name = HttpUtility.HtmlDecode(productLink.Name);
                }
                var citilinkProductId = regexCititlinkProductId.Match(refToProduct.Value);
                if (citilinkProductId.Success)
                    productLink.CitilinkProductId = Convert.ToUInt32(citilinkProductId.Groups["id"].Value);

                if (!productLink.Link.Contains("promo"))
                    products.Add(productLink);
            }

            return products;
        }

        private List<string> ParseCategoriesFromPage(string page)
        {
            var matches = regexCategoryCatalog.Matches(page).Cast<Match>().ToList();
            var matchesList = matches.Select(r => r.Groups.Count > 1 ? r.Groups[1].Value : r.Value).ToList();
            return matchesList;
        }

        private List<HrefTag> ParseSubCategories(string page)
        {
            var list = new List<HrefTag>();

            var matches = regexLiTag.Matches(page);

            foreach (Match oneMatch in matches)
            {
                var (isSuccess, tag) = ParseHrefTag(oneMatch.Groups[1].Value);

                if (!isSuccess)
                    continue;

                list.Add(tag);
            }

            return list;
        }

        private (bool isSuccess, HrefTag tag) ParseHrefTag(string htmlTags)
        {
            var match = regexHrefTag.Match(htmlTags);

            if (!match.Success)
                return (false, null);


            return (true, new HrefTag(match.Groups[1].Value, regexClearHttpTags.Replace(match.Groups[2].Value, "").TrimStart().TrimEnd()));
        }



        private void SaveCategories(List<CitilinkCategoryGroup> categories)
        {
            var subcategoriesAll = categories.SelectMany(r => r.Categories).ToList();
            var values = string.Join(", ",
                subcategoriesAll.Select(r =>
                    $"({MySqlWrap.ToMySqlParameters(r.Name)},{MySqlWrap.ToMySqlParameters(r.Href)})"));
            var insertQuery = $"INSERT ignore into categories (name, link) VALUES {values}";

            mySqlConnection.Execute(insertQuery);
        }


        private List<CitilinkCategory> GetCategoriesFromDb()
        {
            var query =
                $"SELECT c.id, c.name,c.link, c.parse_status, c.last_update, c.pages_num, c.last_parsed_page FROM categories c";

            var table = mySqlConnection.GetDataTable(query);

            var categories = new List<CitilinkCategory>();
            foreach (DataRow oneRow in table.Rows)
            {
                var category = new CitilinkCategory
                {
                    Id = Convert.ToUInt32(oneRow["id"]),
                    Name = Convert.ToString(oneRow["name"]),
                    Link = Convert.ToString(oneRow["link"]),
                    ParseStatus = Convert.ToUInt32(oneRow["parse_status"]),
                    PagesNumInCategory = Convert.ToUInt32(oneRow["pages_num"]),
                    LastParsedPage = Convert.ToUInt32(oneRow["last_parsed_page"]),
                    LastUpdateDt = Convert.ToDateTime(oneRow["last_update"])
                };

                categories.Add(category);
            }

            return categories;
        }


        private void UpdatePagesInfoInCategory(CitilinkCategory category)
        {
            var queryUpdate =
                $"UPDATE categories SET last_parsed_page = {category.LastParsedPage}, pages_num = {category.PagesNumInCategory} WHERE id = {category.Id}; ";

            mySqlConnection.Execute(queryUpdate);
        }

        private void SaveNewProductsLinkToDb(List<CitilinkProduct> links)
        {
            var values = string.Join(", ",
                links.Select(r =>
                    $"({MySqlWrap.ToMySqlParameters(r.CitilinkProductId)},{MySqlWrap.ToMySqlParameters(r.Name)},{MySqlWrap.ToMySqlParameters(r.CategoryProduct)},{MySqlWrap.ToMySqlParameters(r.Link)})"));
            var insertQuery = $"INSERT ignore into products_link (citilink_product_id,name, idCategory, link) VALUES {values}";

            mySqlConnection.Execute(insertQuery);
        }


        private void UpdateCategoryCrawlerStatus(CitilinkCategory category)
        {
            var queryUpdate =
                $"UPDATE categories SET parse_status = {category.ParseStatus} WHERE id = {category.Id}; ";

            mySqlConnection.Execute(queryUpdate);
        }
    }
}
