using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebUtils
{
    public static class CallRest
    {
        public static async Task<CallResponse<TResp>> PostAsync<TReq, TResp>(
          string url,
          TReq reqObj,
          Dictionary<string, string> headers = null)
        {
            return await CallRest.PostAsync<TReq, TResp>(url, reqObj, headers, 0);
        }

        public static async Task<CallResponse<TResp>> PostAsync<TReq, TResp>(
          string url,
          TReq reqObj,
          Dictionary<string, string> headers = null,
          int timeout = 0)
        {
            WebProxy webProxy = (WebProxy)null;
            CallResponse<TResp> callResponse = new CallResponse<TResp>();


            var httpClientHandler = new HttpClientHandler();

            if (webProxy != null)
                httpClientHandler.Proxy = (IWebProxy)webProxy;

            var client = new HttpClient(httpClientHandler);

            if (timeout > 0)
                client.Timeout = TimeSpan.FromSeconds(timeout);

            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

            if (headers != null)
            {
                foreach (string key in headers.Keys)
                    client.DefaultRequestHeaders.Add(key, headers[key]);
            }
            var jsonObject = JsonConvert.SerializeObject(reqObj);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            callResponse.HttpResponse = response;
            if (response.IsSuccessStatusCode == true)
            {
                var end = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(end))
                    callResponse.Response = JsonConvert.DeserializeObject<TResp>(end);
            }

            return callResponse;
        }

        public static async Task<CallResponse<TResp>> PostAsync<TReq, TResp>(
          string url,
          TReq reqObj,
          HttpClient client)
        {
            CallResponse<TResp> callResponse = new CallResponse<TResp>();
            var jsonObject = JsonConvert.SerializeObject(reqObj);
            var content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            callResponse.HttpResponse = response;
            if (response.IsSuccessStatusCode == true)
            {
                var end = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(end))
                    callResponse.Response = JsonConvert.DeserializeObject<TResp>(end);
            }

            return callResponse;
        }



        public static async Task<CallResponse<TResp>> GetAsync<TResp>(
           string url,
           HttpClient client)
        {
            CallResponse<TResp> callResponse = new CallResponse<TResp>();

            var response = await client.GetAsync(url);
            callResponse.HttpResponse = response;
            if (response.IsSuccessStatusCode == true)
            {
                var end = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(end))
                    callResponse.Response = JsonConvert.DeserializeObject<TResp>(end);
            }

            return callResponse;
        }
    }
}
