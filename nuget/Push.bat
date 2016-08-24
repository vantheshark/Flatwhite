@echo OFF
@echo Publishing following 3 packs:
@echo:
DIR /B *.nupkg
@echo:
SETLOCAL
SET VERSION=1.0.24
pause
C:\tools\nuget push Flatwhite.%VERSION%.nupkg -NonInteractive
C:\tools\nuget push Flatwhite.Autofac.%VERSION%.nupkg -NonInteractive
C:\tools\nuget push Flatwhite.WebApi.%VERSION%.nupkg -NonInteractive
pause
ENDLOCAL