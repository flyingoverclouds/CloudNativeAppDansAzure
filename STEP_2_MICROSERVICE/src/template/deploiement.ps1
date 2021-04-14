

#$sub="XXXXX"
$sub=az account show --query "id" -o tsv

$suffixe=Read-host "Entrez le suffixe à utiliser" 

$rgname="CNA-"+ $suffixe +"-rg"
$accountname="cna"+$suffixe+"storage"
$planname="cnaplan"+$suffixe
$webappname="cnawebui"+$suffixe
$catalogservicename="cnaapicatalog"+$suffixe
$tablename="catalog" 
$containername="images"
$location="francecentral"
$cosmosdbname="cnacosmosdb"+$suffixe
$catalogserviceurl="https://"+$catalogservicename+".azurewebsites.net/api/catalogitems"


####################################################################################
# Compilation des applications
Remove-Item WebAppUi.zip  
dotnet publish ..\WebAppUI\CnAppForAzureDev.csproj
Compress-Archive -Path ..\WebAppUI\bin\Debug\netcoreapp3.1\publish\* -DestinationPath WebAppUi.zip  

Remove-Item CnaCatalogService.zip  
dotnet publish ..\CnaCatalogService\CnaCatalogService.csproj
Compress-Archive -Path ..\CnaCatalogService\bin\Debug\netcoreapp3.1\publish\* -DestinationPath CnaCatalogService.zip  



####################################################################################

#az login
az account set --subscription $sub

############################## GROUPE DE RESSOURCE #################################
write-host  Création du groupe de ressource : $rgname 
az group create -n $rgname -l $location
####################################################################################
################################ COSMOSDB ################################
write-host Création du compte CosmosDb $cosmosdbname

az cosmosdb create -n $cosmosdbname -g $rgname --capabilities EnableTable --default-consistency-level Eventual --locations regionName="francecentral" failoverPriority=0 isZoneRedundant=False --locations regionName="UK West" failoverPriority=1 isZoneRedundant=False


$cosmosdbKey=az cosmosdb keys list -g $rgname -n $cosmosdbname --query primaryMasterKey
$cosmosdbKey=$cosmosdbKey.Substring(1).Substring(0,$cosmosdbKey.Length-2)

$cosmosdbconnectionstring="DefaultEndpointsProtocol=https;AccountName="+$cosmosdbname+";AccountKey="+$cosmosdbKey+";TableEndpoint=https://"+$cosmosdbname+".table.cosmos.azure.com:443/;"

write-host $cosmosdbconnectionstring

Write-host Création de la table $tablename
az cosmosdb table create -a $cosmosdbname -g $rgname -n $tableName --throughput 400
####################################################################################
################################ COMPTE DE STOCKAGE POUR IMAGES ################################
write-host  Création du compte de stockage : $accountname
$checknamestorage=az storage account check-name -n $accountname --query nameAvailable
if ($checknamestorage -eq $false)
{
    Write-Error 'Nom du storage account invalide : $accountname'
    break
}

az storage account create -n $accountname -g $rgname  --sku  Standard_LRS  -l $location
#Récupère la clé de stockage : $accountname
$accountkey=az storage account keys list -n $accountname -g $rgname --query [0].value
#supprimer les guillemets
$accountkey=$accountkey.Substring(1).Substring(0,$accountkey.Length-2)

## Using Cosmosdb instead
write-host  Création de la table : $tablename 
   az storage table create -n $tablename --account-name $accountname --account-key $accountkey


write-host  Création du container : $containername 
  az storage container create -n $containername --account-name $accountname --account-key $accountkey

write-host  Insertion entités test dans la table : $tableName
  
az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=1 Id=1 OwnerId=1 ProductId=1 ProductName=Tomates  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/tomates.jpg  ProductAllergyInfo=None 
az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=2 Id=2 OwnerId=1 ProductId=2 ProductName=Pain  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/pain.jpg  ProductAllergyInfo=None 
az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=3 Id=3 OwnerId=1 ProductId=3 ProductName=Aubergines  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/Aubergine.png  ProductAllergyInfo=None 
az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=4 Id=4 OwnerId=1 ProductId=4 ProductName=Coca  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/Coca.png  ProductAllergyInfo=None 
 ########################## SERVICE PLAN ET APP SERVICE ###########################
write-host  Création du service plan : $planname
az appservice plan create -g $rgname -n $planname -l $location

az webapp create -g $rgname -p $planname -n $webappname 
az webapp config appsettings set -g $rgname -n $webappname --settings  CatalogItemsServiceUrl=$catalogserviceurl  MaxItemsOnHomePage=6 ApplicationInsightsAgent_EXTENSION_VERSION="~2" 
az webapp deployment source config-zip -g $rgname -n $webappname --src .\WebAppUI.zip
az webapp stop -g $rgname -n $webappname
az webapp start -g $rgname -n $webappname

write-host  Création API $catalogservicename
az webapp create -g $rgname -p $planname -n $catalogservicename 
az webapp config appsettings set -g $rgname -n $catalogservicename --settings UseCosmosDb=false AccountKey=$accountkey AccountName=$accountname CosmosDbConnectionString=$cosmosdbconnectionstring CatalogName=$tablename ContainerName=$containername  MaxItems=25 MaxItemsOnHomePage=6 ApplicationInsightsAgent_EXTENSION_VERSION="~2" 
az webapp deployment source config-zip -g $rgname -n $catalogservicename --src .\CnaCatalogService.zip
az webapp stop -g $rgname -n $webappname
az webapp start -g $rgname -n $webappname



##################################################################################
#Cleaning temporary deployment artefact
Remove-Item WebAppUI.zip
Remove-Item CnaCatalogService.zip