@echo off

copy /y bin\Release\ImageViewer.dll GameData\ImageViewer\Plugins
copy  /y ImageViewerCont.version GameData\ImageViewer\ImageViewerCont.version

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"

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


copy ..\MiniAVC.dll Gamedata\ImageViewer
copy LICENSE Gamedata\ImageViewer

set FILE="%RELEASEDIR%\ImageViewerCont-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% GameData\ImageViewer
pause
