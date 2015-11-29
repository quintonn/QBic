using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            var currentDir = Environment.CurrentDirectory + @"\..\";
            var webPath = currentDir + @"Frontend\webFiles\";

            //Debug.Assert(false);
            SquishIt.Framework.CSS.CSSBundle x = new SquishIt.Framework.CSS.CSSBundle();
            x.AddDirectory(currentDir, true);

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

            var result = x.Render(cssFileName);
            Console.WriteLine(result);

            SquishIt.Framework.JavaScript.JavaScriptBundle j = new SquishIt.Framework.JavaScript.JavaScriptBundle();
            j.AddDirectory(currentDir, true);
            result = j.Render(jsFileName);

            Console.WriteLine(result);
        }
    }
}
