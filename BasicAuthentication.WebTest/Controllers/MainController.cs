//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using BasicAuthentication.WebTest.Models;
using System.Web.Http;

namespace BasicAuthentication.WebTest.Controllers
{
    [RoutePrefix("api/v1")]
    public class MainController : ApiController
    {
        static MainController()
        {
            var steve = new TestUser()
            {
                UserName = "Steve",
                Email = "steve@gmail.com",
                EmailConfirmed = true
            };
            
        }

        [HttpGet]
        [Route("initializeSystem")]
        [Authorize]
        public IHttpActionResult InitializeSystem()
        {
            var json = new
            {
                LoggedIn = true
            };
            return Json(json);
        }

        [HttpGet]
        [Route("pingTest")]
        [Authorize]
        public IHttpActionResult PingTest()
        {
            var json = new
            {
                Success = true
            };
            return Json(json);
        }
    }
}