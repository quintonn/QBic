﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;
using ISession = NHibernate.ISession;

namespace WebsiteTemplate.Backend.Services
{
    public class UserService
    {
        private DataService DataService { get; set; }
        private ApplicationSettingsCore ApplicationSettings { get; set; }
        private static readonly ILogger Logger = SystemLogger.GetLogger<UserService>();

        private UserManager<User> UserManager { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }

        public UserService(DataService dataService, ApplicationSettingsCore appSettings, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            DataService = dataService;
            ApplicationSettings = appSettings;
            UserManager = userManager;
            HttpContextAccessor = httpContextAccessor;
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

            using (var session = DataService.OpenSession())
            {
                var result = await UserManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    return "Unable to create user:\n" + String.Join("\n", result.Errors.Select(x => x.Description).ToList());
                }

                foreach (var role in userRoles)
                {
                    var dbUserRole = session.Get<UserRole>(role.ToString());

                    var roleAssociation = new UserRoleAssociation()
                    {
                        User = user,
                        UserRole = dbUserRole
                    };

                    DataService.SaveOrUpdate<UserRoleAssociation>(session, roleAssociation);
                }

                session.Flush();
            }
            try
            {
                var emailResult = await SendAcccountConfirmationEmail(user.Id, userName, email);

                return emailResult;
            }
            catch (FormatException e)
            {
                return "User created but there was an error sending activation email:\n" + e.Message;
            }
        }

        public virtual async Task<string> SendPasswordResetLink(string userNameOrEmail)
        {
            Models.SystemSettings settings;
            using (var session = DataService.OpenSession())
            {
                settings = session.QueryOver<Models.SystemSettings>().SingleOrDefault<Models.SystemSettings>();
            }

            if (settings == null)
            {
                throw new Exception("No system settings have been setup.");
            }

            var user = await UserManager.FindByNameAsync(userNameOrEmail) as User;

            if (user == null)
            {
                user = await UserManager.FindByEmailAsync(userNameOrEmail) as User;
            }

            var emailStatus = String.Empty;

            if (user != null)
            {
                var passwordResetLink = await UserManager.GeneratePasswordResetTokenAsync(user);

                using var session = DataService.OpenSession();

                var dbToken = new PasswordResetToken()
                {
                    Token = passwordResetLink,
                    Expiration = DateTime.UtcNow.Add(ApplicationSettings.PasswordResetTokenExpireTimeSpan)
                };
                session.SaveOrUpdate(dbToken);
                session.Flush();
                var myuri = new Uri(Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(HttpContextAccessor.HttpContext.Request));

                var body = "Hi " + user.UserName;

                body += "\nA request has been made to reset your password on " + ApplicationSettings.GetApplicationName();

                body += "\n\nClick on the following link to reset your password:\n";

                var json = new JsonHelper();
                json.Add("userId", user.Id);
                json.Add("token", passwordResetLink);
                var parameters = Encryption.Encrypt(json.ToString(), ApplicationSettings.ApplicationPassPhrase);
                body += GetCurrentUrl() + "?anonAction=" + ((int)EventNumber.ResetPassword) + "&params=" + HttpUtility.UrlEncode(parameters);

                var mailMessage = new MailMessage(settings.EmailFromAddress, user.Email, "Password Reset", body);
                //mailMessage.From = new MailAddress("admin@q10hub.com", "Admin");

                var sendEmailTask = Task.Run(() =>
                {
                    try
                    {
                        var smtpClient = new SmtpClient(settings.EmailHost, settings.EmailPort);

                        var password = Encryption.Decrypt(settings.EmailPassword, ApplicationSettings.ApplicationPassPhrase);
                        smtpClient.Credentials = new System.Net.NetworkCredential(settings.EmailUserName, password);
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.EnableSsl = settings.EmailEnableSsl;

                        smtpClient.Send(mailMessage);

                        emailStatus = "If a user with username or email address exists then a password reset link will be sent to the user's registered email address.";
                    }
                    catch (Exception e)
                    {
                        var message = e.Message + "\n" + e.ToString();
                        emailStatus = message;
                    }
                });
                sendEmailTask.Wait();
            }
            else
            {
                return "No user found";
            }

            await Task.Delay(1000); // To prevent it being too obvious that a username/email address exists or does not.
            return emailStatus;
        }

