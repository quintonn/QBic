﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Web;
using WebsiteTemplate.Menus.ViewItems;

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
    }
}