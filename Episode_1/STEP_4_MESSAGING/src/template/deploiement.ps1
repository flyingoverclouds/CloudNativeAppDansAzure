# Script pour etape Etape 4 - Eric Vernié
# 
# Ce script crée
# - un groupe de ressource
# - un compte de stockage/Cosmosdb
#   - On récupère la clé de stockage que l'on utilisera pour : 
#    - Créer une table dans laquelle nous inserons une entité de test
#    - Créer un container (Blob)
#    - Alimenter le Paramètre AccountKey des App Services 
# - Un espace de nom pour service bus
#   - avec une file d'attente et un topic + une souscription pour ce topic
#   - On récupère la chaine de connexion 
# - Service Plan afin de créer
#   - 2 App Services, dans lesquels nous inserrons des paramètres par défaut
# - Une passerelle d'API
# - Une Azure Function
# - Les services AppInsight


$sub=az account show --query "id" -o tsv
az account set -s $sub
az extension add --name application-insights

$suffixe=Read-host "Entrez le suffixe à utiliser (que des minuscule et chiffres :) " 

####################################################################################
# Compilation des applications
Remove-Item WebAppUi.zip  
dotnet publish ..\WebAppUI\CnAppForAzureDev.csproj
Compress-Archive -Path ..\WebAppUI\bin\Debug\netcoreapp3.1\publish\* -DestinationPath WebAppUi.zip  

Remove-Item CnaCatalogService.zip  
dotnet publish ..\CnaCatalogService\CnaCatalogService.csproj
Compress-Archive -Path ..\CnaCatalogService\bin\Debug\netcoreapp3.1\publish\* -DestinationPath CnaCatalogService.zip  


Remove-Item CnaFuncOnCatalog.zip  
dotnet publish ..\CnaFuncOnCatalog\CnaFuncOnCatalog.csproj
Compress-Archive -Path ..\CnaFuncOnCatalog\bin\Debug\netcoreapp3.1\publish\* -DestinationPath CnaFuncOnCatalog.zip  

################################################################
# déclaration des variables

$rgname="CNA-"+ $suffixe +"-rg"
$planname="cna$suffixe-plan"
$webappname="cnawebui$suffixe"
$catalogservicename="cnawebapi"+$suffixe
$functionname="cnafuncapi"+$suffixe
$functionstorage="cnafuncstr"+$suffixe
$accountname="cnacatalogstr"+$suffixe
$tablename="catalog" 
$containername="images" 
#$apimname="cnaapim"+ $suffixe
$servicebusnamespace="cnasb"+$suffixe
$queuename="cnacatalogqueue"
$topicname="cnacatalogtopic"
$subscriptionname="cnacatalogsubin"
$cosmosdbname="cnacosmosdb"+$suffixe
$location="francecentral"
$catalogitemBaseaddressurl="https://" +$catalogservicename + ".azurewebsites.net/api/catalog/items"

############################## GROUPE DE RESSOURCE #################################
write-host  Création du groupe de ressources : $rgname 
az group create -n $rgname -l $location
####################################################################################

################################ COMPTE DE STOCKAGE ################################
write-host  Création du compte de stockage : $accountname
   az storage account create -n $accountname -g $rgname  --sku  Standard_LRS  -l $location

write-host  Création du compte de stockage : $accountname
  az storage account create -n $accountname -g $rgname  --sku  Standard_LRS  -l $location

write-host  Récupère la clé de stockage : $accountname
$accountkey=az storage account keys list -n $accountname -g $rgname --query [0].value
#supprimer les guillemets
$accountkey=$accountkey.Substring(1).Substring(0,$accountkey.Length-2)

write-host  Création de la table : $tablename
   az storage table create -n $tablename --account-name $accountname --account-key $accountkey

write-host  Création du container : $containername 
  az storage container create -n $containername --account-name $accountname --account-key $accountkey

  write-host  Insertion entités test dans la table : $tableName
  az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=1 Id=1 OwnerId=1 ProductId=1 ProductName=Tomates  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/tomates.jpg  ProductAllergyInfo=None 
  az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=2 Id=2 OwnerId=1 ProductId=2 ProductName=Pain  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/pain.jpg  ProductAllergyInfo=None 
  az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=3 Id=3 OwnerId=1 ProductId=3 ProductName=Aubergines  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/Aubergine.png  ProductAllergyInfo=None 
  az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=4 Id=4 OwnerId=1 ProductId=4 ProductName=Coca  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/Coca.png  ProductAllergyInfo=None 


