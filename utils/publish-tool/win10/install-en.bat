@echo off
title Ligral Installer
echo Auto installing ligral...
cd /d %~dp0
set LIGRAL=ligral
set INSTALLPATH=%LOCALAPPDATA%\%LIGRAL%
set EXECUTABLE=ligral-v0.2.3-win10-x64.exe
set DEFAULTSETTINGS=default.lig
set PLUGINS=plugins\
echo Install ligral into the default directory at
echo %INSTALLPATH%
set /p input=Enter [Y] to install ligral into the above dir (recommend), or keep ligral here.
if /i "%input%"=="y" ( 
    if not exist %INSTALLPATH% (
        md %INSTALLPATH%
    ) 
    copy %EXECUTABLE% %INSTALLPATH%\%EXECUTABLE%
    copy %DEFAULTSETTINGS% %INSTALLPATH%\%DEFAULTSETTINGS%
    xcopy %PLUGINS% %INSTALLPATH%\%PLUGINS%
    cd %INSTALLPATH%
    echo:
) else (
    set INSTALLPATH=%~dp0
)

echo %PATH% | findstr /I %INSTALLPATH% >nul || (
    set PATH_=%PATH:~0,-1024%
    if defined PATH_ (
        echo The PATH variable in your envionment variables is too long (more than 1024), please try to remove some unused paths and retry, or add the following path manually: 
        echo %INSTALLPATH%
        echo:
    ) else (
        setx PATH "%PATH%";"%INSTALLPATH%"
    )
)

if exist ligral.exe del ligral.exe
mklink ligral.exe %EXECUTABLE%
if errorlevel 1 (
    copy %EXECUTABLE% ligral.exe
    echo Due to the lack of administrator previlege, ligral is copied and consumes disk space. Running this with admin's previlege is recommended.
)
echo:
echo Installaton completed, press any key to quit...
pause > nul