cls

echo Deleting previous publish

rd /s /q "./bin/Publish/Release/"

echo Publishing 'x64 Release'
dotnet publish Broadifyer.csproj /p:PublishProfile="x64 Release"
echo Publishing 'x86 Release'
dotnet publish Broadifyer.csproj /p:PublishProfile="x86 Release"

cd "./bin/Publish/Release"

echo Compressing 'x64 Release'
cd x64
tar -acf ../Broadifyer-x64.zip *
echo Compressing 'x86 Release'
cd x86
tar -acf ../Broadifyer-x86.zip *

echo Done!

cd ../../../

