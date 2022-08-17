@echo off
cd /d %~dp0/../..
dotnet publish src -p:PublishSingleFile=true -p:UseAppHost=true -o bin/Publish/win10 -c Release --self-contained true -r win10-x64
dotnet publish src -p:PublishSingleFile=true -p:UseAppHost=true -o bin/Publish/linux -c Release --self-contained true -r linux-x64
@REM dotnet publish -p:PublishSingleFile=true -o bin/Publish/win10 -c Release --self-contained true --version-suffix beta -r win10-x64
@REM dotnet publish -p:PublishSingleFile=true -o bin/Publish/linux -c Release --self-contained true --version-suffix beta -r linux-x64

cp -r utils\publish-tool\linux\ligral-dpkg\ bin\Publish\linux\
cp bin\Publish\linux\ligral bin\Publish\linux\ligral-v0.2.3-linux-x64
cp bin\Publish\linux\ligral bin\Publish\linux\ligral-dpkg\usr\bin\ligral-v0.2.3\ligral-v0.2.3-linux-x64
cp bin\Debug\netcoreapp3.1\default.lig bin\Publish\linux\ligral-dpkg\usr\bin\ligral-v0.2.3\default.lig
cp -r bin\Debug\netcoreapp3.1\plugins bin\Publish\linux\ligral-dpkg\usr\bin\ligral-v0.2.3\plugins

cp bin\Publish\win10\ligral.exe bin\Publish\win10\ligral-v0.2.3-win10-x64.exe
cp bin\Debug\netcoreapp3.1\default.lig bin\Publish\win10\default.lig
cp -r bin\Debug\netcoreapp3.1\plugins bin\Publish\win10\plugins