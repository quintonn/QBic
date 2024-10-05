using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace QBic.Core.Utilities
{
    /// <summary>
    /// This class contanis many utilities to help common .net problems.
    /// </summary>
    public static class QBicUtils
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

        public static string GetCurrentDirectory()//IHostingEnvironment env)
        {
            try
            {
                //return HttpRuntime.AppDomainAppPath;
                //string contentRootPath = _env.ContentRootPath;
                //string webRootPath = _env.WebRootPath;

                //return contentRootPath + "\n" + webRootPath;
                return Directory.GetCurrentDirectory();
            }
            catch (ArgumentNullException)
            {
                return Directory.GetCurrentDirectory();
            }
        }

        public static void SendEmailWithAttachments(string body, IList<string> recipients, string subject, string emailHost, int emailPort, string fromEmailUser, string fromEmailPassword, bool enableSsl, bool isHtmlBody = false, params Attachment[] attachments)
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
            attachments.ToList().ForEach(a =>
            {
                mailMessage.Attachments.Add(a);
            });
            
            foreach (var recipient in recipients.Skip(1).ToList()) // skip 1 because first recipient is in mail message constructor
            {
                mailMessage.To.Add(recipient);
            }

            smtpClient.Send(mailMessage);
        }

        public static void SendEmail(string body, IList<string> recipients, string subject, string emailHost, int emailPort, string fromEmailUser, string fromEmailPassword, bool enableSsl, bool isHtmlBody = false)
        {
            SendEmailWithAttachments(body, recipients, subject, emailHost, emailPort, fromEmailUser, fromEmailPassword, enableSsl, isHtmlBody);
        }
        
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return GetString(base64EncodedBytes);
        }
    }
}