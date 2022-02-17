# What we will be doing in this script.
#   1. Create a resource group
#   2. Deploy Azure services

# Update these according to the environment
$subscriptionName = "Visual Studio Enterprise"
$resourceGroupName = "rg-building-smarter-solutions-using-cognitive-services"
$servicePrincipalName = "sp-building-smarter-solutions-using-cognitive-services"
$administratorEmail = "me@eldert.net"
$basePath = "/home/codespace/workspace/Building-Smarter-Solutions-Using-Azure-And-Cognitive-Services"

# Login to Azure
Get-AzSubscription -SubscriptionName $subscriptionName | Set-AzContext

# Retrieves the dynamic parameters
$administratorObjectId = (Get-AzADUser -Mail $administratorEmail).Id

# If the app registration exists, delete it, as we will create a new one
$servicePrincipal = Get-AzADServicePrincipal -DisplayName $servicePrincipalName

if($servicePrincipal)
{
    Remove-AzADServicePrincipal -DisplayName $servicePrincipalName -Force
    Remove-AzADApplication -DisplayName $servicePrincipalName -Force
}

# Create app registration
$servicePrincipal = New-AzADServicePrincipal -DisplayName $servicePrincipalName
$clientId = $servicePrincipal.ApplicationId | ConvertTo-SecureString -AsPlainText -Force

# Create client secret
$bytes = New-Object Byte[] 32
([System.Security.Cryptography.RandomNumberGenerator]::Create()).GetBytes($bytes)
$clientSecret = [System.Convert]::ToBase64String($bytes) | ConvertTo-SecureString -AsPlainText -Force
$endDate = [System.DateTime]::Now.AddYears(5)
New-AzADAppCredential -DisplayName $servicePrincipalName -Password $clientSecret -EndDate $endDate

# Create the resource group
New-AzResourceGroup -Name $resourceGroupName -Location 'West Europe' -Tag @{CreationDate=[DateTime]::UtcNow.ToString(); Project="Building Smarter Solutions Using Azure and Cognitive Services"; Purpose="Session"} -Force

# Deploy Key Vault
New-AzResourceGroupDeployment -Name "BuildSmarterSolutionsKeyVault" -ResourceGroupName $resourceGroupName -TemplateFile "$basePath\assets\code\iac\security\key-vault.json" -administratorObjectId $administratorObjectId -servicePrincipalClientIdValue $clientId -servicePrincipalPasswordValue $clientSecret

# Deploy first group of resources
New-AzResourceGroupDeployment -Name "BuildSmarterSolutions1" -ResourceGroupName $resourceGroupName -TemplateFile "$basePath\assets\code\iac\azuredeploy.1.json"

# Deploy the License Plate Recognizer Function
dotnet publish "$basePath\assets\code\functions\license-plate-recognizer\LicensePlateRecognizer.csproj" -c Release -o "$basePath\assets\code\functions\license-plate-recognizer\publish"
Compress-Archive -Path "$basePath\assets\code\functions\license-plate-recognizer\publish\*" -DestinationPath "$basePath\assets\code\functions\license-plate-recognizer\Deployment.zip"
$functionApp = Get-AzResource -ResourceGroupName $resourceGroupName -Name fa-license-plate-recognizer-*
Publish-AzWebapp -ResourceGroupName $resourceGroupName -Name $functionApp.Name -ArchivePath "$basePath/assets/code/functions/license-plate-recognizer/Deployment.zip" -Force
Remove-Item "$basePath\assets\code\functions\license-plate-recognizer\Deployment.zip"

# Deploy the Retrieve Latest Model Function
dotnet publish "$basePath\assets\code\functions\retrieve-latest-model\RetrieveLatestModel.csproj" -c Release -o "$basePath\assets\code\functions\retrieve-latest-model\publish"
Compress-Archive -Path "$basePath\assets\code\functions\retrieve-latest-model\publish\*" -DestinationPath "$basePath\assets\code\functions\retrieve-latest-model\Deployment.zip"
$functionApp = Get-AzResource -ResourceGroupName $resourceGroupName -Name fa-retrieve-latest-model-*
Publish-AzWebapp -ResourceGroupName $resourceGroupName -Name $functionApp.Name -ArchivePath "$basePath/assets/code/functions/retrieve-latest-model/Deployment.zip" -Force
Remove-Item "$basePath\assets\code\functions\retrieve-latest-model\Deployment.zip"

# Deploy second group of resources
New-AzResourceGroupDeployment -Name "BuildSmarterSolutions2" -ResourceGroupName $resourceGroupName -TemplateFile "$basePath\assets\code\iac\azuredeploy.2.json"

# Optional for debugging, loops through each local file individually
#Get-ChildItem "$basePath\assets\code\iac" -Recurse -Filter *.json | 
#Foreach-Object {
#    Write-Output "Deploying: " $_.FullName
#    New-AzResourceGroupDeployment -Name Demo -ResourceGroupName $resourceGroupName -TemplateFile $_.FullName -TemplateParameterFile "$basePath\assets\code\iac\azuredeploy.parameters.json" -ErrorAction Continue
#}