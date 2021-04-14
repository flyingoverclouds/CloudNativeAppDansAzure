#!/bin/bash
#La ligne ci-dessus permet de remplacer les CR/LF ajouté par git en LF  (Oui je sais qu'il y a un config par défaut)
#sed -i.bak 's/\r$//' deploiement.sh
# installation dotnet https://docs.microsoft.com/fr-fr/dotnet/core/install/linux-ubuntu#2004-
# Script pour etape Etape 4 
# https://github.com/EricVernie/CloudNativeAppForAzureDev/blob/step4/README.md
# 
# Ce script crée
# - un groupe de ressource
# - un compte de stockage
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




# A supprimer dans le github public
sub=9a96981c-4056-48b7-9301-7c2757548bcf

echo "Entrez le suffixe à utiliser"
read suffixe
################################################################
#déclaration des variables
rgname="cna"$suffixe"-rg"
planname="cnaplan"$suffixe
webappname="cnawebui"$suffixe
catalogservicename="cnawebapi"$suffixe
functionname="cnafuncapi"$suffixe
functionstorage="cnafuncstr"$suffixe
accountname="cnacatalogstr"$suffixe
apimname=ncaapim$suffixe
catalogitembaseAddressurl="https://$catalogservicename.azurewebsites.net/api/catalog/items"
tablename=catalog 
containername=images 
cosmosdbname="cnacosmosdb"$suffixe
location=francecentral
runtime="DOTNETCORE|3.1" 
servicebusnamespace="cnaservicebus"$suffixe
queuename="cnacatalogqueue"
topicname="cnacatalogtopic"
subscriptionname="cnacatalogsubin"
################################################################

#az login

az account set --subscription $sub
 ############################## GROUPE DE RESSOURCE #################################
echo  création du groupe de ressource : $rgname 
az group create -n $rgname -l $location
####################################################################################

################################ COMPTE DE STOCKAGE ################################
echo  création du compte de stockage : $accountname
   az storage account create -n $accountname -g $rgname  --sku  Standard_LRS  -l $location

echo  création du compte de stockage : $accountname
  az storage account create -n $accountname -g $rgname  --sku  Standard_LRS  -l $location

echo  Récupère la clé de stockage 
accountkey=`az storage account keys list -n $accountname -g $rgname --query [0].value`
#supprimer les guillemets
accountkey=(`awk -F\" '{ print $2 }' <<< $accountkey`)

echo  création de la table : $tablename
   az storage table create -n $tablename --account-name $accountname --account-key $accountkey

echo  création du container : $containername 
  az storage container create -n $containername --account-name $accountname --account-key $accountkey

echo  insertion d une entité dans la table : $tableName
  az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=1 Id=1 OwnerId=1 ProductId=1 ProductName=Tomates  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/tomates.jpg  ProductAllergyInfo=None 
  az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=2 Id=2 OwnerId=1 ProductId=2 ProductName=Pain  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/Pain.jpg  ProductAllergyInfo=None 
  az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=3 Id=3 OwnerId=1 ProductId=3 ProductName=Aubergines  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/Aubergine.jpg  ProductAllergyInfo=None 
  az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=4 Id=4 OwnerId=1 ProductId=4 ProductName=Coca  ProductPictureUrl=https://picturesforcna.blob.core.windows.net/pictures/Coca.png  ProductAllergyInfo=None 

 ################################ COSMOSDB ################################
echo Création du compte CosmosDb $cosmosdbname

az cosmosdb create -n $cosmosdbname -g $rgname --capabilities EnableTable --default-consistency-level Eventual --locations regionName="francecentral" failoverPriority=0 isZoneRedundant=False --locations regionName="UK West" failoverPriority=1 isZoneRedundant=False


cosmosdbKey=`az cosmosdb keys list -g $rgname -n $cosmosdbname --query primaryMasterKey`
cosmosdbKey=(`awk -F\" '{ print $2 }' <<< $cosmosdbKey`)



cosmosdbconnectionstring="DefaultEndpointsProtocol=https;AccountName="+$cosmosdbname+";AccountKey="+$cosmosdbKey+";TableEndpoint=https://"+$cosmosdbname+".table.cosmos.azure.com:443/;"



echo Création de la table $tablename
az cosmosdb table create -a $cosmosdbname -g $rgname -n $tablename --throughput 400
####################################################################################

 ################################ SERVICE BUS #######################################
echo création du service bus
az servicebus namespace create -n $servicebusnamespace -g $rgname -l $location
az servicebus queue create --resource-group $rgname --namespace-name $servicebusnamespace --name $queuename
az servicebus topic create --resource-group $rgname --namespace-name  $servicebusnamespace --name $topicname
az servicebus topic subscription create -g $rgname  --namespace-name $servicebusnamespace  --topic-name $topicname -n $subscriptionname
servicebusconnectionstring=`az servicebus namespace authorization-rule keys list --resource-group $rgname --namespace-name $servicebusnamespace --name RootManageSharedAccessKey  --query primaryConnectionString`
#supprimer les guillemets
servicebusconnectionstring=(`awk -F\" '{ print $2 }' <<< $servicebusconnectionstring`)

