@echo off
cd /d %~dp0/../..
@REM dotnet publish src -p:PublishSingleFile=true -p:UseAppHost=true -o bin/Publish/win10 -c Release --self-contained true -r win10-x64
@REM dotnet publish src -p:PublishSingleFile=true -p:UseAppHost=true -o bin/Publish/linux -c Release --self-contained true -r linux-x64
@REM dotnet publish -p:PublishSingleFile=true -o bin/Publish/win10 -c Release --self-contained true --version-suffix beta -r win10-x64
@REM dotnet publish -p:PublishSingleFile=true -o bin/Publish/linux -c Release --self-contained true --version-suffix beta -r linux-x64

xcopy /s /i /y utils\publish-tool\linux\ligral-dpkg\ bin\Publish\linux\ligral-dpkg\
copy /Y utils\publish-tool\linux\pack.sh bin\Publish\linux\pack.sh
copy /Y bin\Publish\linux\ligral bin\Publish\linux\ligral-v0.2.3-linux-x64
copy /Y bin\Publish\linux\ligral bin\Publish\linux\ligral-dpkg\usr\bin\ligral-v0.2.3\ligral-v0.2.3-linux-x64
copy /Y bin\Debug\netcoreapp3.1\default.lig bin\Publish\linux\ligral-dpkg\usr\bin\ligral-v0.2.3\default.lig
xcopy /s /i /y bin\Debug\netcoreapp3.1\plugins bin\Publish\linux\ligral-dpkg\usr\bin\ligral-v0.2.3\plugins

copy /Y bin\Publish\win10\ligral.exe bin\Publish\win10\ligral-v0.2.3-win10-x64.exe
copy /Y bin\Debug\netcoreapp3.1\default.lig bin\Publish\win10\default.lig
xcopy /s /i /y bin\Debug\netcoreapp3.1\plugins bin\Publish\win10\plugins
copy /Y utils\publish-tool\win10\install.bat bin\Publish\win10\install.bat
copy /Y utils\publish-tool\win10\install-en.bat bin\Publish\win10\install-en.bat