using BasicAuthentication.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;
using System.Linq;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using WebsiteTemplate.Menus.PropertyChangedEvents;
using WebsiteTemplate.CustomMenuItems;

namespace WebsiteTemplate.Backend.Users
{
    public class AddUser : GetInput
    {
        public override EventNumber GetId()
        {
            return EventNumber.AddUser;
        }

        public override string Description
        {
            get
            {
                return "Add User";
            }
        }

        public async override Task<IList<Event>> OnPropertyChanged(string propertyName, object propertyValue)
        {
            //if (propertyName == "UserName" && propertyValue.ToString() == "test")
            //{
            //    return new List<Event>()
            //    {
            //        new UpdateInput("Email", "test@gmail.com")
            //    };
            //}
            return await base.OnPropertyChanged(propertyName, propertyValue);
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("UserName", "User Name", mandatory: true)
                {
                    //RaisePropertyChangedEvent = true
                });
                list.Add(new StringInput("Email", "Email"));
                list.Add(new PasswordInput("Password", "Password"));
                list.Add(new PasswordInput("ConfirmPassword", "Confirm Password"));

                //list.Add(new DateInput("Date", "Date", DateTime.Today));

                //Editing a fileInput is breaking at the moment
                //list.Add(new FileInput("File", "File")
                //{
                //    RaisePropertyChangedEvent = true
                //});

                //list.Add(new FileInput("File", "File"));

                using (var session = Store.OpenSession())
                {
                    var items = session.CreateCriteria<UserRole>()
                                       .List<UserRole>()
                                       .OrderBy(u => u.Description)
                                       .ToDictionary(u => u.Id, u => (object)u.Description);

                    var listSelection = new ListSelectionInput("UserRoles", "User Roles")
                    {
                        AvailableItemsLabel = "List of User Roles:",
                        SelectedItemsLabel = "Chosen User Roles:",
                        ListSource = items
                    };

                    list.Add(listSelection);
                }

                return list;
            }
        }

        public override IList<InputButton> InputButtons
        {
            get
            {
                return new List<InputButton>()
                {
                    new InputButton("Submit", 0),
                    new InputButton("Cancel", 1, false)
                };
            }
        }

        public override System.Threading.Tasks.Task<InitializeResult> Initialize(string data)
        {
            return Task.FromResult<InitializeResult>(new InitializeResult(true));
        }

        public override async System.Threading.Tasks.Task<IList<Event>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                    //new ExecuteAction(EventNumber.ViewUsers, String.Empty)
                };
            }
            else if (actionNumber == 0)
            {
                var user = new User(true)
                {
                    Email = GetValue<string>("Email"),
                    UserName = GetValue<string>("UserName")
                };
                var password = GetValue<string>("Password");
                var confirmPassword = GetValue<string>("ConfirmPassword");

                //var date = GetValue<string>("Date");
                //var date = GetValue<DateTime>("Date");

                //var testFile = GetValue<FileInfo>("File");

                if (password != confirmPassword)
                {
                    return new List<Event>()
                    {
                        new ShowMessage("Password and password confirmation do not match")
                    };
                }

                var userRoles = GetValue<List<string>>("UserRoles");

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

                using (var session = Store.OpenSession())
                {
                    foreach (var role in userRoles)
                    {
                        var dbUserRole = session.Get<UserRole>(role.ToString());

                        var roleAssociation = new UserRoleAssociation()
                        {
                            User = user,
                            UserRole = dbUserRole
                        };
                        session.Save(roleAssociation);
                    }
                    session.Flush();
                }

                try
                {
                    var sendEmail = new SendConfirmationEmail();
                    sendEmail.Store = Store;
                    sendEmail.Request = Request;

                    var formData = new Dictionary<string, object>()
                    {
                        {  "Id", user.Id }
                    };
                    sendEmail.InputData = formData;
                    var emailResult = await sendEmail.ProcessAction();
                    success = true;
                }
                catch (FormatException e)
                {
                    message = e.Message;

                    success = false;
                }

                if (!success)
                {
                    return new List<Event>()
                    {
                        new ShowMessage("User created but there was an error sending activation email:\n" + message),
                        new CancelInputDialog(),
                        new ExecuteAction(EventNumber.ViewUsers, String.Empty)
                    };
                }

                return new List<Event>()
                {
                    new ShowMessage("User created successfully.\nCheck your inbox for activation email."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUsers, String.Empty)
                };
            }

            return null;
        }
    }
}