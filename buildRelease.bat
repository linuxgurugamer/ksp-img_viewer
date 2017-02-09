﻿@echo off

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


set VERSIONFILE=ImageViewerCont.version
rem The following requires the JQ program, available here: https://stedolan.github.io/jq/download/
c:\local\jq-win64  ".VERSION.MAJOR" %VERSIONFILE% >tmpfile
set /P major=<tmpfile

c:\local\jq-win64  ".VERSION.MINOR"  %VERSIONFILE% >tmpfile
set /P minor=<tmpfile

c:\local\jq-win64  ".VERSION.PATCH"  %VERSIONFILE% >tmpfile
set /P patch=<tmpfile

c:\local\jq-win64  ".VERSION.BUILD"  %VERSIONFILE% >tmpfile
set /P build=<tmpfile
del tmpfile
set VERSION=%major%.%minor%.%patch%
if "%build%" NEQ "0"  set VERSION=%VERSION%.%build%



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
