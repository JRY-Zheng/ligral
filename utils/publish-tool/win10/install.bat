@echo off
title Ligral 安装程序
echo 正在自动安装 ligral...
cd /d %~dp0
set LIGRAL=ligral
set INSTALLPATH=%LOCALAPPDATA%\%LIGRAL%
set EXECUTABLE=ligral-v0.2.3-win10-x64.exe
set DEFAULTSETTINGS=default.lig
set PLUGINS=plugins\
echo 是否将 ligral 安装到默认安装路径？
echo 默认安装路径为：%INSTALLPATH%
set /p input=输入[Y]安装到默认安装路径（推荐），否则保留在当前路径。
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
        echo 环境变量中 PATH 变量过长，请删除不必要的路径重试，或者手动添加以下路径：
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
    echo 由于没有管理员权限，ligral 将被复制而占用磁盘空间。建议用管理员模式运行。
)
echo:
echo 安装完成，按任意键退出...
pause > nul