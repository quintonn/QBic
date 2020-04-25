rem nuget pack -OutputDirectory "..\..\..\..\NugetSource"  "..\..\BasicAuthentication.csproj"
rem if not exist ..\NugetPackages mkdir ..\NugetPackages
rem nuget pack -OutputDirectory "..\NugetPackages"  "..\..\BasicAuthentication.csproj"
rem -Exclude "config\*.*"
rem nuget pack -OutputDirectory "..\..\..\NugetSource"  "..\WebsiteTemplate.csproj"
rem nuget pack -OutputDirectory "..\..\..\..\NugetSource"  "..\..\BasicAuthentication.csproj"
del ..\..\..\..\NugetSource\BasicAuthentication*.nupkg
ECHO Packing Nuget Package
nuget pack -OutputDirectory "..\..\..\..\NugetSource"  "..\..\BasicAuthentication.csproj"
ECHO Publishing Nuget Package
REM nuget push "..\..\..\..\NugetSource\BasicAuthentication*.nupkg" -Source http://nugetrepo.q10hub.com/7c8fb305-b447-4e6f-86dd-88a39d99b47c v4J[TT?}eG_}+7rSg;r-AcGbhjH+F

nuget push "..\..\..\..\NugetSource\BasicAuthentication*.nupkg" -Source http://nuget.letsallgo.online/7c8fb305-b447-4e6f-86dd-88a39d99b47c v4J[TT?}eG_}+7rSg;r-AcGbhjH+F

ECHO Publish Complete