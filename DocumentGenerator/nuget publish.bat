del ..\..\..\NugetSource\DocumentGenerator*.nupkg
ECHO Packing Nuget Package
nuget pack -OutputDirectory "..\..\..\NugetSource"  "..\DocumentGenerator.csproj"
ECHO Publishing Nuget Package
REM nuget push "..\..\..\NugetSource\DocumentGenerator*.nupkg" -Source http://nugetrepo.q10hub.com/7c8fb305-b447-4e6f-86dd-88a39d99b47c v4J[TT?}eG_}+7rSg;r-AcGbhjH+F

REM nuget push "..\..\..\NugetSource\DocumentGenerator*.nupkg" -Source http://nuget.letsallgo.online/7c8fb305-b447-4e6f-86dd-88a39d99b47c v4J[TT?}eG_}+7rSg;r-AcGbhjH+F
nuget push "..\..\..\NugetSource\DocumentGenerator*.nupkg" -Source https://moat.repocastle.com/NuGet/qbic/repo abf17bcd-1c2f-4389-9c96-2a9a6edc5e35

ECHO Publish Complete