####################################################################################

################################ COSMOSDB ################################
write-host Création du compte CosmosDb $cosmosdbname

az cosmosdb create -n $cosmosdbname -g $rgname --capabilities EnableTable --default-consistency-level Eventual --locations regionName="francecentral"


$cosmosdbKey=az cosmosdb keys list -g $rgname -n $cosmosdbname --query primaryMasterKey
$cosmosdbKey=$cosmosdbKey.Substring(1).Substring(0,$cosmosdbKey.Length-2)

$cosmosdbconnectionstring="DefaultEndpointsProtocol=https;AccountName="+$cosmosdbname+";AccountKey="+$cosmosdbKey+";TableEndpoint=https://"+$cosmosdbname+".table.cosmos.azure.com:443/;"

write-host $cosmosdbconnectionstring

Write-host Création de la table $tablename
az cosmosdb table create -a $cosmosdbname -g $rgname -n $tableName --throughput 400
####################################################################################
 
 ################################ SERVICE BUS #######################################
write-host Création du service Service bus : $servicebusnamespace
az servicebus namespace create -n $servicebusnamespace -g $rgname -l $location
write-host Création de la queue :  $queuename
az servicebus queue create --resource-group $rgname --namespace-name $servicebusnamespace --name $queuename
write-host Création du topic :  $topicname
az servicebus topic create --resource-group $rgname --namespace-name  $servicebusnamespace --name $topicname
write-host Création de la subscription  $subscriptionname pour le topic $topicname
az servicebus topic subscription create -g $rgname  --namespace-name $servicebusnamespace  --topic-name $topicname -n $subscriptionname
#Récupère la chaine de connexion du ServiceBus
$servicebusconnectionstring=az servicebus namespace authorization-rule keys list --resource-group $rgname --namespace-name $servicebusnamespace --name RootManageSharedAccessKey  --query primaryConnectionString

#supprimer les guillemets
$servicebusconnectionstring=$servicebusconnectionstring.Substring(1).Substring(0,$servicebusconnectionstring.Length-2)
####################################################################################

########################## SERVICE PLAN ET APP SERVICE ###########################
write-host  Création du service plan : $planname
az appservice plan create -g $rgname -n $planname -l $location

write-host  Création de la webapp $webappname  
az webapp create -g $rgname -p $planname -n $webappname -r "`"DOTNETCORE|3.1`""

write-host  Création du service $catalogservicename
az webapp create -g $rgname -p $planname -n $catalogservicename -r "`"DOTNETCORE|3.1`""

####################################################################################


################################ APP INSIGHTS #####################################
write-host Création des services AppInsights
az monitor app-insights component create -g $rgname -l $location -a $webappname --application-type web --kind web
az monitor app-insights component create -g $rgname -l $location -a $catalogservicename --application-type web --kind web
az monitor app-insights component create -g $rgname -l $location -a $functionname --application-type web --kind web
# az monitor app-insights component create -g $rgname -l $location -a $apimname --kind other

#Récupère les chaines de connexions AppInsights
$webuiappinsightsconnectionstring=az monitor app-insights component show -g $rgname -a $webappname --query connectionString
$serviceappinsightsconnectionstring=az monitor app-insights component show -g $rgname -a $catalogservicename --query connectionString
$functionappinsightsconnectionstring=az monitor app-insights component show -g $rgname -a $functionname --query connectionString


#supprimer les guillemets
$webuiappinsightsconnectionstring=$webuiappinsightsconnectionstring.Substring(1).Substring(0,$webuiappinsightsconnectionstring.Length-2)
$serviceappinsightsconnectionstring=$serviceappinsightsconnectionstring.Substring(1).Substring(0,$serviceappinsightsconnectionstring.Length-2)
$functionappinsightsconnectionstring=$functionappinsightsconnectionstring.Substring(1).Substring(0,$functionappinsightsconnectionstring.Length-2)

#Récuperer seulement L'instrumentkey
$webuiappinsightinstrumentkey=az monitor app-insights component show -g $rgname -a $webappname --query instrumentationKey
$serviceappinsightinstrumentkey=az monitor app-insights component show -g $rgname -a $catalogservicename --query instrumentationKey
$functionnameappinsightskey=az monitor app-insights component show -g $rgname -a $functionname --query instrumentationKey

