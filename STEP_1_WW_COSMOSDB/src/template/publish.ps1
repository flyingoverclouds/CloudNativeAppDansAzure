Set-Location ..\WebAppUI\
dotnet publish -o .\publish
Set-Location ..\Template\
dotnet .\ZipPublishFile.dll ..\WebAppUI\publish\ .\WebAppUI.zip
