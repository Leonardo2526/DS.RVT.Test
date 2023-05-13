using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using RestSharp;
using System.Diagnostics;
using System.Threading;

namespace DS.RevitCmd.Template2
{
    internal class RestClass
    {
        public string Put(string path)
        {
            var options = "http://localhost:3000/" + path;
            //var options = new RestClientOptions("http://localhost:3000/" + path);

            var client = new RestClient(options);

            var request = new RestRequest(Method.POST);

            request.RequestFormat = DataFormat.Json;
            // отправляемый объект 
            Person tom = new Person { Name = "Dan", Age = 38 };

            //request.AddQueryParameter("name", "Aranaks"); 
            request.AddXmlBody(tom);

            var response = client.Execute(request);
            //var response = client.Post(request);
            //var response = client.ExecutePostAsync<Person>(request).Result;

            var content = response.Content; // raw content as string

            // просматриваем данные ответа
            // статус
            Debug.WriteLine($"\nStatus: {response.StatusCode}");

            // содержимое ответа
            Debug.WriteLine("Content");
            var cont = response.Content;
            Debug.WriteLine(cont);

            return content;
        }
    }
}
