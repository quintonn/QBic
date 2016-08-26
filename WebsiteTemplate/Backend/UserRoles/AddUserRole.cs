using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class AddUserRole : GetInput
    {
        public override string Description
        {
            get
            {
                return "Add User Role";
            }
        }

        private UserRoleService UserRoleService { get; set; }

        public AddUserRole(UserRoleService service)
        {
            UserRoleService = service;
        }

        public override IList<InputButton> InputButtons
        {
            get
            {
                return new List<InputButton>()
                {
                    new InputButton("Submit", 0),
                    new InputButton("Cancel", 1, false),
                };
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("Name", "Name", mandatory: true));
                list.Add(new StringInput("Description", "Description", mandatory: true));

                //list.Add(new FileInput("File", "File"));

                //list.Add(new DateInput("Date", "Date", DateTime.Now)
                //{
                //    Mandatory = false
                //});

                //list.Add(new StringInput("xxx", "xxx", "", "", true));
                //list.Add(new StringInput("xxx2", "xxx2", "", "x", false)
                //{
                //    MandatoryConditions = new List<WebsiteTemplate.Menus.ViewItems.Condition>()
                //    {
                //        new WebsiteTemplate.Menus.ViewItems.Condition("Name", WebsiteTemplate.Menus.ViewItems.Comparison.Equals, "q")
                //    },
                //    VisibilityConditions = new List<WebsiteTemplate.Menus.ViewItems.Condition>()
                //    {
                //        new WebsiteTemplate.Menus.ViewItems.Condition("Name", WebsiteTemplate.Menus.ViewItems.Comparison.Equals, "q")
                //    },
                //});

                //list.Add(new FileInput("File", "File", null, "x"));

                list.Add(new ViewInput("view", "View", new TestView(), "abc", null, false));

                var listSelection = new ListSelectionInput("Events", "Allowed Events")
                {
                    AvailableItemsLabel = "List of Events:",
                    SelectedItemsLabel = "Chosen Events:",
                    ListSource = UserRoleService.GetListOfEvents()
                };

                list.Add(listSelection);
                //list.Add(new StringInput("abc", "abc"));

                return list;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.AddUserRole;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            return new InitializeResult(true);
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUserRoles, "")
                };
            }
            else if (actionNumber == 0)
            {
                var name = GetValue("Name");
                var description = GetValue("Description");
                var events = GetValue<List<string>>("Events");

                //var date = GetValue<DateTime?>("Date");
                //var view = GetValue<List<JToken>>("view");
                //var localTime = ((DateTime)date).ToLocalTime();

                //var file = GetValue<WebsiteTemplate.Menus.InputItems.FileInfo>("File");

                if (String.IsNullOrWhiteSpace(name))
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("Name is mandatory and must be provided. ")
                    };
                }
                if (String.IsNullOrWhiteSpace(description))
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("Description is mandatory and must be provided.")
                    };
                }
                if (events == null)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("Events is not present and is mandatory.")
                    };
                }

                var dbUserRole = UserRoleService.FindUserRoleByName(name);

                if (dbUserRole != null)
                {
                    return new List<IEvent>()
                        {
                            new ShowMessage("User role {0} already exists.", name)
                        };
                }

                UserRoleService.AddUserRole(name, description, events);

                return new List<IEvent>()
                {
                    new ShowMessage("User role created successfully."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUserRoles)
                };
            }
            return null;

        }
    }
}