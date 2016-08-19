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

namespace WebsiteTemplate.Backend.Users
{
    public class EditUser : GetInput
    {
        private UserService UserService { get; set; }

        public EditUser(UserService service)
        {
            UserService = service;
        }

        public override System.Threading.Tasks.Task<InitializeResult> Initialize(string data)
        {
            var json = JsonHelper.Parse(data);
            var id = json.GetValue("Id");
            var results = new List<Event>();
            User = UserService.RetrieveUser(id);
            return Task.FromResult<InitializeResult>(new InitializeResult(true));
        }

        public override async System.Threading.Tasks.Task<IList<Event>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                var id = GetValue<string>("Id");
                var userName = GetValue<string>("UserName");
                var email = GetValue<string>("Email");
                var userRoles = GetValue<List<string>>("UserRoles");

                if (String.IsNullOrWhiteSpace(email))
                {
                    return new List<Event>()
                        {
                            new ShowMessage("Unable to modify user. Email is mandatory.")
                        };
                }
                if (String.IsNullOrWhiteSpace(userName))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("User name is mandatory.")
                    };
                }

                var existingUser = UserService.FindUserByUserName(userName);
                if (existingUser != null && existingUser.Id != id)
                {
                    return new List<Event>()
                        {
                            new ShowMessage("Unable to modify user. User with name {0} already exists.", userName)
                        };
                }

                UserService.UpdateUser(id, userName, email, userRoles);

                return new List<Event>()
                {
                    new ShowMessage("User modified successfully."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUsers)
                };
            }

            return new List<Event>()
            {
                new CancelInputDialog()
            };
        }

        private User User { get; set; }

        public override EventNumber GetId()
        {
            return EventNumber.EditUser;
        }

        public override string Description
        {
            get
            {
                return "Edit User";
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var results = new List<InputField>();

                results.Add(new StringInput("UserName", "User Name", User?.UserName));
                results.Add(new StringInput("Email", "Email", User?.Email));
                results.Add(new HiddenInput("Id", User?.Id));

                var items = UserService.GetUserRoles()
                                       .OrderBy(u => u.Description)
                                       .ToDictionary(u => u.Id, u => (object)u.Description);
                var existingItems = UserService.RetrieveUserRoleAssocationsForUserId(User.Id)
                                               .OrderBy(u => u.UserRole.Description)
                                               .Select(u => u.UserRole.Id)
                                               .ToList();

                var listSelection = new ListSelectionInput("UserRoles", "User Roles", existingItems)
                {
                    AvailableItemsLabel = "List of User Roles:",
                    SelectedItemsLabel = "Chosen User Roles:",
                    ListSource = items
                };

                results.Add(listSelection);

                return results;
            }
        }

        public override IList<InputButton> InputButtons
        {
            get
            {
                return new List<InputButton>()
                {
                    new InputButton("Cancel", 0, false),
                    new InputButton("Submit", 1),
                };
            }
        }
    }
}