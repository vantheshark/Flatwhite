@echo OFF
@echo Publishing following 3 packs:
@echo:
DIR /B *.nupkg
@echo:
SETLOCAL
SET VERSION=1.0.26
pause
C:\tools\nuget push Flatwhite.%VERSION%.nupkg -NonInteractive -Source https://www.nuget.org/api/v2/package
C:\tools\nuget push Flatwhite.Autofac.%VERSION%.nupkg -NonInteractive -Source https://www.nuget.org/api/v2/package
C:\tools\nuget push Flatwhite.WebApi.%VERSION%.nupkg -NonInteractive -Source https://www.nuget.org/api/v2/package
pause
ENDLOCAL