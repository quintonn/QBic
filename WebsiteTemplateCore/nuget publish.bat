rem if not exist ..\NugetPackages mkdir ..\NugetPackages
rem nuget pack -OutputDirectory "..\NugetPackages"  "..\WebsiteTemplateCore.csproj"
rem -Exclude "config\*.*"
ECHO Packing Nuget Package
nuget pack -OutputDirectory "..\..\..\NugetSource"  "..\WebsiteTemplateCore.csproj"
ECHO Publishing Nuget Package
nuget push "..\..\..\NugetSource\WebsiteTemplateCore*.nupkg" -s http://nugetrepo.q10hub.com/7c8fb305-b447-4e6f-86dd-88a39d99b47c v4J[TT?}eG_}+7rSg;r-AcGbhjH+F
reM nuget push "..\..\..\NugetSource\WebsiteTemplateCore*.nupkg" -s http://localhost/OnlineNugetRepo/7c8fb305-b447-4e6f-86dd-88a39d99b47c v4J[TT?}eG_}+7rSg;r-AcGbhjH+F
ECHO Deleting Nuget Package
del ..\..\..\NugetSource\WebsiteTemplateCore*.nupkg
ECHO Done with nuget code