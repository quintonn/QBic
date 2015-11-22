using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.SiteSpecific.EventItems
{
    public class AddMenu : GetInput
    {
        public override EventNumber GetId()
        {
            return EventNumber.AddMenu;
        }

        public override string Description
        {
            get
            {
                return "Add Menu";
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>()
                {
                    UserRole.AnyOne
                };
            }
        }

        public override IList<InputButton> InputButtons
        {
            get
            {
                return new List<InputButton>()
                {
                    new InputButton("Submit", 0),
                    new InputButton("Cancel", 1)
                };
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("Name", "Menu Name"));
                list.Add(new BooleanInput("HasSubmenus", "Has Sub-menus", false));

                var events = Enum.GetNames(typeof(EventNumber))
                                    .Where(u => !u.Equals("Nothing", StringComparison.InvariantCultureIgnoreCase))
                                    .ToList();

                list.Add(new ComboBoxInput("Event", "Menu Action")
                    {
                        ListItems = events,
                        VisibilityConditions = new List<Menus.ViewItems.Condition>()
                        {
                            new Condition("HasSubmenus", Comparison.Equals, "false")
                        }
                    });

                //var userRoles = Enum.GetNames(typeof(UserRole))
                //                   .Where(u => !u.Equals("AnyOne", StringComparison.InvariantCultureIgnoreCase))
                //                   .Select(u => u)
                var userRoles = Enum.GetValues(typeof(UserRole)).Cast<int>().ToDictionary(e => e.ToString(), e => (object)Enum.GetName(typeof(UserRole), e));
                
                var listSelection = new ListSelectionInput("UserRoles", "Allowed User Roles")
                {
                    AvailableItemsLabel = "User Roles:",
                    SelectedItemsLabel = "Chosen User Roles:",
                    ListSource =  userRoles
                };
                //list.Add(new StringInput("UserRoles", "Allowed User Roles"));
                list.Add(listSelection);

                list.Add(new HiddenInput("ParentMenuId", ParentMenuId));


                return list;
            }
        }

        private string ParentMenuId { get; set; }

        public override async Task<InitializeResult> Initialize(string data)
        {
            ParentMenuId = data;
            return new InitializeResult(true);
        }

        public override async Task<IList<Event>> ProcessAction(string data, int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewMenus, "")
                };
            }
            else if (actionNumber == 0)
            {
                if (String.IsNullOrWhiteSpace(data))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("There was an error creating the menu item. No input was received.")
                    };
                };

                //var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                var json = JObject.Parse(data);

                var name = json.GetValue("Name").ToString();
                var hasSubMenus = Convert.ToBoolean(json.GetValue("HasSubmenus"));
                var eventName = json.GetValue("Event").ToString();
                ParentMenuId = json.GetValue("ParentMenuId").ToString();

                var userRoles = (json.GetValue("UserRoles") as JArray).Select(u => (UserRole)Convert.ToInt32(u)).ToList();

                if (String.IsNullOrWhiteSpace(name))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("Menu name is mandatory and must be provided.")
                    };
                }

                EventNumber? eventNumber = null;
                if (hasSubMenus == false)
                {
                    if (String.IsNullOrWhiteSpace(eventName))
                    {
                        return new List<Event>()
                        {
                            new ShowMessage("Event is mandatory when 'Has Sub Menus' is unchecked.")
                        };
                    }

                    eventNumber = (EventNumber)Enum.Parse(typeof(EventNumber), eventName);
                }

                Menu parentMenu = null;
                using (var session = Store.OpenSession())
                {
                    if (!String.IsNullOrWhiteSpace(ParentMenuId))
                    {
                        parentMenu = session.Get<Menu>(ParentMenuId);
                    }

                    var menu = new Menu()
                    {
                        Event = eventNumber,
                        Name = name,
                        ParentMenu = parentMenu,
                        UserRoleString = "AnyOne",
                        AllowedUserRoles = userRoles
                    };

                    session.Save(menu);
                    session.Flush();
                }

                return new List<Event>()
                {
                    new ShowMessage("Menu created successfully."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewMenus, ParentMenuId)
                };
            }

            return null;
        }
    }
}