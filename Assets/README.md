# Building Smarter Solutions Using Azure and Cognitive Services

## TODO

- Registration status en link naar CRM toevoegen in cockpit
- Check to add cards for questions (reasons?)
- Text uit reason op worksheet vertalen vanuit NL naar EN
- In la-register-visitor there are a couple of fields with cr105 in their name, can not be changed as it's the key of the fields
- IaC for bot

## Deployment

- Create a service principal, which we will use for our Event Grid connectors in the Logic Apps. The script found here can be used from Azure CLI to create this. Update the value of eventGrid.servicePrincipal.clientId in the the [parameters file](./Code/azuredeploy.parameters.json).
- Update all other values in the [parameters file](./Code/azuredeploy.parameters.json) according to your own environment.
- We start by deploying [azuredeploy.1.json](./Code/azuredeploy.1.json). The parameter _servicePrincipalPasswordEventGridValue_ expects the client secret of the service principal we just created. The subscription key for parameter _rdwApiSubscriptionKey_ can be created on the [Socrata website](https://opendata.rdw.nl/login?return_to=%2Fprofile%2Fedit%2Fdeveloper_settings). Use the [parameters file](./Code/azuredeploy.parameters.json) for the _full_ parameter. Finally, the _templatesBaseUri_ parameter the location of the raw templates, such as [https://raw.githubusercontent.com/EldertGrootenboer/Sessions/master/Building%20Smarter%20Solutions%20Using%20Azure%20and%20Cognitive%20Services/Code/IaC](https://raw.githubusercontent.com/EldertGrootenboer/Sessions/master/Building%20Smarter%20Solutions%20Using%20Azure%20and%20Cognitive%20Services/Code/IaC).
- Next, we are going to deploy the Functions. For this we can use Visual Studio Code, and we will deploy to the Function Apps we just created.
- Finally we deploy the other services using [azuredeploy.2.json](./Code/azuredeploy.2.json).

## Example utterances

The Somtrans LNG would like to register Eldert Grootenboer as a visitor to do repairs by tomorrow afternoon. Their email address is eldert@eldert.net.
