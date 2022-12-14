using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.PasswordReset
{
    public class ResetPassword : GetInput
    {
        private UserService UserService { get; set; }
        private ApplicationSettingsCore AppSettings { get; set; }
        private UserManager<User> UserManager { get; set; }
        private string UserId { get; set; }
        private string PasswordToken { get; set; }
        private string TempJson { get; set; }

        private static readonly ILogger Logger = SystemLogger.GetLogger<ResetPassword>();

        public ResetPassword(UserService userService, ApplicationSettingsCore appSettings, UserManager<User> userManager)
        {
            UserService = userService;
            AppSettings = appSettings;
            UserManager = userManager;
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }
        public override string Description
        {
            get
            {
                return "Reset Password";
            }
        }

        public override IList<InputField> GetInputFields()
        {
            var result = new List<InputField>();

            result.Add(new PasswordInput("NewPassword", "New Password", mandatory: true));
            result.Add(new PasswordInput("ConfirmPassword", "Confirm Password", mandatory: true));

            result.Add(new HiddenInput("UserId", UserId));
            result.Add(new HiddenInput("PassToken", PasswordToken));
            result.Add(new HiddenInput("TempJson", TempJson));
            return result;
        }

        public override EventNumber GetId()
        {
            return EventNumber.ResetPassword;
        }

        public override bool RequiresAuthorization
        {
            get
            {
                return false;
            }
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            if (!data.Contains("UserId"))
            {
                Logger.LogInformation("ResetPassword:");
                Logger.LogInformation("*****************************************");
                Logger.LogInformation(data);
                var jsonData = Encryption.Decrypt(data, AppSettings.ApplicationPassPhrase);
                Logger.LogInformation($"JSONData =\r\n-------------------\r\n{jsonData}\r\n--------------------------\r\n");

                TempJson = jsonData;
                var json = JsonHelper.Parse(jsonData);
                Logger.LogInformation("json parsed:\r\n" + json.ToString());

                UserId = json.GetValue("userId");
                PasswordToken = json.GetValue("token");
            }

            return new InitializeResult(true);
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            Logger.LogInformation("Processing reset password action");
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                    new UserConfirmation("Password has not been reset", "Ok", String.Empty)
                        {
                            OnConfirmationUIAction = EventNumber.Logout
                        },
                };
            }
            else if (actionNumber == 0)
            {
                var userId = GetValue("UserId");
                var passwordToken = GetValue("PassToken");
                var newPassword = GetValue("NewPassword");
                var confirmPassword = GetValue("ConfirmPassword");

                if (newPassword != confirmPassword)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("Passwords don't match. Please try again."),
                    };
                }

                //WebsiteUtils.SetCurrentUser("System");

                /*
                var verifyTokenResult = await UserManager.VerifyUserTokenAsync(userId, "ResetPassword", passwordToken);
                if (verifyTokenResult == false)
                {
                    //TODO: This does not actually do what i expected. The tokens never expire. wtf?!  -- ok, got them to expire after 6 hours, still. wtf
                    //      Need to implement this ourselves. Keep a table of used tokens.
                    return new List<IEvent>()
                    {
                        new ShowMessage("Unable to reset password. The password reset link is no longer valid")
                    };
                }*/

                Logger.LogInformation("Calling reset password async");
                var user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage($"Unable to retrieve user '{userId}'")
                    };
                }
                Logger.LogInformation($"User for userId({userId}) = {user?.UserName}");
                var idResult = await UserManager.ResetPasswordAsync(user, passwordToken, newPassword);
                Logger.LogInformation("Calling reset password async: " + idResult.Succeeded);
                if (idResult.Succeeded == false)
                {
                    var errorMessage = "Unable to reset password:\n" + String.Join("\n", idResult.Errors.Select(x => x.Description).ToList());
                    return new List<IEvent>()
                    {
                        new ShowMessage(errorMessage)
                    };
                }
                else
                {
                    return new List<IEvent>()
                    {
                        new CancelInputDialog(),
                        new UserConfirmation("Password changed successfully. You can now log in with your new password", "Ok", String.Empty)
                        {
                            OnConfirmationUIAction = EventNumber.Logout
                        },
                        //new LogoutEvent(true)
                    };
                }
            }

            return new List<IEvent>();
        }
    }
}