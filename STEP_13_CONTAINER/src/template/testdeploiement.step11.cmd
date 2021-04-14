@echo off
REM ## SIMULATING deploument.cmd envvariable creation
set sub=a2796b68-37b4-495e-9c36-6c442134664d
set suffixe=nct01
set accountKey=y9481DilWnrk+tfVczMfwJ0R7jN9M3FhquInIJCJAxNMeNJjR3swmIxmg+Jae4mfZKbsulk4SNgu1pY9DYb3Xw==

set rgname=nca%suffixe%-rg
set planname=ncaplan%suffixe%
set webappname=ncawebui%suffixe%
set catalogservicename=ncaapicatalog%suffixe%
set accountname=ncastorage%suffixe%
set tablename=catalog 
set containerName=images 
set location=francecentral 
set catalogserviceurl=https://%catalogservicename%.azurewebsites.net/api/catalogitems

call deploiement.step11.cmd %1