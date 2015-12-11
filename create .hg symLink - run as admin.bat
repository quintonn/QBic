@echo off 
cls 
set /p x=Press return to start...
mklink /d %~dp0\.hg %~dp0\hg
set /p z=Done successfully