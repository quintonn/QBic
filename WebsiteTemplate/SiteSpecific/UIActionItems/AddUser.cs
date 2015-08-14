using BasicAuthentication.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.UIActionItems;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.EventItems
{
    public class AddUser : GetInput
    {
        public override EventNumber Id
        {
            get
            {
                return EventNumber.AddUser;
            }
        }

        public override string Name
        {
            get
            {
                return "Add User";
            }
        }

        public override string Description
        {
            get
            {
                return "Add a new user";
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

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("UserName", "User Name"));
                list.Add(new StringInput("Email", "Email"));
                list.Add(new PasswordInput("Password", "Password"));
                list.Add(new PasswordInput("ConfirmPassword", "Confirm Password"));

                return list;
            }
        }

        //public override IList<Menus.BaseItems.Event> InputButtons
        public override IList<InputButton> InputButtons
        {
            get
            {
                return new List<InputButton>()
                {
                    //new CreateUser(),
                    //new CancelInputDialog()
                    new InputButton("Submit", 0),
                    new InputButton("Cancel", 1)
                };
            }
        }

        public override System.Threading.Tasks.Task Initialize(string data)
        {
            //throw new NotImplementedException();
            return Task.FromResult(0);
        }

        public override async System.Threading.Tasks.Task<IList<Event>> ProcessAction(string data, int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUsers)
                };
            }
            else if (actionNumber == 0)
            {
                if (String.IsNullOrWhiteSpace(data))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("There was an error creating a new user. No input was received.")
                    };
                };

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
                    return new List<Event>()
                    {
                        new ShowMessage("Password and password confirmation do not match")
                    };
                }

                var message = "";
                var success = false;

                var result = await CoreAuthenticationEngine.UserManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    message = "Unable to create user:\n" + String.Join("\n", result.Errors);
                    return new List<Event>()
                    {
                         new ShowMessage(message)
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

                    return new List<Event>()
                    {
                        new ShowMessage("User created but there was an error sending activation email:\n" + message),
                        new CancelInputDialog(),
                        new ExecuteAction(EventNumber.ViewUsers)
                    };
                }

                return new List<Event>()
                {
                    new ShowMessage("User created successfully.\nCheck your inbox for activation email."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUsers)
                };
            }

            return null;
        }
    }
}