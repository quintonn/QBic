rem if not exist ..\NugetPackages mkdir ..\NugetPackages
rem nuget pack -OutputDirectory "..\NugetPackages"  "..\WebsiteTemplateCore.csproj"
rem -Exclude "config\*.*"
nuget pack -OutputDirectory "..\..\..\NugetSource"  "..\WebsiteTemplateCore.csproj"
rem nuget push "..\NugetPackages\WebsiteTemplate*.nupkg" -s http://cloudup.azurewebsites.net/ CF8F5B76-5C89-4032-AC36-B1C6DB70CA67