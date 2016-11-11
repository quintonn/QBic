rem if not exist ..\NugetPackages mkdir ..\NugetPackages
rem nuget pack -OutputDirectory "..\NugetPackages"  "..\WebsiteTemplateCore.csproj"
rem -Exclude "config\*.*"
ECHO Packing Nuget Package
nuget pack -OutputDirectory "..\..\..\NugetSource"  "..\WebsiteTemplateCore.csproj"
ECHO Publishing Nuget Package
nuget push "..\..\..\NugetSource\WebsiteTemplateCore*.nupkg" -s http://nugetrepo.q10hub.com/nuget v4J[TT?}eG_}+7rSg;r-AcGbhjH+F
ECHO Deleting Nuget Package
del ..\..\..\NugetSource\WebsiteTemplateCore*.nupkg
ECHO Done with nuget code