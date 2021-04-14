#publier l'appplication
cd ../CnaCatalogService/
dotnet publish -o ./publish
cd ../WebAppUI/
dotnet publish -o ./publish
cd ../CnaFuncOnCatalog/
dotnet publish -o ./publish
cd ../Template/
dotnet ./ZipPublishFile.dll ../CnaFuncOnCatalog/publish/ ./CnaFuncOnCatalog.zip
dotnet ./ZipPublishFile.dll ../WebAppUI/publish/ ./WebAppUI.zip
dotnet ./ZipPublishFile.dll ../CnaCatalogService/publish/ ./CnaCatalogService.zip