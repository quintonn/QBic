using BasicAuthentication.Users;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Services
{
    public class UserService
    {
        private DataService DataService { get; set; }
        private IApplicationSettings ApplicationSettings { get; set; }

        public UserService(DataService dataService, IApplicationSettings appSettings)
        {
            DataService = dataService;
            ApplicationSettings = appSettings;
        }

        public List<UserRole> GetUserRoles()
        {
            using (var session = DataService.OpenSession())
            {
                return session.QueryOver<UserRole>().List().ToList();
            }
        }

        public async Task<string> CreateUser(string userName, string email, string password, List<string> userRoles)
        {
            var user = new User(true)
            {
                Email = email,
                UserName = userName
            };

            var result = await CoreAuthenticationEngine.UserManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return "Unable to create user:\n" + String.Join("\n", result.Errors);
            }

            using (var session = DataService.OpenSession())
            {
                foreach (var role in userRoles)
                {
                    var dbUserRole = session.Get<UserRole>(role.ToString());

                    var roleAssociation = new UserRoleAssociation()
                    {
                        User = user,
                        UserRole = dbUserRole
                    };
                    DataService.SaveOrUpdate(roleAssociation);
                }
                session.Flush();
            }

            try
            {
                var emailResult = await SendEmail(user.Id, userName, email);

                return emailResult;
            }
            catch (FormatException e)
            {
                return "User created but there was an error sending activation email:\n" + e.Message;
            }
        }

        public async Task<string> SendEmail(string userId, string userName, string emailAddress)
        {
            var smtp = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
            if (smtp == null)
            {
                return await Task.FromResult(String.Empty);
            }

            var emailToken = CoreAuthenticationEngine.UserManager.GenerateEmailConfirmationTokenAsync(userId).Result;

            var myuri = new Uri(System.Web.HttpContext.Current.Request.Url.AbsoluteUri);

            var body = "Hi " + userName;

            body += "\nWelcome to " + ApplicationSettings.GetApplicationName();

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
                    System.Diagnostics.Trace.WriteLine(message);
                    System.Diagnostics.Debug.WriteLine(message);
                    return message;
                }
                return String.Empty;
            });
            return await sendEmailTask;
        }

        private string GetCurrentUrl()
        {
            var request = HttpContext.Current.Request.RequestContext.HttpContext.Request;

            var uri = request.Url;

            var result = uri.Scheme + "://" + uri.Host + request.ApplicationPath;
            return result;
        }

        public User RetrieveUser(string userId)
        {
            using (var session = DataService.OpenSession())
            {
                return session.Get<User>(userId);
            }
        }

        public void DeleteUser(string userId)
        {
            using (var session = DataService.OpenSession())
            {
                var user = session.Get<User>(userId);

                var userRoles = session.CreateCriteria<UserRoleAssociation>()
                                       .CreateAlias("User", "user")
                                       .Add(Restrictions.Eq("user.Id", userId))
                                       .List<UserRoleAssociation>()
                                       .ToList();
                userRoles.ForEach(u =>
                {
                    DataService.TryDelete(u);
                });

                DataService.TryDelete(user);
                session.Flush();
            }
        }

        public User FindUserByUserName(string userName)
        {
            using (var session = DataService.OpenSession())
            {
                return session.QueryOver<User>().Where(u => u.UserName == userName).SingleOrDefault();
            }
        }

        public void UpdateUser(string userId, string userName, string email, List<string> userRoles)
        {
            using (var session = DataService.OpenSession())
            {
                var dbUser = session.Get<User>(userId);
                dbUser.UserName = userName;

                dbUser.EmailConfirmed = dbUser.Email == email;
                dbUser.Email = email;
                DataService.SaveOrUpdate(dbUser);

                var existingUserRoles = session.CreateCriteria<UserRoleAssociation>()
                                               .CreateAlias("User", "user")
                                               .Add(Restrictions.Eq("user.Id", dbUser.Id))
                                               .List<UserRoleAssociation>()
                                               .ToList();
                existingUserRoles.ForEach(u =>
                {
                    DataService.TryDelete(u);
                });

                foreach (var role in userRoles)
                {
                    var dbUserRole = session.Get<UserRole>(role.ToString());

                    var roleAssociation = new UserRoleAssociation()
                    {
                        User = dbUser,
                        UserRole = dbUserRole
                    };
                    DataService.SaveOrUpdate(roleAssociation);
                }

                session.Flush();
            }
        }

        public IList<UserRoleAssociation> RetrieveUserRoleAssocationsForUserId(string userId)
        {
            using (var session = DataService.OpenSession())
            {
                var existingItems = session.CreateCriteria<UserRoleAssociation>()
                                           .CreateAlias("User", "user")
                                           .Add(Restrictions.Eq("user.Id", userId))
                                           .List<UserRoleAssociation>();
                return existingItems;
            }
        }

        public List<User> RetrieveUsers(int currentPage, int linesPerPage, string filter)
        {
            using (var session = DataService.OpenSession())
            {
                return CreateUserQuery(session, filter)
                                     .Skip((currentPage - 1) * linesPerPage)
                                     .Take(linesPerPage)
                                     .List<User>()
                                     .ToList();
            }
        }

        public int RetrieveUserCount(string filter)
        {
            using (var session = DataService.OpenSession())
            {
                return CreateUserQuery(session, filter).RowCount();
            }
        }

        private IQueryOver<User> CreateUserQuery(ISession session, string filter)
        {
            return session.QueryOver<User>().Where(Restrictions.On<User>(x => x.UserName).IsInsensitiveLike(filter, MatchMode.Anywhere) ||
                                                   Restrictions.On<User>(x => x.Email).IsInsensitiveLike(filter, MatchMode.Anywhere));
        }
    }
}