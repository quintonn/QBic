﻿using BasicAuthentication.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.UIActionItems
{
    public class CreateUser : DoSomething
    {
        public override async Task<IList<UIActionResult>> ProcessAction(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
            {
                return new List<UIActionResult>()
                {
                    new UIActionResult()
                    {
                        UIAction = new ShowMessage(),
                        ResultData = "There was an error creating a new user. No input was received."
                    }
            };
        }

            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

            var user = new User(true)
            {
                Email = parameters["Email"],
                UserName = parameters["UserName"],
            };
            var password = parameters["Password"];
            var confirmPassword = parameters["ConfirmPassword"];

            if (password != confirmPassword)
            {
                return new List<UIActionResult>()
                {
                    new UIActionResult()
                    {
                        UIAction = new ShowMessage(),
                        ResultData = "Password and password confirmation do not match"
                    }
                };
            }

            var message = "";
            var success = false;

            var result = await CoreAuthenticationEngine.UserManager.CreateAsync(user, "password");

            if (!result.Succeeded)
            {
                message = "Unable to create user:\n" + String.Join("\n", result.Errors);
                return new List<UIActionResult>()
                {
                     new UIActionResult()
                    {
                        ResultData = message,
                        UIAction = new ShowMessage()
                    }
                };
            }

            try
            {
                var sendEmail = new SendConfirmationEmail();
                sendEmail.Store = Store;
                sendEmail.Request = Request;
                await sendEmail.ProcessAction(user.Id);
                success = true;
            }
            catch (FormatException e)
            {
                message = e.Message;

                success = false;
            }

            if (!success)
            {
                //await CoreAuthenticationEngine.UserManager.DeleteAsync(user);

                return new List<UIActionResult>()
                {
                    new UIActionResult()
                    {
                        ResultData = "User created but there was an error sending activation email:\n" + message,
                        UIAction = new ShowMessage()
                    },
                    new UIActionResult()
                    {
                         This should rather show the view of users.
                         Need to either auto - refresh the last view or execute a specific menu item click
                        UIAction = new CancelInputDialog()
                    }
                };
            }

            return new List<UIActionResult>()
            {
                new UIActionResult()
                {
                    UIAction = new ShowMessage(),
                    ResultData = "User created successfully.\nCheck your inbox for activation email."
                },
                new UIActionResult()
                {
                     This should rather show the view of users.
                         Need to either auto - refresh the last view or execute a specific menu item click
                    UIAction = new CancelInputDialog()
                }
            };
        }

        public override int Id
        {
            get
            {
                return UIActionNumbers.CREATE_USER;
            }
        }

        public override string Name
        {
            get
            {
                return "Create User";
            }
        }

        public override string Description
        {
            get
            {
                return "Creates a new user";
            }
        }

        public override string MenuLabel
        {
            get
            {
                return "Submit";
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>()
                {
                    UserRole.AddUser
                };
            }
        }
    }
}