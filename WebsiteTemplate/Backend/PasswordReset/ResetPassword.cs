using BasicAuthentication.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.PasswordReset
{
    public class ResetPassword : GetInput
    {
        private UserService UserService { get; set; }
        private ApplicationSettingsCore AppSettings { get; set; }
        private string UserId { get; set; }
        private string PasswordToken { get; set; }

        public ResetPassword(UserService userService, ApplicationSettingsCore appSettings)
        {
            UserService = userService;
            AppSettings = appSettings;
        }

        public override string Description
        {
            get
            {
                return "Reset Password";
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var result = new List<InputField>();

                result.Add(new PasswordInput("NewPassword", "New Password", mandatory: true));
                result.Add(new PasswordInput("ConfirmPassword", "Confirm Password", mandatory: true));

                result.Add(new HiddenInput("UserId", UserId));
                result.Add(new HiddenInput("PassToken", PasswordToken));

                return result;
            }
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
                var jsonData = Encryption.Decrypt(data, AppSettings.ApplicationPassPhrase);
                var json = JsonHelper.Parse(jsonData);

                UserId = json.GetValue("userId");
                PasswordToken = json.GetValue("token");
            }

            return new InitializeResult(true);
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
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

                XXXUtils.SetCurrentUser("System");

                var verifyTokenResult = await CoreAuthenticationEngine.UserManager.VerifyUserTokenAsync(userId, "ResetPassword", passwordToken);
                if (verifyTokenResult == false)
                {
                    //TODO: This does not actually do what i expected. The tokens never expire. wtf?!  -- ok, got them to expire after 6 hours, still. wtf
                    //      Need to implement this ourselves. Keep a table of used tokens.
                    return new List<IEvent>()
                    {
                        new ShowMessage("Unable to reset password. The password reset link is no longer valid")
                    };
                }

                var idResult = await CoreAuthenticationEngine.UserManager.ResetPasswordAsync(userId, passwordToken, newPassword);
                if (idResult.Succeeded == false)
                {
                    var errorMessage = "Unable to create user:\n" + String.Join("\n", idResult.Errors);
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