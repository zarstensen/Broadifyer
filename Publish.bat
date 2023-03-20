cls

echo Deleting previous publish

rd /s /q "./Broadifyer/bin/Publish/Release/"

echo Publishing Broadifyer

cd Broadifyer

echo Publishing 'x64 Release'
dotnet publish Broadifyer.csproj /p:PublishProfile="x64 Release" /p:DebugSymbols=false
echo Publishing 'x86 Release'
dotnet publish Broadifyer.csproj /p:PublishProfile="x86 Release" /p:DebugSymbols=false

echo Publishing updater

cd ../Updater

echo Publishing 'x64 Release'
dotnet publish Updater.csproj /p:PublishProfile="x64 Release" /p:DebugSymbols=false
echo Publishing 'x86 Release'
dotnet publish Updater.csproj /p:PublishProfile="x86 Release" /p:DebugSymbols=false


cd "../Broadifyer/bin/Publish/Release"

echo Compressing 'x64 Release'
cd x64
tar -acf ../Broadifyer-x64.zip *
echo Compressing 'x86 Release'
cd ../x86
tar -acf ../Broadifyer-x86.zip *

echo Done!

cd ../../../../../

