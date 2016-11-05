using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Menus
{
    public abstract class ModifyMenu : ModifyItemUsingInputProcessor<MenuProcessor, Menu>
    {
        private MenuService MenuService { get; set; }
        public ModifyMenu(MenuProcessor menuProcessor, bool isNew, MenuService menuService)
            :base(menuProcessor, isNew)
        {
            MenuService = menuService;
        }

        public override string ItemNameForDisplay
        {
            get
            {
                return "Menu";
            }
        }

        public override string Title
        {
            get
            {
                return IsNew ? "New Menu" : DataItem.Name;
            }
        }

        public override IList<InputField> GetInputFields()
        {
            var list = new List<InputField>();

            list.Add(new StringInput("Name", "Menu Name", DataItem.Name, null, true));
            list.Add(new BooleanInput("HasSubmenus", "Has Sub-menus", DataItem.Event == null && IsNew == false));

            list.Add(new ComboBoxInput("Event", "Menu Action", DataItem.Event?.ToString(), null, true)
            {
                ListItems = MenuService.GetEventList(),
                VisibilityConditions = new List<Condition>()
                        {
                            new Condition("HasSubmenus", Comparison.Equals, "false")
                        }
            });

            list.Add(new HiddenInput("ParentMenuId", ParentMenuId));
            list.Add(new HiddenInput("Id", DataItem?.Id));


            return list;
        }

        private string ParentMenuId { get; set; }

        public override async Task<InitializeResult> Initialize(string data)
        {
            var jobject = JsonHelper.Parse(data);
            if (IsNew)
            {
                var parentId = jobject.GetValue("ParentId");
                ParentMenuId = parentId;
                DataItem = new Menu();
            }
            else
            {
                var id = jobject.GetValue("Id");
                DataItem = ItemProcessor.RetrieveItem(id);
                ParentMenuId = DataItem.ParentMenu?.Id;
            }
            
            return new InitializeResult(true);
        }

        public override async Task<ProcessingResult> ValidateInputs()
        {
            var name = GetValue("Name");
            var hasSubMenus = GetValue<bool>("HasSubmenus");
            int? eventValue = null;
            var parentMenuId = GetValue<string>("ParentMenuId");
            var menuId = GetValue("Id");

            if (String.IsNullOrWhiteSpace(name))
            {
                return new ProcessingResult(false, "Menu name is mandatory and must be provided.");
            }

            if (hasSubMenus == false)
            {
                eventValue = GetValue<int?>("Event");
                if (eventValue == null)
                {
                    return new ProcessingResult(false, "Menu action is mandatory when 'Has Sub Menus' is unchecked.");
                }
            }

            return null;
        }

        public override EventNumber ViewToShowAfterModify
        {
            get
            {
                return EventNumber.ViewMenus;
            }
        }

        public override string ParametersToPassToViewAfterModify
        {
            get
            {
                return GetValue<string>("ParentMenuId");
            }
        }
    }
}