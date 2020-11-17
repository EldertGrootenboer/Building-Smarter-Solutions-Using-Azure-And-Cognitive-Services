# Building Smarter Solutions Using Azure and Cognitive Services

## Preparations

- Update all other values in the [parameters file](./code/azuredeploy.parameters.json) according to your own environment.
- Make sure to update the organization in the according to the _commonDataService_ parameter your CRM organization.
- Make sure the Entities / Tables and their corresponding Fields / Columns specified in the _commonDataService_ parameter are created in your CDS environment.
- Make sure a Visitor entity exists with name eldert-grootenboer.

### Bot

- Clone the bot repository and make sure you can build and run it.
- Add an appsettings.json file with the content found below.
- After deployment, update the configuration as necessary.

```json
{
  "MicrosoftAppId": "",
  "MicrosoftAppPassword": "",
  "LuisAppId": "00000000-0000-0000-0000-000000000000",
  "LuisAPIKey": "<your-luis-api-key>",
  "LuisAPIHostName": "<your-luis-region>.api.cognitive.microsoft.com",
  "ApiManagementEndpoint": "https://<your-apim-instance-name>.azure-api.net",
  "ApiManagementCreateVisitorPath": "/visitors/create",
  "BlobConnectionString": "DefaultEndpointsProtocol=https;AccountName=<your-storage-account-name>;AccountKey=<your-storage-account-key>;EndpointSuffix=core.windows.net",
  "BlobContainerVisitorPictures": "visitorpictures",
  "ApiManagementSubscriptionKey": "<your-api-management-subscription-key>"
}
```

## Deployment

- Use the script [1-deployment.ps1](./code/iac/1-deployment.ps1) to deploy all the resources in Azure.
- After deployment, you will need to create some connections in the various Logic Apps as described below.

## Post-deployment

### LUIS

- Create LUIS authoring application from [https://eu.luis.ai](https://eu.luis.ai).
- Import the LUIS definition from [luis-building-smarter-solutions-using-cognitive-services.json](./code/iac/cognitive-services/luis-models/luis-building-smarter-solutions-using-cognitive-services.json).
- Update the configuration parameter _LuisAppId_ of the bot application settings with the identifier found on Manage page of the LUIS application on [https://eu.luis.ai](https://eu.luis.ai).
- Update the configuration parameter _LuisAPIKey_ of the bot with the API key which can be found in the Azure portal.

### VIsual Studio Code

- Update faceApiSubscriptionKey in your .vscode/settings.json.
- Update formRecognizerApiSubscriptionKey in your .vscode/settings.json.

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

- To train the Form Recognizer API we use [https://fott-preview.azurewebsites.net/](https://fott-preview.azurewebsites.net/).
- Start by creating a connection. Grab a SAS token from the Storage Account in Azure, and format it as <https://storageAccountName.blob.core.windows.net/departureCameraContainerName?sv=2019-12-12&ss=bfqt&srt=sco&sp=rwdlacupx&se=2022-11-16T22:39:52Z&st=2020-11-16T14:39:52Z&spr=https&sig=QU%2F%sjsbnsjbndejbd%2Bj13ElAaTa1F6MWIYH8%3D>.
- Create a new project using the connection we just created. Set the Form Recognizer service URI to <https://westeurope.api.cognitive.microsoft.com/>.
- To train the model, start by uploading all images from [demo-4-departure](./demo/demo-4-departure) except [demo.jpg](./demo/demo-4-departure/demo.jpg)
- Next, add the following labels and select them on each of the images.
  - Ship
  - Reason
  - Signee
  - Date
  - Signature
- After labeling the different fields, go to the train blade and train the model.
- After training your model, check if you can list them using [form-recognizer.rest](./code/iac/rest-calls/form-recognizer.rest). If you don't see your model, you will probably need to update the version (currently set to v2.1-preview.1) to the latest version. In this case, you will also need to update the version in [RetrieveLatestModel.cs](./code/functions/retrieve-latest-model/RetrieveLatestModel.cs).

### Face API

- Execute the calls in file [faces.rest](./code/iac/rest-calls/faces.rest) to create a person group.

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

The Somtrans LNG would like to register Eldert Grootenboer as a visitor to do repairs by tomorrow afternoon. Their email address is eldert@eldert.net to contact them on.
