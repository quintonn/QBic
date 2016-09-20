using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Menus
{
    public class ViewMenus : ShowViewUsingInputProcessor<MenuProcessor, Menu>
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

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public ViewMenus(MenuProcessor menuProcessor)
            :base(menuProcessor)
        {
            
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

            columnConfig.AddButtonColumn("Sub Menus", "Id", "...", EventNumber.ViewMenus, new ShowHideColumnSetting()
            {
                Display = ColumnDisplayType.Show,
                Conditions = new List<Condition>()
                {
                    new Condition("Event", Comparison.IsNull)
                }
            }, MenuId);

            columnConfig.AddHiddenColumn("Position");

            columnConfig.AddButtonColumn("", "Id", "Up", EventNumber.IncrementMenuOrder,
                new ShowHideColumnSetting()
                {
                    Display = ColumnDisplayType.Show,
                    Conditions = new List<Condition>()
                    {
                        new Condition("Position", Comparison.GreaterThan, "0")
                    }
                });

            columnConfig.AddButtonColumn("", "Id", "Down", EventNumber.DecrementMenuOrder);
            //TODO: Would be good to hide the Down button for the last item.

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

        public override void PerformAdditionalProcessingOnDataRetrieval(string data, bool obtainingDataCountOnly)
        {
            if (obtainingDataCountOnly == true)
            {
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
                else if (!String.IsNullOrWhiteSpace(eventParams?.ToString()))
                {
                    MenuId = eventParams.GetValue("MenuId");
                }
                else
                {
                    MenuId = String.Empty; // Comes here when clicking back on view menu, but only on second screen, not third, fourth, etc.
                }

                if (!String.IsNullOrWhiteSpace(MenuId))
                {
                    var parentMenu = ItemProcessor.RetrieveItem(MenuId);
                    ParentId = parentMenu.ParentMenu != null ? parentMenu.ParentMenu.Id : "";
                    mTitle = "Menus: " + parentMenu.Name;
                }
                else
                {
                    mTitle = "Menus";
                }
            }
        }

        public override IDictionary<string, object> RetrieveAdditionalParametersForDataQuery(string data)
        {
            var items = new Dictionary<string, object>()
            {
                { "MenuId", MenuId }
            };
            return items;
        }

        public override IEnumerable MapResultsToCustomData(IList<Menu> data)
        {
            var newList = data.Select(r => new
            {
                Name = r.Name,
                Id = r.Id,
                Event = r.Event == null ? "" : EventService.EventList.ContainsKey(r.Event.Value) ? EventService.EventList[r.Event.Value].Description : "",
                ParentMenu = r.ParentMenu,
                CanDelete = r.CanDelete,
                Position = r.Position
            }).ToList();

            return newList;
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