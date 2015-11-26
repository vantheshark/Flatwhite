del Flatwhite.*.nupkg

SETLOCAL
SET VERSION=1.0.6

nuget pack Flatwhite\Package.nuspec -Version %VERSION%
nuget pack Flatwhite.Autofac\Package.nuspec -Version %VERSION%
nuget pack Flatwhite.WebApi\Package.nuspec -Version %VERSION%
ENDLOCAL
pause