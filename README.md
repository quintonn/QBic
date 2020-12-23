[![Build Status](https://travis-ci.com/quintonn/qBic.svg?branch=master)](https://travis-ci.com/quintonn/qBic)

##### Table of Contents  
1. [Introduction](#introduction)  
1. [Goal](#goal)  
1. [Features](#features)  
1. [Prerequisites](#prerequisites)  
1. [Creating a new qBic Project](#creating-a-new-qBic-project)  
5.1. [Creating a project Manually](#creating-a-project-manually)  
5.2. [Using the Custom qBic Project Template](#using-the-custom-qBic-project-template)  
1. [Documentation](#documentation)
1. [Sample Code](#sample-code)
1. [Known Problems](#known-problems)  
8.1 [SQLite.Interop.dll](#sqLiteinteropdll)  
8.2 [Unable to delete SQLite dll](#unable-to-delete-sqLite-dll)  
1. [Projects using qBic](#projects-using-qBic)

# Introduction
A platform for creating Web Servers or CMS systems in .Net 5  
The front-end, part is dynamically created based on .Net code-configuration.  
This does not rely on blazor, silverlight, web-assembly or any of those frameworks, but uses normal JS, HTML and CSS underneath.  
The easiest way to demonstrate what qBic offers, is by showing [an example](#sample-code).  

With qBic, you can create admin portals, dashboards, CMS sites, identity servers, and much more.  
All of this while only writing .Net code.  
No HTML, JavaScript or CSS.  
A lot of the **magic** is driven by abstract classes and inheritance.  

A sample of how quick and easy it is to create a new .Net project using qBic:  
![Quick qBic Demo](https://github.com/quintonn/qBic/raw/master/Images/qBicQuickDemo.gif "Quick qBic Demo")

# Goal
The goal of this framework/platform is to never have to worry about the front-end, web, code.  
I wasn't very good with JavaScript or HTML when I started this project, and I disliked having to write and debug code in the browser when all I wanted to do was work on the back-end C# code.  
So I created this Framework so I didn't have to repeat the same work over and over again.  
Not only did I make this framework create the UI for me, but it also adds a number of commonly used [features](#Features).  

One of my main goals was to allow **a single developer** to build a relatively large and complex system.  
And I believe qBic has achieved that.

# Features
Any **qBic Application** will get all of the following features out of the box, without having to write any extra code:
1. Database synchronization, using NHibernate
2. Auditing
3. User repository
4. Authentication and Authorization
5. Email based account confirmation
6. Email based password reset functionality
7. Role based access control
8. Customizable menu system

# Prerequisites
Before using and running qBic, the following should be installed or adhered to:
1. You have .net 5 installed on your development machine.  
2. You run Visual Studio as an administrator when working on qBic projects.

**TODO:** Creating a new project from this template is not quite yet possible because the wwwroot code is not transferred yet

# Creating a new qBic Project
You can either choose to setup a new qBic project [manually](#creating-a-project-manually) , or  
You can support me and purchase my custom [Visual Studio Project Template](https://github.com/quintonn/qBic#using-the-custom-qBic-project-template).  

## Creating a project Manually
1. Create a new Visual Studio ASP.NET Core Web Application
1. Choose ASP.NET Core Empty and ASP.NET Core 5.0, and leave all other defaults
1. Install the WebsiteTemplate nuget package, either from Visual Studio, or by running the following command:  
   ```
   Install-Package WebsiteTemplate
   ```
1. Create a new class that inherits from **ApplicationStartup**
1. Implement the mandatory functions and create the constructor required by **ApplicationStartup** parent class  
   The **SetupDefaults** is a good place to create your default, or admin, user  
   This is actually required, else you will not be able to log into your application when it starts for the first time  
   The Test project's [Startup](WebsiteTemplate.Test/Startup.cs) file can be used to see a basic example of adding an admin/first user
1. Add a class that inherits from **ApplicationSettingsCore**  
   I usually call this file **AppSettings**, but it can be anything
1. Implement the mandatory properties and methods of the **ApplicationSettingsCore** class  
   **GetApplicationStartupType** should return the type of class you created in step #5
1. Set the override field **UpdateDatabase** to **true**  
   This will create the database and tables, and can be set to false once you have your database set up and read
1. Make the generated **Startup.cs** class look as follows (where AppSettings and AppStartup be replaced with the files you created earlier):  

   ```cs
   using Microsoft.AspNetCore.Builder;
   using Microsoft.Extensions.Configuration;
   using Microsoft.Extensions.DependencyInjection;
   using Microsoft.Extensions.Logging;
   using qBic.Core.Utilities;
   using System;
   using WebsiteTemplate;
   
   namespace SampleProject
   {
   	public class Startup
   	{
   		public static IConfiguration Config;
   
   		public Startup(IConfiguration config)
   		{
   			Config = config;
   		}
   		public void ConfigureServices(IServiceCollection services)
   		{
   			services.UseqBic<AppSettings, AppStartup>(Config);
   		}
   
   		public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, ILoggerFactory logFactory)
   		{
   			// Setup internal logging system. Inherited from .net 4
   			SystemLogger.Setup(logFactory);
   
   			app.UseqBic(serviceProvider);
   		}
   	}
   }

   ```
1. Create a new file called "siteOverrides.css" in **wwwroot/css**  
   You can populate it as follows to start with:  
   
   ```css
   .w3-app-color, .w3-hover-app-color {
   	color: #fff !important;
   	background-color: #2196F3 !important;
   }
   .w3-hover-app-color {
   	-webkit-transition: background-color .3s,color .15s,box-shadow .3s,opacity 0.3s;
   	transition: background-color .3s,color .15s,box-shadow .3s,opacity 0.3s;
   }
   ```  
1. You should now be ready to run the application  
   It might take a couple of minutes (but not more than 5) to run the first time as it sets up the database  
   But eventually you should be presented with a login screen if you have followed all the steps correctly, as follows:  
   ![Login Prompt](First_Login.png "Successful Login Prompt")
	
## Using the Custom qBic Project Template
You can purchase the Visual Studio Template [<img src="qBic.png" width="30px">](https://gum.co/SsWHZE) for **$10** from [Gumroad](https://gum.co/SsWHZE).  

The default username and password is admin/password if you've used the qBic Project Template.  


# Documentation
Documentation will be added over the course of time.  

But, for the time being, there is a [qBic Samples](https://github.com/quintonn/qBicSamples) repository with examples of using the qBic platform. 

# Sample Code
Consider the following code.  
This is all the code required to create basic CRUD functionality for a **Category** class, including the view and input screens and **all** the logic to actually view, create, edit and delete a category.  
```c#
public class CategoryCrudItem : BasicCrudMenuItem<Category>
{
    public CategoryCrudItem()
    {
    }

    public override bool AllowInMenu => true;

    public override string GetBaseItemName()
    {
        return "Category";
    }

    public override EventNumber GetBaseMenuId()
    {
        return 6530; // An internal number to uniquely identify this activity/event
    }

    public override Dictionary<string, string> GetColumnsToShowInView()
    {
        var res = new Dictionary<string, string>();
        res.Add("Name", "Name");
        res.Add("Description", "Description");
        return res;
    }

    public override Dictionary<string, string> GetInputProperties()
    {
        var res = new Dictionary<string, string>();
        res.Add("Name", "Name");
        res.Add("Description", "Description");
        return res;
    }
}    
```
And below is the view and the input screen this code generates:  
### Screen View
![View of Categories](Category_View.png "View of categories")  
### Input Screen
![Categories input screen](Edit_Category.png "Modifying a category")

# Known Problems
### SQLite.Interop.dll
Sometimes you might see this error when you try and run your qBic application while using SQLite 
> Unable to load DLL 'SQLite.Interop.dll': The specified module could not be found  

There are many reasons for seeing this error and we have made many efforts to fix them, but some reasons still persist:
1. The **Identity** of the **AppPool** in **IIS** that runs the qBic website does not have access to the location the project was created in.  

   This might be happen if the **AppPool** is **DefaultAppPool** and the **Identity** is **NetworkService** and the project is created inside the **C:\Users\XXXX\source\repos** folder which are the defaults for Visual Studio and IIS.  

   The easiest fix for this is to create or move the project into a different location (e.g. c:\MyProjects\).  
   Another solution might be to change the **Identity** of the appPool, or give it access to the source\repos folder.

### Unable to delete SQLite dll
When using SQLite (instead of MS-SQL) you might see an error when building your applicatio that the SQLite dll could not be removed.  
As mentioned above, many steps have been taken to eliminate this error, but it might still ocurr from time to time.  

The simplest solution for this has been to run the following command in an elevated command prompt:  
> iisreset

# Projects using qBic
Some projects that have used, or are using, qBic are listed below.  
Feel free to add your project too:  
1. The back-end for a custom mobile application for Bargain Books in South Africa  
   *This has been discontinued*
2. [Repo Castle](https://repocastle.com/)  
   A private NuGet repository with fine-grained access control
3. The [qBic Samples](https://github.com/quintonn/qBicSamples) application  
   This project shows example of features of qBic
4. A custom school CMS.  
   The system keeps track of studendts' attendance & grades, and also generate and prints report cards.  
   