        public virtual async Task<string> SendAcccountConfirmationEmail(string userId, string userName, string emailAddress)
        {
            Models.SystemSettings settings;
            User dbUser;
            using (var session = DataService.OpenSession())
            {
                dbUser = session.Get<User>(userId);
                settings = session.QueryOver<Models.SystemSettings>().SingleOrDefault<Models.SystemSettings>();
            }

            if (settings == null)
            {
                throw new Exception("No system settings have been setup.");
            }

            Logger.LogInformation("Sending account confirmation email to " + emailAddress);

            var emailToken = await UserManager.GenerateEmailConfirmationTokenAsync(dbUser);

            var myuri = new Uri(Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(HttpContextAccessor.HttpContext.Request));

            var body = "Hi " + userName;

            body += "\nWelcome to " + ApplicationSettings.GetApplicationName();

            body += "\n\nPlease click on the following link to activate your account and confirm your email address:\n";

            body += GetCurrentUrl() + "/api/v1/menu/ConfirmEmail?userId=" + userId + "&token=" + HttpUtility.UrlEncode(emailToken);

            var mailMessage = new MailMessage(settings.EmailFromAddress, emailAddress, "Email Confirmation", body);

            var sendEmailTask = Task.Run(() =>
            {
                try
                {
                    var smtpClient = new SmtpClient(settings.EmailHost, settings.EmailPort);

                    var password = Encryption.Decrypt(settings.EmailPassword, ApplicationSettings.ApplicationPassPhrase);
                    smtpClient.Credentials = new System.Net.NetworkCredential(settings.EmailUserName, password);
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = settings.EmailEnableSsl;

                    Logger.LogInformation("Sending email...");
                    smtpClient.Send(mailMessage);
                    Logger.LogInformation("Email sent...");
                }
                catch (Exception e)
                {
                    var message = e.Message + "\n" + e.ToString();
                    Console.WriteLine(message);
                    Logger.LogError("Error sending email:\n" + message, e);
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
            //var request = HttpContext.Current.Request.RequestContext.HttpContext.Request;
            var request = HttpContextAccessor.HttpContext.Request;

            var uri = request.Path;

            var result = request.Scheme + "://" + request.Host + request.PathBase;//.ApplicationPath;
            return result;
        }

        public User RetrieveUser(string userId)
        {
            using (var session = DataService.OpenSession())
            {
                return session.Get<User>(userId);
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

                //dbUser.EmailConfirmed = dbUser.Email == email;
                dbUser.Email = email;
                DataService.SaveOrUpdate(session, dbUser);

                var existingUserRoles = session.CreateCriteria<UserRoleAssociation>()
                                               .CreateAlias("User", "user")
                                               .Add(Restrictions.Eq("user.Id", dbUser.Id))
                                               .List<UserRoleAssociation>()
                                               .ToList();

                var rolesToDelete = existingUserRoles.Where(e => !userRoles.Contains(e.UserRole.Id)).ToList();
                var existingUserRoleIds = existingUserRoles.Select(e => e.UserRole.Id).ToList();
                var userRolesToAdd = userRoles.Where(e => !existingUserRoleIds.Contains(e)).ToList();

                rolesToDelete.ForEach(u =>
                {
                    DataService.TryDelete(session, u);
                });

                foreach (var role in userRolesToAdd)
                {
                    var dbUserRole = session.Get<UserRole>(role.ToString());

                    var roleAssociation = new UserRoleAssociation()
                    {
                        User = dbUser,
                        UserRole = dbUserRole
                    };
                    DataService.SaveOrUpdate(session, roleAssociation);
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