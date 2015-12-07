using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Backend.Menus
{
    public class ModifyMenu : GetInput
    {
        private Menu Menu { get; set; } = new Menu();
        private bool IsNew { get; set; } = true;
        public override int GetId()
        {
            return EventNumber.ModifyMenu;
        }

        public override string Description
        {
            get
            {
                return IsNew ? "Add Menu" : "Edit Menu";
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

                list.Add(new StringInput("Name", "Menu Name", Menu.Name));
                list.Add(new BooleanInput("HasSubmenus", "Has Sub-menus", Menu.Event == null));

                var events = MainController.EventList.Select(e => e.Value.Description)
                                                     .Where(m => !String.IsNullOrWhiteSpace(m))
                                                     .OrderBy(m => m)
                                                     .ToList();

                list.Add(new ComboBoxInput("Event", "Menu Action", Menu.Event?.ToString())
                    {
                        ListItems = events,
                        VisibilityConditions = new List<Condition>()
                        {
                            new Condition("HasSubmenus", Comparison.Equals, "false")
                        }
                    });

                //var userRoles = Enum.GetValues(typeof(UserRole)).Cast<int>().ToDictionary(e => e.ToString(), e => (object)Enum.GetName(typeof(UserRole), e));
                
                //var listSelection = new ListSelectionInput("UserRoles", "Allowed User Roles")
                //{
                //    AvailableItemsLabel = "User Roles:",
                //    SelectedItemsLabel = "Chosen User Roles:",
                //    ListSource =  userRoles
                //};
                
                //list.Add(listSelection);

                list.Add(new HiddenInput("ParentMenuId", ParentMenuId));
                list.Add(new HiddenInput("IsNew", IsNew));
                list.Add(new HiddenInput("Id", Menu?.Id));


                return list;
            }
        }

        private string ParentMenuId { get; set; }

        public override async Task<InitializeResult> Initialize(string data)
        {
            var jobject = JObject.Parse(data);
            JToken tmp;
            if (jobject.TryGetValue("IsNew", out tmp))
            {
                IsNew = Convert.ToBoolean(tmp);
                var parentId = jobject.GetValue("ParentId").ToString();
                ParentMenuId = parentId;
            }
            else
            {
                IsNew = false;
                var id = jobject.GetValue("Id").ToString();
                using (var session = Store.OpenSession())
                {
                    Menu = session.Get<Menu>(id);
                }
            }
            
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

                var json = JObject.Parse(data);

                var isNew = Convert.ToBoolean(json.GetValue("IsNew").ToString());
                var name = json.GetValue("Name").ToString();
                var hasSubMenus = Convert.ToBoolean(json.GetValue("HasSubmenus"));
                var eventName = json.GetValue("Event").ToString();
                var menuId = json.GetValue("Id").ToString();
                ParentMenuId = json.GetValue("ParentMenuId").ToString();

                //var userRoles = (json.GetValue("UserRoles") as JArray).Select(u => (UserRole)Convert.ToInt32(u)).ToList();

                if (String.IsNullOrWhiteSpace(name))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("Menu name is mandatory and must be provided.")
                    };
                }

                int? eventNumber = null;
                if (hasSubMenus == false)
                {
                    if (String.IsNullOrWhiteSpace(eventName))
                    {
                        return new List<Event>()
                        {
                            new ShowMessage("Event is mandatory when 'Has Sub Menus' is unchecked.")
                        };
                    }

                    eventNumber = MainController.EventList.Where(e => e.Value.Description == eventName).Select(e => Convert.ToInt32(e.Value.GetEventId())).First();
                }

                Menu parentMenu = null;
                using (var session = Store.OpenSession())
                {
                    Menu menu;
                    if (!isNew)
                    {
                        menu = session.Get<Menu>(menuId);
                        ParentMenuId = menu.ParentMenu?.Id;
                    }
                    else
                    {
                        menu = new Menu();
                    }

                    if (!String.IsNullOrWhiteSpace(ParentMenuId))
                    {
                        parentMenu = session.Get<Menu>(ParentMenuId);
                    }

                    menu.Event = eventNumber;
                    menu.Name = name;
                    menu.ParentMenu = parentMenu;

                    session.Save(menu);
                    session.Flush();
                }

                return new List<Event>()
                {
                    new ShowMessage("Menu {0} successfully.", isNew ? "created" : "modified"),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewMenus, ParentMenuId)
                };
            }
            return null;
        }
    }
}