
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.BotBuilderSamples.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Handlers
{
    public class VisitorHandler
    {
        private static HttpClient _client = new HttpClient();
        private readonly string _registerVisitorUri;
        private readonly string _blobConnectionString;
        private readonly string _visitorPicturesContainer;

        public VisitorHandler(IConfiguration configuration)
        {
            _registerVisitorUri = $"{configuration["ApiManagementEndpoint"]}{configuration["ApiManagementCreateVisitorPath"]}";
            _blobConnectionString = configuration["BlobConnectionString"];
            _visitorPicturesContainer = configuration["BlobContainerVisitorPictures"];
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration["ApiManagementSubscriptionKey"]);
        }

        // Save the registration in CRM.
        public async Task<HttpResponseMessage> RegisterAsync(VisitDetails visitDetails)
        {
            var request = new
            {
                ship = visitDetails.Ship,
                visitor = visitDetails.Visitor.Name,
                visitDate = visitDetails.Visit.DateTime.Split("T")[0],
                reason = visitDetails.Visit.Reason,
                email = visitDetails.Visitor.Email
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            return await _client.PostAsync(_registerVisitorUri, content);
        }

        // Save the registration in CRM.
        public async Task UploadPictureAsync(VisitDetails visitDetails)
        {
            var webClient = new WebClient();
            var stream = webClient.OpenRead(visitDetails.Visitor.Picture.Uri);

            var blobServiceClient = new BlobServiceClient(_blobConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_visitorPicturesContainer);
            var blobClient = containerClient.GetBlobClient(visitDetails.Visitor.Name);
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = visitDetails.Visitor.Picture.ContentType });
        }
    }
}