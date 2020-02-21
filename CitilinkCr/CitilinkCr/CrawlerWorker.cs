using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CitilinkCr.Classes;
using WebUtils;

namespace CitilinkCr
{
    public class CrawlerWorker
    {

        private readonly Regex regexCategoryCatalog;
        private readonly Regex regexCategoryName;
        private readonly Regex regexClearHttpTags;
        private readonly Regex regexHrefTag;
        private readonly Regex regexLiTag;


        //?available=0&sorting=name_asc&p=1

        // <ul class=\"category-catalog__item\">.*?<\/ul> 
        // <h4>(.*?)</h4> 
        public CrawlerWorker()
        {
            regexCategoryCatalog = new Regex("<ul class=\"category-catalog__item\">(.*?)</ul>", RegexOptions.Singleline);
            regexCategoryName = new Regex("<h4>(.*?)</h4>", RegexOptions.Singleline);
            regexClearHttpTags = new Regex("<[^>]*>");
            regexHrefTag = new Regex("<\\s*?a\\s*?href\\s*?=\\s*?\"(.*?)\".*?>(.*?)</a>", RegexOptions.Singleline);
            regexLiTag = new Regex("<li>(.*?)</li>", RegexOptions.Singleline);


            var categories = ParseCitilinkCategories();

            ParseAllGoodsFromSubCategories(categories);



            //https://www.citilink.ru/catalog/
        }


        public List<CitilinkCategory> ParseCitilinkCategories()
        {
            HttpRequestParameters request = new HttpRequestParameters()
            {
                RequestUri = "https://www.citilink.ru/catalog/"
            };


            var categoriesPage = Web.GetDocumentAsHttpClient(request);

            var categories = ParseCategoriesFromPage(categoriesPage);



            var categoriesList = new List<CitilinkCategory>();

            foreach (var oneCategory in categories)
            {
                var categoryName = regexCategoryName.Match(oneCategory);
                if (!categoryName.Success)
                    continue;



                var (isSuccess, tag) = ParseHrefTag(categoryName.Groups[1].Value);

                if (!isSuccess)
                    continue;


                var subCategories = ParseSubCategories(oneCategory);
                var cat = new CitilinkCategory(tag.Name, tag) { SubCategories = subCategories };

                categoriesList.Add(cat);
            }




            return categoriesList;
        }



        private void ParseAllGoodsFromSubCategories(List<CitilinkCategory> categories)
        {

            foreach (var oneCategory in categories)
            {

                foreach (var subCategory in oneCategory.SubCategories)
                {


                    var request = new HttpRequestParameters()
                    {
                        RequestUri = $"{subCategory.Href}?available=0&sorting=name_asc&p=0",
                        Referrer = "https://www.citilink.ru/"
                    };


                    var page = Web.GetDocumentAsHttpClient(request);
                }




            }
        }

        private List<string> ParseCategoriesFromPage(string page)
        {
            var matches = regexCategoryCatalog.Matches(page);
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
    }
}
