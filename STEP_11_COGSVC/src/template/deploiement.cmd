echo %1

REM set sub=f3156437-0000-0000-0000-58c9d29dc92b 
set /p sub=Entrez l'ID de souscription (c'est un guid) : 
set /p suffixe=Entrez le suffixe de déploiement : 

set rgname=nca%suffixe%-rg
set planname=ncaplan%suffixe%
set webappname=ncawebui%suffixe%
set catalogservicename=ncaapicatalog%suffixe%
set accountname=ncastorage%suffixe%
set tablename=catalog 
set containerName=images 
set location=francecentral 



set catalogserviceurl=https://%catalogservicename%.azurewebsites.net/api/catalogitems


rem call az login

  call az account set --subscription %sub%
 
echo  création du groupe de ressource : %rgname% 
  call az group create -n %rgname% -l %location%

echo  création du compte de stockage : %accountname% 
  call az storage account create -n %accountname% -g %rgname%  --sku  Standard_LRS  -l %location%

echo  création du service plan : %planname% 
  call az appservice plan create -g %rgname% -n %planname% -l %location% 

echo  Récupère la clé de stockage 
  call az storage account keys list -n %accountname% -g %rgname% 

echo  collez la clé 
  set /p accountKey=  Entrez la clé  

echo  création de la table : %tableName% 
  call az storage table create -n %tablename% --account-name %accountname%  --account-key %accountkey%

echo  création du container : %containerName% 

  call az storage container create -n %containerName% --account-name %accountname%  --account-key %accountkey%

echo  insertion d'une entité dans la table : %tableName% 
  call az storage entity insert --account-key %accountKey%  --account-name %accountname%  -t %tablename% --if-exists replace -e CatalogItemsServiceUrl=%catalogserviceurl%  PartitionKey=1 RowKey=1 Id=1 OwnerId=1 ProductId=1 ProductName=haricots  ProductPictureUrl=https://vignette.wikia.nocookie.net/recipes/images/d/d7/Haricot_Verts.jpg  ProductAllergyInfo=None 

echo  se positionner dans le répertoire ou se trouve le code 
  cd ..\..\src\WebAppUI

echo  création de la webapp %webappname%   + publication de l'application  
call az webapp up -g %rgname% -p %planname% -n %webappname% -l %location% 

echo  se positionner dans le répertoire ou se trouve le code 
  cd ..\CnaCatalogService

echo  création du service pp %catalogservicename%   + publication de l'application  
call az webapp up -g %rgname% -p %planname% -n %catalogservicename% -l %location% 


echo  se repositionner dans le répertoire du modèle 
  cd ..\template\

echo  Ajout de paramétres à l'application (AppSettings) 
call az webapp config appsettings set -g %rgname% -n %webappname% --settings CatalogItemsServiceUrl=%catalogserviceurl%   @webappsettings.json

call az webapp config appsettings set -g %rgname% -n %catalogservicename% --settings AccountName=%accountName% AccountKey=%accountKey% CatalogName=%tableName% ContainerName=%containerName%  @webappsettings.json


set /p nerienfaire= Une fois l'application testée, appuyez sur [ENTREE] pour supprimer le groupe de ressource

call az group delete -g  %rgname% 

