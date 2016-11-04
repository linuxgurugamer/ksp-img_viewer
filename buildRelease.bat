@echo off

copy /y bin\Release\ImageViewer.dll GameData\ImageViewer\Plugins
copy  /y ImageViewerCont.version GameData\ImageViewer\ImageViewerCont.version

set DEFHOMEDRIVE=d:
set DEFHOMEDIR=%DEFHOMEDRIVE%%HOMEPATH%
set HOMEDIR=
set HOMEDRIVE=%CD:~0,2%

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"
echo Default homedir: %DEFHOMEDIR%

rem set /p HOMEDIR= "Enter Home directory, or <CR> for default: "

if "%HOMEDIR%" == "" (
set HOMEDIR=%DEFHOMEDIR%
)
echo %HOMEDIR%

SET _test=%HOMEDIR:~1,1%
if "%_test%" == ":" (
set HOMEDRIVE=%HOMEDIR:~0,2%
)

type ImageViewerCont.version
set /p VERSION= "Enter version: "


rmdir /s /q %HOMEDIR%\install\Gamedata\ImageViewer

xcopy /Y /E GameData\ImageViewer %HOMEDIR%\install\Gamedata\ImageViewer
copy ..\MiniAVC.dll %HOMEDIR%\install\Gamedata\ImageViewer
copy LICENSE %HOMEDIR%\install\Gamedata\ImageViewer

%HOMEDRIVE%
cd %HOMEDIR%\install

set FILE="%RELEASEDIR%\ImageViewerCont-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% GameData\ImageViewer
pause
