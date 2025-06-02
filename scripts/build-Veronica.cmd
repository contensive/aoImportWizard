
rem all paths are relative to the git scripts folder

call build.cmd

set appName=veronica

rem upload to contensive application
c:
cd %collectionPath%
cc -a %appName% --installFile "%collectionName%.zip"
cd ..\..\scripts

