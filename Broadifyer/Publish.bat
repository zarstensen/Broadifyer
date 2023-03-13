cls

echo Deleting previous publish

rd /s /q "./bin/Publish/Release/"

echo Publising 'x64 Release'
dotnet publish Broadifyer.csproj /p:PublishProfile="x64 Release"
echo Publising 'x86 Release'
dotnet publish Broadifyer.csproj /p:PublishProfile="x86 Release"

cd "./bin/Publish/Release"

echo Compressing 'x64 Release'
tar -acf Broadifyer-x64.zip x64
echo Compressing 'x86 Release'
tar -acf Broadifyer-x86.zip x86

echo Done!

cd ../../../

