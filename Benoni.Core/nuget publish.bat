del ..\..\..\..\NugetSource\Benoni.Core*.nupkg
ECHO Packing Nuget Package
nuget pack -OutputDirectory "..\..\..\..\NugetSource"  "..\..\Benoni.Core.csproj"
ECHO Publishing Nuget Package
REM nuget push "..\..\..\..\NugetSource\Benoni.Core*.nupkg" -Source http://nugetrepo.q10hub.com/7c8fb305-b447-4e6f-86dd-88a39d99b47c v4J[TT?}eG_}+7rSg;r-AcGbhjH+F
