using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Menus
{
    public class ViewMenus : ShowView
    {
        private string mTitle = "View Menus";
        public override string Description
        {
            get
            {
                return "View Menus";
            }
        }

        public override string Title
        {
            get
            {
                return mTitle;
            }
        }

        private MenuService MenuService { get; set; }
        private EventService EventService { get; set; }

        public ViewMenus(MenuService menuService, EventService eventService)
        {
            MenuService = menuService;
            EventService = eventService;
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            var results = new List<MenuItem>();
            var menuId = String.Empty;
            var parentId = String.Empty;
            if (dataForMenu.ContainsKey("MenuId"))
            {
                menuId = dataForMenu["MenuId"];
            }
            if (dataForMenu.ContainsKey("ParentId"))
            {
                parentId = dataForMenu["ParentId"];
            }

            if (!String.IsNullOrWhiteSpace(menuId))
            {
                results.Add(new MenuItem("Back", EventNumber.ViewMenus, parentId));
            }

            var jsonObject = new JsonHelper();
            jsonObject.Add("ParentId", menuId);
            var json = jsonObject.ToString();
            results.Add(new MenuItem("Add", EventNumber.AddMenu, json));

            return results;
        }

        public override Dictionary<string, string> DataForGettingMenu
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "MenuId" , MenuId },
                    { "ParentId", ParentId }
                };
            }
        }

        private string MenuId { get; set; }
        private string ParentId { get; set; }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");
            columnConfig.AddHiddenColumn("Name");

            columnConfig.AddStringColumn("Event", "Event");

            //columnConfig.AddDateColumn("Date", "Date");

            columnConfig.AddButtonColumn("Sub Menus", "Id", "...", EventNumber.ViewMenus, new ShowHideColumnSetting()
            {
                Display = ColumnDisplayType.Show,
                Conditions = new List<Condition>()
                {
                    new Condition("Event", Comparison.IsNull)
                }
            }, MenuId);

            columnConfig.AddLinkColumn("", "Id", "Edit", EventNumber.EditMenu, null);

            columnConfig.AddButtonColumn("", "Id", "X",
                new UserConfirmation("Delete Menu Item?")
                {
                    OnConfirmationUIAction = EventNumber.DeleteMenu
                },
                columnSetting: new ShowHideColumnSetting()
                {
                    Display = ColumnDisplayType.Show,
                    Conditions = new List<Condition>()
                   {
                       new Condition("CanDelete", Comparison.Equals, "true")
                   }
                }
            );
        }

        public override IEnumerable GetData(string data, int currentPage, int linesPerPage, string filter)
        {
            ProcessData(data);

            if (!String.IsNullOrWhiteSpace(MenuId))
            {
                var parentMenu = MenuService.RetrieveMenu(MenuId);
                ParentId = parentMenu.ParentMenu != null ? parentMenu.ParentMenu.Id : "";
                mTitle = "Menus: " + parentMenu.Name;
            }
            else
            {
                mTitle = "Menus";
            }

            var results = MenuService.RetrieveMenusWithFilter(MenuId, currentPage, linesPerPage, filter);

            var newList = results.Select(r => new
            {
                Name = r.Name,
                Id = r.Id,
                Event = r.Event == null ? "" : EventService.EventList.ContainsKey(r.Event.Value) ? EventService.EventList[r.Event.Value].Description : "",
                ParentMenu = r.ParentMenu,
                CanDelete = r.CanDelete,
                //Date = DateTime.Now.Date
            }).ToList();

            return newList;
        }

        public void ProcessData(string data)
        {
            //try
            //{
            var json = JsonHelper.Parse(data);
            var id = json.GetValue("Id");                                  // If 'sub-menus' column link is clicked
            var dataItem = json.GetValue("data");                          // If 'back' menu-button button is clicked
            var eventParams = json.GetValue<JsonHelper>("eventParameters"); // This is when 'search' is clicked on the filter

            if (String.IsNullOrWhiteSpace(json.ToString()))
            {
                MenuId = data;
            }
            else if (!String.IsNullOrWhiteSpace(id))
            {
                MenuId = id;
            }
            else if (!String.IsNullOrWhiteSpace(dataItem))
            {
                MenuId = dataItem;
            }
            else
            {
                //throw new Exception("Unhandled situation. Not sure what to assign MenuId");
                MenuId = String.Empty;
            }
            //}
            //catch (Exception e)
            //{
            // Comes here when the 'Menus' menu item is clicked (i.e. viewing all top level menus)
            //  MenuId = "";
            //Console.WriteLine(e.Message);
            //}
        }

        public override int GetDataCount(string data, string filter)
        {
            ProcessData(data);
            var count = MenuService.RetrieveMenusCountWithFilter(MenuId, filter);
            return count;
        }

        public override EventNumber GetId()
        {
            return EventNumber.ViewMenus;
        }

        public override Dictionary<string, object> GetEventParameters()
        {
            var result = new Dictionary<string, object>();

            if (!String.IsNullOrWhiteSpace(MenuId))
            {
                result.Add("MenuId", MenuId);
            }
            return result;
        }
    }
}