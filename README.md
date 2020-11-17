# Building Smarter Solutions Using Azure And Cognitive Services

There's a lot of discussion going on around Artificial Intelligence, and for good reason. AI and Cognitive Services are getting more powerful all the time, and it can be confronting to see all these developments. But how can we leverage this power in our own solutions, using it for making the life of our users and customers easier?

In this session, you will see learn we can get data from the real world, and use this to drive our business, and all this in a serverless manner. Thanks to Microsoft Cognitive Services it's easy to integrate and work with speech, text, images and videos into our processes. Come and learn how to use this to your own advantage, driving your business forward.

## Preparations

- Update all values in the [parameters file](./code/azuredeploy.parameters.json) according to your own environment.
- Make sure to update the organization in the _commonDataService_ parameter according to your CRM organization.
- Make sure the Entities / Tables and their corresponding Fields / Columns specified in the _commonDataService_ parameter are created in your CDS environment.
- Make sure at least one Ship entry is created with a _shipname_ set to **Somtrans LNG**.

## Deployment

- Use the script [1-deployment.ps1](./code/iac/1-deployment.ps1) to deploy all the resources in Azure.

## Post-deployment

### LUIS

- Create LUIS authoring application in the [LUIS portal](https://eu.luis.ai).
- Create a new LUIS authoring resource with, name set to _luis-building-smarter-solutions-using-cognitive-services_, via the [authoring resources blade](https://eu.luis.ai/user/settings/authoringResources).
- Switch to the newly created authoring resource and import the LUIS definition from [luis-building-smarter-solutions-using-cognitive-services.json](./code/iac/cognitive-services/luis-models/luis-building-smarter-solutions-using-cognitive-services.json).
- Train the model and publish it to the production slot when training has finished.
- Update the configuration parameter _LuisAppId_ of the bot application settings with the identifier found on Manage page of the LUIS application in the [Luis portal](https://eu.luis.ai).
- Update the configuration parameter _LuisAPIKey_ of the bot with the API key which can be found in the [Azure portal](https://portal.azure.com).

### Bot

- Clone the bot repository and make sure you can build and run it.
- Add an appsettings.json file with the content found below.
- Update the configuration as necessary.
- Run the bot locally using the [Bot Framework Emulator](https://aka.ms/bot-framework-emulator-debug-with-emulator).

```json
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
  "LuisAppId": "00000000-0000-0000-0000-000000000000",
  "LuisAPIKey": "<your-luis-api-key>",
  "LuisAPIHostName": "<your-luis-region>.api.cognitive.microsoft.com",
  "ApiManagementEndpoint": "https://<your-apim-instance-name>.azure-api.net",
  "ApiManagementSubscriptionKey": "<your-api-management-subscription-key>",
  "ApiManagementCreateVisitorPath": "/visitors/create",
  "BlobConnectionString": "DefaultEndpointsProtocol=https;AccountName=<your-storage-account-name>;AccountKey=<your-storage-account-key>;EndpointSuffix=core.windows.net",
  "BlobContainerVisitorPictures": "visitorpictures"
}
```

### RTT Cockpit

- Clone the RTTCockpit repository and make sure you can build and run it.
- Add an App.config file with the content found below.
- Update the configuration as necessary.

'''xml
<?xml version="1.0"?>
<configuration>
  <appSettings>
    <add key="ServiceBusConnectionString" value="Endpoint=sb://<your-service-bus-namespace>.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=<your-shared-access-key>" />
    <add key="QueueGateCamera" value="gate-camera" />
    <add key="QueueCarCamera" value="car-camera" />
    <add key="QueueDepartureCamera" value="departure-camera" />
  </appSettings>
</configuration>
'''

### VIsual Studio Code

- Create a .vscode/settings.json as outlined below.
- Update faceApiSubscriptionKey in the settings file.
- Update formRecognizerApiSubscriptionKey in the settings file.

```json
{
    "rest-client.environmentVariables": {
        "$shared": {
            "faceApiSubscriptionKey": "<your-api-subscription-key>",
            "formRecognizerApiSubscriptionKey": "<your-api-subscription-key>"
        }
    }
}
```

### Train Form Recognizer API

- To train the Form Recognizer API we use the [Form OCR Testing Tool](https://fott-preview.azurewebsites.net/).
- Start by creating a connection. Create a Blob service SAS URL in the Storage Account in Azure, and format it as <https://storageAccountName.blob.core.windows.net/departurecamera?sv=2019-12-12&ss=bfqt&srt=sco&sp=rwdlacupx&se=2022-11-16T22:39:52Z&st=2020-11-16T14:39:52Z&spr=https&sig=QU%2F%sjsbnsjbndejbd%2Bj13ElAaTa1F6MWIYH8%3D>. Notice that we need to add the name of the departurecamera container.
- Create a new project using the connection we just created. Set the Form Recognizer service URI to <https://your-region.api.cognitive.microsoft.com/>. The API key can be found on the Form Recognizer resource in the [Azure portal](https://portal.azure.com).
- To train the model, start by uploading all images from [demo-4-departure](./demo/demo-4-departure), except [demo.jpg](./demo/demo-4-departure/demo.jpg), into the departurecamera container.
- Next, add the following labels and select them on each of the images.
  - Ship
  - Reason
  - Signee
  - Date
  - Signature
- After labeling the different fields, go to the Train blade and train the model.
- After training your model, check if you can list them using [form-recognizer.rest](./code/iac/rest-calls/form-recognizer.rest). If you don't see your model, you will probably need to update the version (currently set to v2.1-preview.1) to the latest version. In this case, you will also need to update the version in [RetrieveLatestModel.cs](./code/functions/retrieve-latest-model/RetrieveLatestModel.cs).

### Face API

- Execute the call in file [faces.rest](./code/iac/rest-calls/faces.rest) to create a person group.

### Authorize API Connections

- Authorize the following connections, using the Edit API connection blade of the corresponding resource in the [Azure portal](https://portal.azure.com).
  - Common Data Service
  - Office 365 Outlook

### Update Logic App actions

- [la-process-gate-camera](./code/iac/logic-apps/la-process-gate-camera.json)
  - Get visitor details
    - Environment
    - Entity Name
- [la-process-visitor-pictures](./code/iac/logic-apps/la-process-visitor-pictures.json)
  - Get visitor
    - Environment
    - Entity Name
  - Update visitor
    - Environment
    - Entity Name
- [la-register-visitor](./code/iac/logic-apps/la-register-visitor.json)
  - Get ship
    - Environment
    - Entity Name
  - Get visitor
    - Environment
    - Entity Name
  - Update visitor
    - Environment
    - Entity Name
  - Create visitor
    - Environment
    - Entity Name

## Example utterances

- The Somtrans LNG would like to register Eldert Grootenboer as a visitor to do repairs by tomorrow afternoon. Their email address is eldert@eldert.net to contact them on.
