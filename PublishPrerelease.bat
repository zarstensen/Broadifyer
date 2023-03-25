cls

echo Publishing Broadifyer

cd Broadifyer

echo Publishing 'PreRelease'
dotnet publish Broadifyer.csproj -p:Platform=x64 /p:PublishProfile="PreRelease" /p:DebugSymbols=false

echo Publishing updater

cd ../Updater

echo Publishing 'PreRelease'
dotnet publish Updater.csproj /p:PublishProfile="PreRelease" /p:DebugSymbols=false

echo Done!

cd ../

