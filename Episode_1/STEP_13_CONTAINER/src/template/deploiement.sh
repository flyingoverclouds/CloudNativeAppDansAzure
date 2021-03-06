#!/bin/bash
#sed -i.bak 's/\r$//' deploiement.sh

echo "Entrez le suffixe "
read suffixe
sub=e4ff89e1-14eb-4ea4-83f7-8c9959675b6b 
rgname="nca"$suffixe"-rg"
planname="ncaplan"$suffixe
webappname="ncawebui"$suffixe
catalogservicename="ncaapicatalog"$suffixe
accountname="ncastorage"$suffixe
catalogserviceurl="https://$catalogservicename.azurewebsites.net/api/catalogitems"
tablename=catalog 
containername=images 
location=francecentral
runtime="DOTNETCORE|3.1" 

#call laz login

az account set --subscription $sub
 
echo  création du groupe de ressource : $rgname 
az group create -n $rgname -l $location

echo  création du compte de stockage : $accountname
   az storage account create -n $accountname -g $rgname  --sku  Standard_LRS  -l $location

echo  création du compte de stockage : $accountname
  az storage account create -n $accountname -g $rgname  --sku  Standard_LRS  -l $location

echo  Récupère la clé de stockage 
  az storage account keys list -n $accountname -g $rgname 

echo  collez la clé 
read accountkey
  
echo  création de la table : $tablename
   az storage table create -n $tablename --account-name $accountname --account-key $accountkey

echo  création du container : $containername 

  az storage container create -n $containername --account-name $accountname --account-key $accountkey

echo  insertion d une entité dans la table : $tableName
  az storage entity insert --account-key $accountkey  --account-name $accountname -t $tablename --if-exists replace -e PartitionKey=1 RowKey=1 Id=1 OwnerId=1 ProductId=1 ProductName=haricots  ProductPictureUrl=https://vignette.wikia.nocookie.net/recipes/images/d/d7/Haricot_Verts.jpg  ProductAllergyInfo=None 


echo  création du service plan : $planname
  az appservice plan create -g $rgname -n $planname  --is-linux -l $location
  
echo  création de la webapp $webappname  
  az webapp create -g $rgname -p $planname -n $webappname -r $runtime


echo  création du service $catalogservicename

 az webapp create -g $rgname -p $planname -n $catalogservicename -r $runtime

echo  Ajout de paramétres aux applications
  az webapp config appsettings set -g $rgname -n $webappname --settings CatalogItemsServiceUrl=$catalogserviceurl @webappsettings.json

  az webapp config appsettings set -g $rgname -n $catalogservicename --settings AccountName=$accountname AccountKey=$accountkey CatalogName=$tablename ContainerName=$containername  @webappsettings.json

echo Une fois l application testée, appuyez sur [ENTREE] pour supprimer le groupe de ressource
read 
  az group delete -g  $rgname

