using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Backend.Users
{
    public class AddUser : GetInput
    {
        private UserService UserService { get; set; }

        public AddUser(UserService service)
        {
            UserService = service;
        }

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

        public async override Task<IList<IEvent>> OnPropertyChanged(string propertyName, object propertyValue)
        {
            //if (propertyName == "UserName" && propertyValue.ToString() == "test")
            //{
            //    return new List<IEvent>()
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

                list.Add(new StringInput("UserName", "User Name", mandatory: true));
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

                var items = UserService.GetUserRoles()
                                       .OrderBy(u => u.Description)
                                       .ToDictionary(u => u.Id, u => (object)u.Description);

                var listSelection = new ListSelectionInput("UserRoles", "User Roles")
                {
                    AvailableItemsLabel = "List of User Roles:",
                    SelectedItemsLabel = "Chosen User Roles:",
                    ListSource = items
                };

                list.Add(listSelection);

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

        public override async System.Threading.Tasks.Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                    //new ExecuteAction(EventNumber.ViewUsers, String.Empty)
                };
            }
            else if (actionNumber == 0)
            {
                var email = GetValue<string>("Email");
                var userName = GetValue<string>("UserName");
                var password = GetValue<string>("Password");
                var confirmPassword = GetValue<string>("ConfirmPassword");

                //var date = GetValue<string>("Date");
                //var date = GetValue<DateTime>("Date");

                //var testFile = GetValue<FileInfo>("File");

                if (password != confirmPassword)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("Password and password confirmation do not match")
                    };
                }

                var userRoles = GetValue<List<string>>("UserRoles");

                var message = await UserService.CreateUser(userName, email, password, userRoles);

                if (!String.IsNullOrWhiteSpace(message))
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage(message),
                        new CancelInputDialog(),
                        new ExecuteAction(EventNumber.ViewUsers, String.Empty)
                    };
                }

                return new List<IEvent>()
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