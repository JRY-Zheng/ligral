@echo off
title Ligral ��װ����
echo �����Զ���װ ligral...
cd /d %~dp0
set LIGRAL=ligral
set INSTALLPATH=%LOCALAPPDATA%\%LIGRAL%
set EXECUTABLE=ligral-v0.2.3-win10-x64.exe
set DEFAULTSETTINGS=default.lig
set PLUGINS=plugins\
echo �Ƿ� ligral ��װ��Ĭ�ϰ�װ·����
echo Ĭ�ϰ�װ·��Ϊ��%INSTALLPATH%
set /p input=����[Y]��װ��Ĭ�ϰ�װ·�����Ƽ������������ڵ�ǰ·����
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
        echo ���������� PATH ������������ɾ������Ҫ��·�����ԣ������ֶ��������·����
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
    echo ����û�й���ԱȨ�ޣ�ligral �������ƶ�ռ�ô��̿ռ䡣�����ù���Աģʽ���С�
)
echo:
echo ��װ��ɣ���������˳�...
pause > nul