####################################################################################

########################## SERVICE PLAN ET APP SERVICE ###########################
echo  création du service plan : $planname
  az appservice plan create -g $rgname -n $planname  --is-linux -l $location
  
echo  création de la webapp $webappname  
  az webapp create -g $rgname -p $planname -n $webappname -r $runtime

echo  création du service $catalogservicename
 az webapp create -g $rgname -p $planname -n $catalogservicename -r $runtime
####################################################################################


################################ APP INSIGHTS #####################################
echo Création des services AppInsight
az monitor app-insights component create -g $rgname -l $location -a $webappname --application-type web --kind web
az monitor app-insights component create -g $rgname -l $location -a $catalogservicename --application-type web --kind web
az monitor app-insights component create -g $rgname -l $location -a $functionname --application-type web --kind web
# az monitor app-insights component create -g $rgname -l $location -a $apimname --kind other

webuiappinsightsconnectionstring=`az monitor app-insights component show -g $rgname -a $webappname --query connectionString`
serviceappinsightsconnectionstring=`az monitor app-insights component show -g $rgname -a $catalogservicename --query connectionString`
functionappinsightsconnectionstring=`az monitor app-insights component show -g $rgname -a $functionname --query connectionString` 


#supprimer les guillemets
webuiappinsightsconnectionstring=(`awk -F\" '{ print $2 }' <<< $webuiappinsightsconnectionstring`)
serviceappinsightsconnectionstring=(`awk -F\" '{ print $2 }' <<< $serviceappinsightsconnectionstring`)
functionappinsightsconnectionstring=(`awk -F\" '{ print $2 }' <<< $functionappinsightsconnectionstring`)

#Récuperérer seulement L'instrumentkey
webuiappinsightinstrumentkey="${webuiappinsightsconnectionstring//InstrumentationKey=}"
serviceappinsightinstrumentkey="${serviceappinsightsconnectionstring//InstrumentationKey=}"
functionnameappinsightskey="${functionappinsightsconnectionstring//InstrumentationKey=}"
####################################################################################

echo  Ajout de paramétres aux applications
  az webapp config appsettings set -g $rgname -n $webappname --settings APPINSIGHTS_INSTRUMENTATIONKEY=$webuiappinsightinstrumentkey  APPLICATIONINSIGHTS_CONNECTION_STRING=$webuiappinsightsconnectionstring  CatalogItemBaseAddressUrl=$catalogitembaseAddressurl ServiceBusConnectionString=$servicebusconnectionstring  @webuidefaultsettings.json
  az webapp config appsettings set -g $rgname -n $catalogservicename --settings CosmosdbConnectionString=$cosmosdbconnectionstring UseCosmosDb=false APPINSIGHTS_INSTRUMENTATIONKEY=$serviceappinsightinstrumentkey  APPLICATIONINSIGHTS_CONNECTION_STRING=$serviceappinsightsconnectionstring   AccountName=$accountname AccountKey=$accountkey CatalogName=$tablename ContainerName=$containername  ServiceBusConnectionString=$servicebusconnectionstring @catalogdefaultsettings.json

################################ AZURE FUNCTION ##################################
echo Création de la fonction azure
az storage account create -n $functionstorage -g $rgname  --sku  Standard_LRS  -l $location

az functionapp create -n $functionname -g $rgname --app-insights $functionname --app-insights-key $functionnameappinsightskey -p $planname -s $functionstorage --os-type "linux" --runtime "dotnet" --functions-version 3
az functionapp config appsettings set -n $functionname -g $rgname --settings ApplicationInsightsAgent_EXTENSION_VERSION="~2" APPLICATIONINSIGHTS_CONNECTION_STRING=$functionappinsightsconnectionstring QueueName=$queuename ServiceBusConnectionString=$servicebusconnectionstring


###################### API MANAGEMENT  ###################################
echo création de la passerelle 
az apim create --name $apimname -g $rgname  -l $location  --sku-name Developer --publisher-email ericv@microsoft.com --publisher-name Microsoft --no-wait
##################################################################################


################################ DEPLOIEMENT DES APPS ##################################

az webapp deployment source config-zip -g $rgname -n $catalogservicename --src ./CnaCatalogService.zip
az webapp stop -g $rgname -n $catalogservicename
az webapp start -g $rgname -n $catalogservicename

az webapp deployment source config-zip -g $rgname -n $webappname --src ./WebAppUI.zip
az webapp stop -g $rgname -n $webappname
az webapp start -g $rgname -n $webappname

az functionapp deployment source config-zip -g $rgname -n $functionname --src ./CnaFuncOnCatalog.zip
az functionapp stop -g $rgname -n $functionname 
az functionapp start -g $rgname -n $functionname 
##################################################################################

echo Une fois l application testée, appuyez sur [ENTREE] pour supprimer le groupe de ressource
read 
  az group delete -g  $rgname

