$resourceGroupName = "rg-building-smarter-solutions-using-cognitive-services"
$basePath = "C:\Users\elder\OneDrive\Sessions\Building-Smarter-Solutions-Using-Azure-And-Cognitive-Services\assets\code\bot"

# Build in release mode first
az login
az account set --subscription "fdf3a3a3-c8f5-472f-8367-6a9a4a6c11a9"
Remove-Item $basePath\.deployment
az bot prepare-deploy --lang Csharp --code-dir $basePath --proj-file-path "$basePath\CoreBot.csproj"
Compress-Archive -Path $basePath\* -DestinationPath $basePath\Deployment.zip
az webapp deployment source config-zip --resource-group $resourceGroupName --name "bot-building-smarter-solutions" --src "$basePath\Deployment.zip"
Remove-Item $basePath\Deployment.zip