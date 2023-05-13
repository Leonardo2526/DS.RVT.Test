using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.SerilogTests
{
    public class CustomHttpClient : IHttpClient
    {
        private readonly HttpClient httpClient;

        public CustomHttpClient() => httpClient = new HttpClient();

        public void Configure(IConfiguration configuration) => httpClient.DefaultRequestHeaders.Add("X-Api-Key", configuration["apiKey"]);

        public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
        {
            using var content = new StreamContent(contentStream);
            content.Headers.Add("Content-Type", "application/json");

            var response = await httpClient
                .PostAsync(requestUri, content)
                .ConfigureAwait(false);

            // просматриваем данные ответа
            // статус
            Console.WriteLine($"Status: {response.StatusCode}\n");

            // содержимое ответа
            Console.WriteLine("\nContent");
            string respContent = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(respContent);

            return response;
        }

        public void Dispose() => httpClient?.Dispose();
    }
}
