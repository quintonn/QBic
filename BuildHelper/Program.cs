using System;
using System.Collections.Generic;
using System.Diagnostics;
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


            var result = x.Render(webPath + "styles.min.css");
            Console.WriteLine(result);

            SquishIt.Framework.JavaScript.JavaScriptBundle j = new SquishIt.Framework.JavaScript.JavaScriptBundle();
            j.AddDirectory(currentDir, true);
            result = j.Render(webPath + "scripts.min.js");

            Console.WriteLine(result);
        }
    }
}
