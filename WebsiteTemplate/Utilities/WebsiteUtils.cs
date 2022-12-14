using Microsoft.AspNetCore.Http;
using NHibernate;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using WebsiteTemplate.Models;
using ISession = NHibernate.ISession;

namespace WebsiteTemplate.Utilities
{
    public static class WebsiteUtils
    {
        internal static string DateFormat { get; set; }

        public static string GetSystemDateFormat()
        {
            return DateFormat;
        }

        public static string GetSystemSetting(ISession session, string settingKey, string valueIfSettingNotFound = null)
        {
            var result = valueIfSettingNotFound;
            var dbSetting = session.QueryOver<SystemSettingValue>().Where(s => s.KeyName == settingKey).SingleOrDefault();
            if (dbSetting != null)
            {
                result = dbSetting.Value;
            }
            return result;
        }

        public static string GetSystemSetting(IStatelessSession session, string settingKey, string valueIfSettingNotFound = null)
        {
            var result = valueIfSettingNotFound;
            var dbSetting = session.QueryOver<SystemSettingValue>().Where(s => s.KeyName == settingKey).SingleOrDefault();
            if (dbSetting != null)
            {
                result = dbSetting.Value;
            }
            return result;
        }

        public static async Task<string> GetCurrentRequestData(IHttpContextAccessor httpContextAccessor)
        {
            using (var reader = new StreamReader(httpContextAccessor.HttpContext.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                var bodyStr = await reader.ReadToEndAsync();
                httpContextAccessor.HttpContext.Request.Body.Position = 0;
                return bodyStr;
            }

            //using (var stream = HttpContext.Current.Request.InputStream)
            //using (var mem = new MemoryStream())
            //{
            //    await httpContextAccessor.HttpContext.Request.Body.CopyToAsync(mem);
            //    //stream.CopyTo(mem);
            //    var res = Encoding.UTF8.GetString(mem.ToArray());
            //    return res;
            //}
        }

        public static string GetCurrentRequestUrl(IHttpContextAccessor httpContextAccessor)
        {
            var request = httpContextAccessor.HttpContext.Request;// HttpContext.Current.Request.RequestContext.HttpContext.Request;

            var result = request.Scheme + "://" + request.Host + request.PathBase;
            return result;
        }

        public static void SetCurrentUser(string userName, IHttpContextAccessor httpContextAccessor)
        {
            httpContextAccessor.HttpContext.User = new GenericPrincipal(new GenericIdentity(userName), new string[] { });
            //HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(userName), new string[] { });
        }
    }
}