using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Net.Http;
using System.IO;

namespace DS.RevitCmd.Template1
{
    internal  class HtmlClass
    {
      
        public static void  RunQuery(HttpClient httpClient, string path, string value = null, string key = "name")
        {
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString.Add(key, value);
            queryString.Add("age", "25");

            string requestUri = String.IsNullOrEmpty(key) ? "http://localhost:3000/" + path : "http://localhost:3000/" + path + "?" + queryString.ToString();

            // определяем данные запроса
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            Debug.WriteLine(request.RequestUri);

            // получаем ответ
            using HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // просматриваем данные ответа
            // статус
            Debug.WriteLine($"Status: {response.StatusCode}\n");

            // содержимое ответа
            Debug.WriteLine("\nContent");
            string content = response.Content.ReadAsStringAsync().Result;
            Debug.WriteLine(content);
        }

        //public static void RunJson(HttpClient httpClient, string path, string value = null, string key = "name")
        //{
        //    NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        //    queryString.Add(key, value);

        //    string requestUri = String.IsNullOrEmpty(key) ? "http://localhost:3000/" + path : "http://localhost:3000/" + path + "?" + queryString.ToString();

        //    Person tom = new Person { Name = value, Age = 38 };
        //    // создаем JsonContent
        //    JsonContent content = JsonContent.Create(tom);

        //    // определяем данные запроса
        //    using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        //    Console.WriteLine(request.RequestUri);
        //    request.Content = content;
        //    // получаем ответ
        //    using HttpResponseMessage response = httpClient.SendAsync(request).Result;

        //    // просматриваем данные ответа
        //    // статус
        //    Console.WriteLine($"Status: {response.StatusCode}\n");

        //    // содержимое ответа
        //    Console.WriteLine("\nContent");
        //    string cont = response.Content.ReadAsStringAsync().Result;
        //    Console.WriteLine(cont);
        //}
    }
}
