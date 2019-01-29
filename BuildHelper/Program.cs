﻿using SquishIt.AspNet;
using SquishIt.AspNet.Caches;
using SquishIt.AspNet.Utilities;
using SquishIt.Framework;
using SquishIt.Framework.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;

namespace BuildHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildWebsiteTemplate();
        }

        private static void BuildWebsiteTemplate()
        {
            var currentDir = Environment.CurrentDirectory + @"\..\..\";
            var webPath = currentDir + @"WebsiteTemplateCore\FrontEnd\";
            //D:\Projects\WebsiteTemplate\WebsiteTemplate\Frontend

            //if (Debugger.IsAttached == false) Debugger.Launch();

            /// Delete existing files
            var cssFileName = webPath + "styles.min.css";
            if (File.Exists(cssFileName))
            {
                File.Delete(cssFileName);
            }
            var jsFileName = webPath + "scripts.min.js";
            if (File.Exists(jsFileName))
            {
                File.Delete(jsFileName);
            }

            /// Create minified style sheet

            Configuration.Instance.Platform = new PlatformConfiguration();
            Configuration.Instance.Platform.CacheImplementation = new CacheImplementation();
            Configuration.Instance.Platform.DebugStatusReader = new DebugStatusReader();
            Configuration.Instance.Platform.TrustLevel = new MyTrustLevel();
            Configuration.Instance.Platform.PathTranslator = new PathTranslator();

            var cssBundle = Bundle.Css();// new SquishIt.Framework.CSS.CSSBundle();
            var cssFiles = Directory.GetFiles(currentDir + "\\WebsiteTemplate\\FrontEnd\\css", "*.*", SearchOption.AllDirectories);
            foreach (var file in cssFiles)
            {
                if (file.Contains("siteOverrides"))
                {
                    continue;
                }
                cssBundle.Add(file);
            }
            //cssBundle.AddDirectory(currentDir + "\\WebsiteTemplate\\FrontEnd\\css", true);
            
            var result = cssBundle.Render(cssFileName);
            var cssFile = File.ReadAllText(cssFileName);
            cssFile = cssFile.Replace("/WebsiteTemplate", "");
            File.WriteAllText(cssFileName, cssFile);
            Console.WriteLine(result);

            /// Create minified javascript
            var jsBundle = new SquishIt.Framework.JavaScript.JavaScriptBundle();
            
            jsBundle.Add(currentDir + "\\WebsiteTemplate\\FrontEnd\\Scripts\\jquery-3.1.0.min.js");
            jsBundle.AddDirectory(currentDir + "\\WebsiteTemplate\\FrontEnd\\Scripts", true);
            
            result = jsBundle.Render(jsFileName);

            /// Copy html pages in Pages folder
            var pages = Directory.GetFiles(currentDir + "WebsiteTemplate\\FrontEnd\\Pages").ToList();
            if (!Directory.Exists(currentDir + "WebsiteTemplateCore\\FrontEnd\\Pages"))
            {
                Directory.CreateDirectory(currentDir + "WebsiteTemplateCore\\FrontEnd\\Pages");
            }
            pages.ForEach(p =>
            {
                var newFile = currentDir + "WebsiteTemplateCore\\FrontEnd\\Pages\\" + p.Split("\\".ToCharArray()).Last();
                File.Copy(p, newFile, true);
            });

            /// Copy font files
            var fonts = Directory.GetFiles(currentDir + "WebsiteTemplate\\FrontEnd\\fonts").ToList();
            if (!Directory.Exists(currentDir + "WebsiteTemplateCore\\FrontEnd\\fonts"))
            {
                Directory.CreateDirectory(currentDir + "WebsiteTemplateCore\\FrontEnd\\fonts");
            }
            fonts.ForEach(f =>
            {
                var newFile = currentDir + "WebsiteTemplateCore\\FrontEnd\\fonts\\" + f.Split("\\".ToCharArray()).Last();
                File.Copy(f, newFile, true);
            });

            /// Copy site specific css file
            // Let's not do this anymore
            // File.Copy(currentDir + "WebsiteTemplate\\FrontEnd\\css\\siteOverrides.css", currentDir + "WebsiteTemplateCore\\FrontEnd\\css\\siteOverrides.css", true);


            /// Copy and Edit Index.html page
            var indexPath = currentDir + "WebsiteTemplateCore\\Index.html";
            
            File.Copy(currentDir + "WebsiteTemplate\\Index.html", indexPath, true);

            var data = File.ReadAllText(indexPath);
            var regex = new Regex("<link href.*/>?");
            var matches = regex.Matches(data);

            //TODO: Get version from assembly
            var randomVersion = Guid.NewGuid().ToString();
            randomVersion = HttpUtility.UrlEncode(randomVersion);

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (i == 0)
                {
                    data = data.Replace(match.Value, "<link href='Frontend/styles.min.css?v=" + randomVersion + "' rel='stylesheet' />");
                }
                else
                {
                    if (match.Value.Contains("siteOverrides.css"))
                    {
                        continue;
                    }
                    data = data.Replace(match.Value, "");
                }
            }

            regex = new Regex("<script.*</script>?");
            matches = regex.Matches(data);
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (i == 0)
                {
                    data = data.Replace(match.Value, "<script src='Frontend/scripts.min.js?v=" + randomVersion + "'></script>");
                }
                else
                {
                    data = data.Replace(match.Value, "");
                }
            }

            var lines = data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            lines = lines.Where(l => !String.IsNullOrWhiteSpace(l)).ToList();
            data = String.Join("\r\n", lines.ToArray());

            File.WriteAllText(indexPath, data);

            // Copy css images
            var imagePath = currentDir + "\\WebsiteTemplate\\FrontEnd\\css\\images";
            var coreImagePath = webPath + "css\\images";

            if (!Directory.Exists(coreImagePath))
            {
                Directory.CreateDirectory(coreImagePath);
            }
            var images = Directory.GetFiles(imagePath).ToList();
            images.ForEach(i =>
            {
                var imageName = Path.GetFileName(i);
                var newPath = coreImagePath + "\\" + imageName;
                if (!File.Exists(newPath))
                {
                    File.Copy(i, newPath);
                }
            });


            Console.WriteLine(result);
        }
    }
}