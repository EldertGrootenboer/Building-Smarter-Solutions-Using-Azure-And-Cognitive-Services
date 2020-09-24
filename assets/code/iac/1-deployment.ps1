# What we will be doing in this script.
#   1. Create a resource group
#   2. Deploy Azure services

# Update these according to the environment
$subscriptionName = "Visual Studio Enterprise"
$resourceGroupName = "rg-building-smarter-solutions-using-cognitive-services"
$administratorEmail = "me@eldert.net"
$basePath = "C:\Users\elder\OneDrive\Sessions\Building-Smarter-Solutions-Using-Azure-And-Cognitive-Services"

# Login to Azure
Get-AzSubscription -SubscriptionName $subscriptionName | Set-AzContext

# Retrieves the dynamic parameters
$administratorObjectId = (Get-AzADUser -Mail $administratorEmail).Id

# Create the resource group and deploy the resources
New-AzResourceGroup -Name $resourceGroupName -Location 'West Europe' -Tag @{CreationDate=[DateTime]::UtcNow.ToString(); Project="Building Smarter Solutions Using Azure and Cognitive Services"; Purpose="Session"}
New-AzResourceGroupDeployment -Name "BuildSmarterSolutions1" -ResourceGroupName $resourceGroupName -TemplateFile "$basePath\assets\code\iac\azuredeploy.1.json" -administratorObjectId $administratorObjectId

# Deploy the Bot
Invoke-Expression "$basePath\assets\code\bot\Scripts\deployment.ps1"

# Deploy second group of resources
New-AzResourceGroupDeployment -Name "BuildSmarterSolutions2" -ResourceGroupName $resourceGroupName -TemplateFile "$basePath\assets\code\iac\azuredeploy.2.json"

# Optional for debugging, loops through each local file individually
#Get-ChildItem "$basePath\assets\code\iac" -Filter *.json | 
#Foreach-Object {
#    Write-Output "Deploying: " $_.FullName
#    New-AzResourceGroupDeployment -Name Demo -ResourceGroupName $resourceGroupName -TemplateFile $_.FullName -ErrorAction Continue
#}