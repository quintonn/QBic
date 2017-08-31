using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Web;
using WebsiteTemplate.Data.BaseTypes;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Utilities
{
    /// <summary>
    /// Replace the name XXX with a name of my framework????
    /// TOOD: ???
    /// </summary>
    public class XXXUtils
    {
        public static byte[] GetBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        public static string GetString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string GetCurrentDirectory()
        {
            return HttpRuntime.AppDomainAppPath;
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

        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        static extern int FindMimeFromData(IntPtr pBC,
                                            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
                                            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.I1, SizeParamIndex=3)]
                                            byte[] pBuffer,
                                            int cbSize,
                                            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
                                            int dwMimeFlags,
                                            out IntPtr ppwzMimeOut,
                                            int dwReserved
            );


        public static string GetMimeFromBytes(byte[] dataBytes)
        {
            if (dataBytes == null)
            {
                throw new ArgumentNullException("dataBytes");
            }
            var mimeRet = String.Empty;
            var suggestPtr = IntPtr.Zero;
            var filePtr = IntPtr.Zero;
            var outPtr = IntPtr.Zero;
            var mimeProposed = "text/plain";
            var ret = FindMimeFromData(IntPtr.Zero, null, dataBytes, dataBytes.Length, mimeProposed, 0, out outPtr, 0);
            if (ret == 0 && outPtr != IntPtr.Zero)
            {
                //todo: this leaks memory outPtr must be freed
                return Marshal.PtrToStringUni(outPtr);
            }
            return mimeRet;
        }

        public static Version GetApplicationCoreVersion()
        {
            return typeof(ShowView).Assembly.GetName().Version;
        }

        public static void SetCurrentUser(string userName)
        {
            HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(userName), new string[] { });
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

        public static void SendEmail(string body, IList<string> recipients, string subject, string emailHost, int emailPort, string fromEmailUser, string fromEmailPassword, bool enableSsl, bool isHtmlBody = false)
        {
            var smtpClient = new System.Net.Mail.SmtpClient(emailHost, emailPort);

            smtpClient.Credentials = new System.Net.NetworkCredential(fromEmailUser, fromEmailPassword);
            smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = enableSsl;

            if (recipients == null || recipients.Count == 0)
            {
                throw new Exception("No recipients specified for sending email.");
            }
            var mailMessage = new System.Net.Mail.MailMessage(fromEmailUser, recipients.First(), subject, body);
            mailMessage.IsBodyHtml = isHtmlBody;
            foreach (var recipient in recipients.Skip(1).ToList())
            {
                mailMessage.To.Add(recipient);
            }

            smtpClient.Send(mailMessage);
        }

        internal static string DateFormat { get; set; }

        public static string GetDateFormat()
        {
            return DateFormat;
        }

        public static List<Type> GetAllBaseClassTypes(Type applicationStartupType)
        {
            var baseTypes = Assembly.GetAssembly(typeof(Models.User)).GetTypes().Where(t => t.IsClass && t.IsSubclassOf(typeof(BaseClass)) && t.IsAbstract == false).ToList();
            //var siteTypes = Assembly.GetCallingAssembly().GetTypes().Where(t => t.IsClass && t.IsSubclassOf(typeof(BaseClass))).ToList();
            var siteTypes = Assembly.GetAssembly(applicationStartupType).GetTypes().Where(t => t.IsClass && t.IsSubclassOf(typeof(BaseClass)) && t.IsAbstract == false).ToList();

            var allTypes = baseTypes.Union(siteTypes).ToList();

            var result = new List<Type>();

            foreach (var type in allTypes)
            {
                ProcessType(type, result);
            }
            
            return result;
        }

        private static List<Type> ProcessingTypes { get; set; } = new List<Type>();

        private static void ProcessType(Type type, List<Type> sortedTypes)
        {
            /* Only process BaseClass classes and don't repeat any */
            if (!type.IsSubclassOf(typeof(BaseClass)) || sortedTypes.Contains(type))
            {
                return;
            }

            /* Classes to explicitly ignore */
            if (type == typeof(DynamicClass))
            {
                return;
            }

            if (ProcessingTypes.Contains(type))
            {
                return;
            }
            ProcessingTypes.Add(type);

            var properties = type.GetProperties().Where(p => p.PropertyType.IsClass && 
                                                             p.PropertyType.IsSubclassOf(typeof(BaseClass)) && 
                                                             p.PropertyType != type).ToList();
            foreach (var property in properties)
            {
                var pType = property.PropertyType;
                ProcessType(pType, sortedTypes);
            }

            ProcessingTypes.Remove(type);
            sortedTypes.Add(type);
        }

        internal static bool IsPrimitive(Type t)
        {
            // TODO: put any type here that you consider as primitive as I didn't
            // quite understand what your definition of primitive type is
            return new[] {
                typeof(string),
                typeof(char),
                typeof(byte),
                typeof(System.Byte[]),
                typeof(sbyte),
                typeof(ushort),
                typeof(short),
                typeof(uint),
                typeof(int),
                typeof(ulong),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTime?),
                typeof(LongString)
            }.Contains(t) || t.IsPrimitive || t.IsEnum;
        }
    }
}