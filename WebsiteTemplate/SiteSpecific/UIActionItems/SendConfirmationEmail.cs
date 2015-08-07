using BasicAuthentication.Users;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.Utilities;
using System.Web.Http;
using System.Net.Mail;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace WebsiteTemplate.SiteSpecific.UIActionItems
{
    public class SendConfirmationEmail : DoSomething
    {
        public override int Id
        {

            get
            {
                return UIActionNumbers.SEND_CONFIRMATION_EMAIL;
            }
        }

        public override string Name
        {
            get
            {
                return "Send Confirmation Email";
            }
        }

        public override string Description
        {
            get
            {
                return "Sends a confirmation email to a new user";
            }
        }

        public override string MenuLabel
        {
            get
            {
                return String.Empty;
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>()
                {
                    UserRole.SendConfirmationEmail
                };
            }
        }

        public override async Task<IList<UIAction>> ProcessAction(string data)
        {
            var results = new List<UIAction>();

            var emailSentResultMessage = String.Empty;
            var id = data;

            using (var session = Store.OpenSession())
            {
                var user = session.Get<User>(id);
                try
                {
                    emailSentResultMessage = await SendEmail(user.Id, user.UserName, user.Email);
                }
                catch (FormatException e)
                {
                    emailSentResultMessage = e.Message;
                }
            }

            if (String.IsNullOrWhiteSpace(emailSentResultMessage))
            {
                results.Add(new ShowMessage("Email confirmation sent successfully"));
            }
            else
            {
                var result = new ShowMessage("Email confirmation could not be sent\n" + emailSentResultMessage);
                results.Add(result);
            }
            
            return results;
        }

        private async Task<string> SendEmail(string userId, string userName, string emailAddress)
        {
            var smtp = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
            if (smtp == null)
            {
                Trace.WriteLine("No system.net/mailSettings/smtp section in web.config or app.config");
                return await Task.FromResult(String.Empty);
            }

            var emailToken = CoreAuthenticationEngine.UserManager.GenerateEmailConfirmationTokenAsync(userId).Result;

            var myuri = new Uri(System.Web.HttpContext.Current.Request.Url.AbsoluteUri);

            var body = "Hi " + userName;
            body += "\nWelcome to " + WebsiteTemplateConstants.ApplicatoinName;

            body += "\n\nPlease click on the following link to activate and activate your email:\n";

            body += GetCurrentUrl() + "/api/v1/menu/ConfirmEmail?userId=" + userId + "&token=" + HttpUtility.UrlEncode(emailToken);

            var mailMessage = new MailMessage(smtp.From, emailAddress, "Email Confirmation", body);

            var sendEmailTask = Task.Run(() =>
            {
                try
                {
                    var smtpClient = new SmtpClient(smtp.Network.Host, smtp.Network.Port);

                    smtpClient.Credentials = new System.Net.NetworkCredential(smtp.Network.UserName, smtp.Network.Password);
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = smtp.Network.EnableSsl;

                    smtpClient.Send(mailMessage);
                }
                catch (Exception e)
                {
                    var message = e.Message + "\n" + e.ToString();
                    Console.WriteLine(message);
                    Trace.WriteLine(message);
                    Debug.WriteLine(message);
                    return message;
                }
                return String.Empty; ;
            });
            return await sendEmailTask;
        }

        private string GetCurrentUrl()
        {
            var request = Request.GetRequestContext();
            var uri = request.Url.Request.RequestUri;
            var result = uri.Scheme + "://" + uri.Host + request.VirtualPathRoot;
            return result;
        }
    }
}