#supprimer les guillemets
$webuiappinsightinstrumentkey=$webuiappinsightinstrumentkey.Substring(1).Substring(0,$webuiappinsightinstrumentkey.Length-2)
$serviceappinsightinstrumentkey=$serviceappinsightinstrumentkey.Substring(1).Substring(0,$serviceappinsightinstrumentkey.Length-2)
$functionnameappinsightskey=$functionnameappinsightskey.Substring(1).Substring(0,$functionnameappinsightskey.Length-2)


write-host  Ajout de paramétres aux applications

az webapp config appsettings set -g $rgname -n $webappname --settings ApplicationInsightsAgent_EXTENSION_VERSION="~2" APPINSIGHTS_INSTRUMENTATIONKEY=$webuiappinsightinstrumentkey  APPLICATIONINSIGHTS_CONNECTION_STRING=$webuiappinsightsconnectionstring  
az webapp config appsettings set -g $rgname -n $webappname --settings CatalogItemBaseAddressUrl=$catalogitembaseAddressurl ServiceBusConnectionString=$servicebusconnectionstring   MaxItemsOnHomePage=4 QueueName=$queuename TopicName=$topicname SubscriptionName=$subscriptionname

az webapp config appsettings set -g $rgname -n $catalogservicename --settings  ApplicationInsightsAgent_EXTENSION_VERSION="~2" APPINSIGHTS_INSTRUMENTATIONKEY=$serviceappinsightinstrumentkey  APPLICATIONINSIGHTS_CONNECTION_STRING=$serviceappinsightsconnectionstring 
az webapp config appsettings set -g $rgname -n $catalogservicename --settings  CosmosdbConnectionString=$cosmosdbconnectionstring AccountName=$accountname AccountKey=$accountkey CatalogName=$tablename ContainerName=$containername OpenApiLicenseUrl="https://github.com/EricVernie/CloudNativeAppForAzureDev/blob/step3/LICENSE"
az webapp config appsettings set -g $rgname -n $catalogservicename --settings UseCosmosDb=false AlternateImageUrl="https://picturesforcna.blob.core.windows.net/pictures/alternateImage.jpg" MaxItems=25 ServiceBusConnectionString=$servicebusconnectionstring QueueName=$queuename TopicName=$topicname SubscriptionName=$subscriptionname
####################################################################################

################################ AZURE FUNCTION ##################################
write-host Création de la fonction azure
az storage account create -n $functionstorage -g $rgname  --sku  Standard_LRS  -l $location
az functionapp create -n $functionname -g $rgname --app-insights $functionname --app-insights-key $functionnameappinsightskey -c $location -s $functionstorage --functions-version 3 
az functionapp config appsettings set -n $functionname -g $rgname --settings ApplicationInsightsAgent_EXTENSION_VERSION="~2" APPLICATIONINSIGHTS_CONNECTION_STRING=$functionappinsightsconnectionstring QueueName=$queuename ServiceBusConnectionString=$servicebusconnectionstring
##################################################################################


################################ DEPLOIEMENT DES APPS ##################################
#déploiement des applications
Write-host "Deploiement de CnaCatalogService.zip ..."
az webapp deployment source config-zip -g $rgname -n $catalogservicename --src .\CnaCatalogService.zip
Write-host "Restarting webapp ..."
az webapp stop -g $rgname -n $catalogservicename
az webapp start -g $rgname -n $catalogservicename

Write-host "Deploiement de WebAppUi.zip ..."
az webapp deployment source config-zip -g $rgname -n $webappname --src .\WebAppUI.zip
Write-host "Restarting webapp ..."
az webapp stop -g $rgname -n $webappname
az webapp start -g $rgname -n $webappname

Write-host "Deploiement de CnaFuncOnCatalog.zip ..."
az functionapp deployment source config-zip -g $rgname -n $functionname --src .\CnaFuncOnCatalog.zip
Write-host "Restarting webapp ..."
az functionapp stop -g $rgname -n $functionname 
az functionapp start -g $rgname -n $functionname 
##################################################################################

Remove-Item .\CnaCatalogService.zip
Remove-Item .\CnaFuncOnCatalog.zip
Remove-Item .\WebAppUi.zip

# Memento
Write-Host "*** WebApps :"
az webapp list -g $rgname -o table
Write-Host "*** Functions :"
az functionapp list -g $rgname -o table
