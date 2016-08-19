using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class EditUserRole : GetInput
    {
        public UserRole UserRole { get; set; }

        private UserRoleService UserRoleService { get; set; }

        public EditUserRole(UserRoleService service)
        {
            UserRoleService = service;
        }
        public override string Description
        {
            get
            {
                return "Edit User Role";
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

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("Name", "Name", UserRole?.Name));
                list.Add(new StringInput("Description", "Description", UserRole?.Description));
                list.Add(new HiddenInput("Id", UserRole?.Id));

                var items = MainController.EventList.ToDictionary(e => e.Key, e => e.Value.Description)
                                                    .OrderBy(e => e.Value)
                                                    .ToDictionary(e => e.Key.ToString(), e => (object)e.Value);

                var existingItems = UserRoleService.RetrieveEventRoleAssociationsForUserRole(UserRole.Id);

                var listSelection = new ListSelectionInput("Events", "Allowed Events", existingItems)
                {
                    AvailableItemsLabel = "List of Events:",
                    SelectedItemsLabel = "Chosen Events:",
                    ListSource = items
                };

                list.Add(listSelection);

                return list;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.EditUserRole;
        }

        public override Task<InitializeResult> Initialize(string data)
        {
            var json = JsonHelper.Parse(data);
            var id = json.GetValue("Id");

            UserRole = UserRoleService.RetrieveUserRole(id);

            return Task.FromResult(new InitializeResult(true));
        }

        public override async Task<IList<Event>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUserRoles)
                };
            }
            else if (actionNumber == 0)
            {
                var id = GetValue<string>("Id");
                var name = GetValue<string>("Name");
                var description = GetValue<string>("Description");

                var events = GetValue<List<string>>("Events");

                if (String.IsNullOrWhiteSpace(name))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("Name is mandatory and must be provided.")
                    };
                }
                if (String.IsNullOrWhiteSpace(description))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("Description is mandatory and must be provided.")
                    };
                }

                var dbUserRole = UserRoleService.FindUserRoleByName(name);
                if (dbUserRole != null && dbUserRole.Id != id)
                {
                    return new List<Event>()
                        {
                            new ShowMessage("User role {0} already exists.", name)
                        };
                }

                UserRoleService.UpdateUserRole(id, name, description, events);

                return new List<Event>()
                {
                    new ShowMessage("User role modified successfully."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUserRoles)
                };
            }

            return null;
        }
    }
}