# Build in release mode first
az login
az account set --subscription "fdf3a3a3-c8f5-472f-8367-6a9a4a6c11a9"
rm .\.deployment
az bot prepare-deploy --lang Csharp --code-dir "." --proj-file-path ".\CoreBot.csproj"
Compress-Archive -Path .\* -DestinationPath .\Deployment.zip
az webapp deployment source config-zip --resource-group "rg-building-smarter-solutions-using-cognitive-services" --name "bot-smarter-solutions-cognitive-services" --src ".\Deployment.zip"
rm .\Deployment.zip