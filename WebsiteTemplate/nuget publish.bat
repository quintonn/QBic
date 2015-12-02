if not exist ..\NugetPackages mkdir ..\NugetPackages
nuget pack -OutputDirectory "..\NugetPackages"  "..\WebsiteTemplate.csproj"
rem -Exclude "config\*.*"
rem nuget pack -OutputDirectory "..\..\..\NugetSource"  "..\WebsiteTemplate.csproj"
nuget push "..\NugetPackages\WebsiteTemplate*.nupkg" -s http://cloudup.azurewebsites.net/ CF8F5B76-5C89-4032-AC36-B1C6DB70CA67