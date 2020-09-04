using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace EPH.Functions
{
    public static class LicensePlateRecognizer
    {
        [FunctionName("LicensePlateRecognizer")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                log.LogInformation("Reading body");
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation("Convert to OCR object.");
                var ocr = JsonConvert.DeserializeObject<OCR>(requestBody);

                log.LogInformation("LINQ query.");
                var words = ocr.regions.SelectMany(group => group.lines.SelectMany(line => line.words.Where(word => word.text.Length == 8 && word.text.Count(character => character == '-') == 2)));

                log.LogInformation($"Return, count is {words.Count()}.");
                return words.Count() > 0 ?
                    (IActionResult)new OkObjectResult(words.First().text.Replace("-", string.Empty)) :
                    new NotFoundResult();
            }
            catch(Exception exception)
            {
                return new BadRequestObjectResult(exception);
            }
        }
    }
}
