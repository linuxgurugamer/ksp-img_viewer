@echo on
set H=R:\KSP_1.1.4_dev
echo %H%

copy /y bin\Debug\ImageViewer.dll GameData\ImageViewer\Plugins
copy  /y ImageViewerCont.version GameData\ImageViewer\ImageViewerCont.version

xcopy /Y /E GameData\ImageViewer %H%\GameData\ImageViewer

pause
