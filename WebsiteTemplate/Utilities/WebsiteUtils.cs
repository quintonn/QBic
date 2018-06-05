using NHibernate;
using System.IO;
using System.Text;
using System.Web;
using WebsiteTemplate.Models;

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

        public static string GetCurrentRequestData()
        {
            using (var stream = HttpContext.Current.Request.InputStream)
            using (var mem = new MemoryStream())
            {
                stream.CopyTo(mem);
                var res = Encoding.UTF8.GetString(mem.ToArray());
                return res;
            }
        }

        public static string GetCurrentRequestUrl()
        {
            var request = HttpContext.Current.Request.RequestContext.HttpContext.Request;

            var uri = request.Url;

            var result = uri.Scheme + "://" + uri.Host + request.ApplicationPath;
            return result;
        }

        //public static void SetCurrentUser(string userName)
        //{
        //    HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(userName), new string[] { });
        //}
    }
}