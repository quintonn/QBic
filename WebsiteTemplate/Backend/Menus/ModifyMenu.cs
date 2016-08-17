using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.CustomMenuItems;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.PropertyChangedEvents;
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

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("Name", "Menu Name", Menu.Name, null, true));
                list.Add(new BooleanInput("HasSubmenus", "Has Sub-menus", Menu.Event == null && IsNew == false));

                var events = MainController.EventList.Where(e => e.Value.ActionType != EventType.InputDataView)
                                                     .Where(m => !String.IsNullOrWhiteSpace(m.Value.Description))
                                                     .Where(m => m.Value is ShowView) //TODO: Might have to add isEdit as property to GetInput type or change all edit/add to modify only
                                                     .OrderBy(m => m.Value.Description)
                                                     .ToDictionary(m => m.Key.ToString(), m => (object)m.Value.Description);

                list.Add(new ComboBoxInput("Event", "Menu Action", Menu.Event?.ToString(), null, true)
                    {
                        ListItems = events,
                        VisibilityConditions = new List<Condition>()
                        {
                            new Condition("HasSubmenus", Comparison.Equals, "false")
                        }
                    });

                //list.Add(new StringInput("Test", "Test")
                //{
                //    VisibilityConditions = new List<Condition>()
                //    {
                //        new Condition("Event", Comparison.Equals, "123")
                //    }
                //});

                User xxx;
                using (var session = Store.OpenSession())
                {
                    xxx = session.QueryOver<User>().Take(1).List<User>().First();
                }

                //list.Add(new DataSourceComboBoxInput<User>("User", "User", x => x.Id, x => x.UserName, xxx.Id, null, null, null, true, true)
                //{
                    //    MandatoryConditions = new List<Condition>()
                    //    {
                    //        new Condition("Name", Comparison.Equals, "test")
                    //    },
                    //    VisibilityConditions = new List<Condition>()
                    //    {
                    //        new Condition("Name", Comparison.Equals, "test")
                    //    }
                //    RaisePropertyChangedEvent = true
                //});

                //list.Add(new DataSourceComboBoxInput<User>("User2", "User2", x => x.Id, x => x.UserName, xxx.Id, null, null, null, true, true)
                //{
                    
                //});

                list.Add(new HiddenInput("ParentMenuId", ParentMenuId));
                list.Add(new HiddenInput("IsNew", IsNew));
                list.Add(new HiddenInput("Id", Menu?.Id));


                return list;
            }
        }

        private string ParentMenuId { get; set; }

        public override async Task<InitializeResult> Initialize(string data)
        {
            var jobject = JsonHelper.Parse(data);
            if (!String.IsNullOrWhiteSpace(jobject.GetValue("IsNew")))
            {
                IsNew = jobject.GetValue<bool>("IsNew");
                var parentId = jobject.GetValue("ParentId");
                ParentMenuId = parentId;
                Menu = new Menu();
            }
            else
            {
                IsNew = false;
                var id = jobject.GetValue("Id");
                using (var session = Store.OpenSession())
                {
                    Menu = session.Get<Menu>(id);
                }
                ParentMenuId = Menu?.ParentMenu?.Id;
            }
            
            return new InitializeResult(true);
        }

        public override async Task<IList<Event>> ProcessAction(int actionNumber)
        {
            ParentMenuId = GetValue<string>("ParentMenuId");

            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewMenus, ParentMenuId)
                };
            }
            else if (actionNumber == 0)
            {
                var isNew = GetValue<bool>("IsNew");
                var name = GetValue("Name");
                var hasSubMenus = GetValue<bool>("HasSubmenus");
                var eventValue = GetValue<string>("Event");
                var menuId = GetValue("Id");
                
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
                    if (String.IsNullOrWhiteSpace(eventValue))
                    {
                        return new List<Event>()
                        {
                            new ShowMessage("Menu action is mandatory when 'Has Sub Menus' is unchecked.")
                        };
                    }

                    eventNumber = Convert.ToInt32(eventValue);
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
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewMenus, ParentMenuId),
                    new ShowMessage("Menu {0} successfully.", isNew ? "created" : "modified"),
                };
            }
            return null;
        }
    }
}