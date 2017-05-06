using Microsoft.Owin;
using System;
using System.Threading.Tasks;

namespace WebsiteTemplate.Utilities
{
    public class GlobalExceptionMiddleware : OwinMiddleware
    {
        public GlobalExceptionMiddleware(OwinMiddleware next, string x, string y, int xx) 
            : base(next)
        {
            Console.WriteLine("");
        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                await Next.Invoke(context);
            }
            catch (Exception ex)
            {
                //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
                // your handling logic
            }
        }
    }
}