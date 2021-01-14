using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

namespace EPH.Functions
{
    public static class RetrieveLatestModel
    {
        private static HttpClient _httpClient;

        private static HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                }
                
                return _httpClient;
            }
        }

        [FunctionName("RetrieveLatestModel")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var hasMore = true;
            List<Model> models = new List<Model>();
            var url = "https://westeurope.api.cognitive.microsoft.com/formrecognizer/v2.1-preview.2/custom/models?op=full";

            while (hasMore)
            {
                HttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("FormRecognizerApiSubscriptionKey"));
                var response = await HttpClient.GetAsync(url);

                var stream = await response.Content.ReadAsStreamAsync();
                var body = await new StreamReader(stream).ReadToEndAsync();
                var modelsResponse = JsonConvert.DeserializeObject<ModelsResponse>(body);

                models.AddRange(modelsResponse.modelList);

                hasMore = !string.IsNullOrWhiteSpace(modelsResponse.nextLink);
                url = modelsResponse.nextLink;
            }

            return models.Count == 0 ?
                (IActionResult)(new NotFoundResult()) :
                new OkObjectResult(models.OrderBy(model => model.lastUpdatedDateTime).LastOrDefault().modelId);
        }
    }
}