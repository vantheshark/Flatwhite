del Flatwhite.*.nupkg

SETLOCAL
SET VERSION=1.0.15
powershell -Command "(gc Flatwhite\Package.nuspec) -replace '__OLD_VERSION__', '%VERSION%' | Out-File -encoding ASCII Flatwhite\Package.nuspec"
powershell -Command "(gc Flatwhite.Autofac\Package.nuspec) -replace '__OLD_VERSION__', '%VERSION%' | Out-File -encoding ASCII Flatwhite.Autofac\Package.nuspec"
powershell -Command "(gc Flatwhite.WebApi\Package.nuspec) -replace '__OLD_VERSION__', '%VERSION%' | Out-File -encoding ASCII Flatwhite.WebApi\Package.nuspec"

nuget pack Flatwhite\Package.nuspec -Version %VERSION% -NonInteractive
nuget pack Flatwhite.Autofac\Package.nuspec -Version %VERSION% -NonInteractive
nuget pack Flatwhite.WebApi\Package.nuspec -Version %VERSION% -NonInteractive


git checkout Flatwhite\Package.nuspec
git checkout Flatwhite.Autofac\Package.nuspec
git checkout Flatwhite.WebApi\Package.nuspec
ENDLOCAL
pause