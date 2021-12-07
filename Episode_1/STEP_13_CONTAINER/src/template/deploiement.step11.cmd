echo %1

echo running STEP11 specific deployment task.
echo -  Target subscription: %sub%
echo -  Suffixe: %suffixe%

echo *Creating storage container for uploaded pictures ...
REM call az storage container create --name productimages --public-access blob --account-key=%accountKey% --account-name=%accountname% 

echo *Creating storage queue for invalid pictures ...
REM call az storage queue create --name invalidimages  --account-key=%accountKey% --account-name=%accountname% 

echo *Creating ComputerVision cognitiveservice ...
set cogsvcname=cgproduct-%suffixe%
set cogsvcsku=F0
REM call az cognitiveservices account create --kind "ComputerVision" --name %cogsvcname% --location %location% --resource-group %rgname% --sku %cogsvcsku% --subscription %sub%

REM TODO : get csVisionKey & csVisionEndpoint to inject as parameters in function app

echo *Creating function app
set funcname=funcnewimg-%suffixe%
REM call az functionapp create -g %rgname%  -p %planname% -n %funcname% -s %accountname% --os-type=Windows 

echo *Update parameters on function app
REM inject CS vision paramterers
REM inject storage account connection string
REM call az function config appsettings set -g %rgname% -n %funcname% --settings CatalogItemsServiceUrl=%catalogserviceurl%

