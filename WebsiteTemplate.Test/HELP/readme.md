# Common Issues

## ANCM Out-Of-Process

You might get the following error when deploying a dotnet application:  

```
HTTP Error 502.5 - ANCM Out-Of-Process Startup Failure
Common solutions to this issue:
The application process failed to start
The application process started but then stopped
The application process started but failed to listen on the configured port
Troubleshooting steps:
Check the system event log for error messages
Enable logging the application process' stdout messages
Attach a debugger to the application process and inspect
For more guidance on diagnosing and handling these errors, visit Troubleshoot ASP.NET Core on Azure App Service and IIS.
```  

**The solution**:  
The solution is to make sure your web.config file is copied over and that the "processPath" is not the exe file, but rather set to "dotnet".  
  
One way to make sure it doesn't set it to the exe is to have the exe removed from the bin folder.  
An example ItemGroup from a csproj file:  
```xml
<ItemGroup>
    <Content Remove="bin\Release\net8.0\Marketplace.exe" />
    <None Remove="bin\Release\net8.0\Marketplace.exe" />
  </ItemGroup>
```
And:  
```xml
 <Target Name="RemoveExeAfterPublish" AfterTargets="Publish">
    <Delete Files="$(PublishDir)\Marketplace.exe" />
  </Target>
```

And also make sure your web.config file is deployed:  
```xml
<Content Include="Web.config">
      <CopyToPublishDirectory>Always</CopyToPublishDirectory>
    </Content>
```
