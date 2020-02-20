using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace WebUtils
{

    /// <summary>
    /// параметры реквест запроса к ссылке
    /// </summary>
    public class HttpRequestParameters
    {
        public string BaseUrl { get; set; }
        public string Referrer { get; set; }
        public string RequestUri { get; set; }

    }

    /// <summary>
    /// класс работы с получением данных из сети Интернет
    /// </summary>
    public static class Web
    {
        //public static string GetDocument(string url)
        //{
        //    StringBuilder resBuilder = new StringBuilder();
        //    string line;
        //    WebClient client = new WebClient();
        //    client.Encoding = Encoding.UTF8;
        //    client.Headers.Add("Upgrade-Insecure-Requests", "1");
        //    client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
        //    Stream data = client.OpenRead(url);
        //    StreamReader reader = new StreamReader(data, Encoding.UTF8);
        //    while ((line = reader.ReadLine()) != null)
        //    {
        //        line = reader.ReadLine();
        //        resBuilder.AppendLine(line);
        //    }
        //    data.Close();
        //    reader.Close();
        //    return resBuilder.ToString();
        //}


        /// <summary>
        /// по параметрам запроса, делает запрос к RequestUri и читает все полученные данные
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string GetDocumentAsHttpClient(HttpRequestParameters parameters)
        {
            var resBuilder = new StringBuilder();

            var client = new HttpClient();
            if (!string.IsNullOrEmpty(parameters.BaseUrl))
                client.BaseAddress = new Uri(parameters.BaseUrl);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134");
            if (!string.IsNullOrEmpty(parameters.Referrer))
                client.DefaultRequestHeaders.Referrer = new Uri(parameters.Referrer);
            var response = client.GetAsync(parameters.RequestUri).Result;
            if (response.IsSuccessStatusCode)
            {

                using (var res = response.Content.ReadAsStreamAsync().Result)
                {
                    using (StreamReader reader = new StreamReader(res, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            resBuilder.AppendLine(line);
                        }
                    }
                }
            }

            return resBuilder.ToString();
        }

    }
}
