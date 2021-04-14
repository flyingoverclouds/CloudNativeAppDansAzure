Set-Location ..\CnaCatalogService\
dotnet publish -o .\publish
Set-Location ..\WebAppUI\
dotnet publish -o .\publish
Set-Location ..\Template\
dotnet .\ZipPublishFile.dll ..\WebAppUI\publish\ .\WebAppUI.zip
dotnet .\ZipPublishFile.dll ..\CnaCatalogService\publish\ .\CnaCatalogService.zip