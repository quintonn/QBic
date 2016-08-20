using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Menus
{
    public abstract class ModifyMenu : GetInput
    {
        public ModifyMenu(MenuService menuService)
        {
            MenuService = menuService;
        }
        private Menu Menu { get; set; } = new Menu();
        internal abstract bool IsNew { get; }
        private MenuService MenuService { get; set; }

        public override string Description
        {
            get
            {
                return IsNew ? "Add Menu" : "Edit Menu";
            }
        }

        public override string Title
        {
            get
            {
                return IsNew ? "New Menu" : Menu.Name;
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("Name", "Menu Name", Menu.Name, null, true) { RaisePropertyChangedEvent = true });
                list.Add(new BooleanInput("HasSubmenus", "Has Sub-menus", Menu.Event == null && IsNew == false));

                list.Add(new ComboBoxInput("Event", "Menu Action", Menu.Event?.ToString(), null, true)
                    {
                        ListItems = MenuService.GetEventList(),
                        VisibilityConditions = new List<Condition>()
                        {
                            new Condition("HasSubmenus", Comparison.Equals, "false")
                        }
                    });

                list.Add(new HiddenInput("ParentMenuId", ParentMenuId));
                list.Add(new HiddenInput("Id", Menu?.Id));


                return list;
            }
        }

        private string ParentMenuId { get; set; }

        public override async Task<InitializeResult> Initialize(string data)
        {
            var jobject = JsonHelper.Parse(data);
            if (IsNew)
            {
                var parentId = jobject.GetValue("ParentId");
                ParentMenuId = parentId;
                Menu = new Menu();
            }
            else
            {
                var id = jobject.GetValue("Id");
                Menu = MenuService.RetrieveMenu(id);
                ParentMenuId = Menu?.ParentMenu?.Id;
            }
            
            return new InitializeResult(true);
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            ParentMenuId = GetValue<string>("ParentMenuId");

            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewMenus, ParentMenuId)
                };
            }
            else if (actionNumber == 0)
            {
                var name = GetValue("Name");
                var hasSubMenus = GetValue<bool>("HasSubmenus");
                int? eventValue = null;
                var parentMenuId = GetValue<string>("ParentMenuId");
                var menuId = GetValue("Id");
                
                if (String.IsNullOrWhiteSpace(name))
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("Menu name is mandatory and must be provided.")
                    };
                }

                if (hasSubMenus == false)
                {
                    eventValue = GetValue<int?>("Event");
                    if (eventValue == null)
                    {
                        return new List<IEvent>()
                        {
                            new ShowMessage("Menu action is mandatory when 'Has Sub Menus' is unchecked.")
                        };
                    }
                }

                MenuService.SaveOrUpdateMenu(menuId, parentMenuId, eventValue, name);
                
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewMenus, ParentMenuId),
                    new ShowMessage("Menu {0} successfully.", IsNew ? "created" : "modified"),
                };
            }
            return null;
        }
    }
}