@echo OFF
@echo Publishing following 3 packs:
@echo:
DIR /B *.nupkg
@echo:
SETLOCAL
SET VERSION=1.0.14
pause
nuget push Flatwhite.%VERSION%.nupkg -NonInteractive
nuget push Flatwhite.Autofac.%VERSION%.nupkg -NonInteractive
nuget push Flatwhite.WebApi.%VERSION%.nupkg -NonInteractive
pause
ENDLOCAL