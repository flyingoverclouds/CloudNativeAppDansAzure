Set-Location ..\CnaCatalogService\
dotnet publish -r win10-x86 --self-contained -o .\publish
Set-Location ..\WebAppUI\
dotnet publish -r win10-x86 --self-contained -o .\publish
Set-Location ..\CnaFuncOnCatalog\
dotnet publish -o .\publish
Set-Location ..\Template\
dotnet .\ZipPublishFile.dll ..\CnaFuncOnCatalog\publish\ .\CnaFuncOnCatalog.zip
dotnet .\ZipPublishFile.dll ..\WebAppUI\publish\ .\WebAppUI.zip
dotnet .\ZipPublishFile.dll ..\CnaCatalogService\publish\ .\CnaCatalogService.zip