using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Backend.Menus
{
    public class ViewMenus : ShowView
    {
        private string mDescription = "View Menus";
        public override string Description
        {
            get
            {
                return mDescription;
            }
        }

        public override IList<MenuItem> GetViewMenu()
        {
            var results = new List<MenuItem>();

            if (!String.IsNullOrWhiteSpace(MenuId))
            {
                results.Add(new MenuItem("Back", EventNumber.ViewMenus, ParentId));
            }

            var jsonObject = new JObject();
            jsonObject.Add("IsNew", true);
            jsonObject.Add("ParentId", MenuId);
            var json = jsonObject.ToString();
            results.Add(new MenuItem("Add", EventNumber.ModifyMenu, json));

            return results;
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

            columnConfig.AddLinkColumn("", "Id", "Edit", EventNumber.ModifyMenu, null);

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
            using (var session = Store.OpenSession())
            {
                var query = session.QueryOver<Menu>();

                if (!String.IsNullOrWhiteSpace(MenuId))
                {
                    query = query.Where(m => m.ParentMenu.Id == MenuId);

                    var parentMenu = session.Get<Menu>(MenuId);
                    ParentId = parentMenu.ParentMenu != null ? parentMenu.ParentMenu.Id : "";
                    mDescription = "Menus: " + parentMenu.Name;
                }
                else
                {
                    mDescription = "Menus";
                    query = query.Where(m => m.ParentMenu == null);
                }

                if (!String.IsNullOrWhiteSpace(filter))
                {
                    query = query.WhereRestrictionOn(x => x.Name).IsLike(filter, MatchMode.Anywhere);
                }

                var results = query
                    .Skip((currentPage - 1) * linesPerPage)
                    .Take(linesPerPage)
                    .List<Menu>()
                    .ToList();

                var newList = results.Select(r => new
                {
                    Name = r.Name,
                    Id = r.Id,
                    Event = r.Event == null ? "" : MainController.EventList.ContainsKey(r.Event.Value) ? MainController.EventList[r.Event.Value].Description : "",
                    ParentMenu = r.ParentMenu,
                    CanDelete = r.CanDelete,
                }).ToList();

                return newList;
            }
        }

        //TODO: Possible (most likely) change this to have JSON object as input. So i have this try catch in only one place
        //      Then i can set the incoming json object to either be null, or a blank json object.
        //      --- Lets see what happens in ClaimManager - if it'll be usefull
        public void ProcessData(string data)
        {
            try
            {
                var json = JObject.Parse(data);
                var id = json.GetValue("Id");                                  // If 'sub-menus' column link is clicked
                var dataItem = json.GetValue("data");                          // If 'back' menu-button button is clicked
                var eventParams = json.GetValue("eventParameters") as JObject; // This is when 'search' is clicked on the filter

                if (id != null)
                {
                    MenuId = id.ToString();
                }
                else if (dataItem != null)
                {
                    MenuId = dataItem.ToString();
                }
                else if (eventParams != null)
                {
                    MenuId = eventParams.GetValue("MenuId")?.ToString();
                }
            }
            catch (Exception e)
            {
                // Comes here when the 'Menus' menu item is clicked (i.e. viewing all top level menus)
                MenuId = "";
                Console.WriteLine(e.Message);
            }
        }

        public override int GetDataCount(string data, string filter)
        {
            ProcessData(data);
            using (var session = Store.OpenSession())
            {
                var query = session.QueryOver<Menu>();
                if (!String.IsNullOrWhiteSpace(MenuId))
                {
                    query = query.Where(m => m.ParentMenu.Id == MenuId);

                    var parentMenu = session.Get<Menu>(MenuId);
                    ParentId = parentMenu.ParentMenu != null ? parentMenu.ParentMenu.Id : "";
                    mDescription = "Menus: " + parentMenu.Name;
                }
                else
                {
                    mDescription = "Menus";
                    query = query.Where(m => m.ParentMenu == null);
                }

                if (!String.IsNullOrWhiteSpace(filter))
                {
                    query = query.WhereRestrictionOn(x => x.Name).IsLike(filter, MatchMode.Anywhere);
                }

                var count = query.RowCount();
                return count;
            }
        }

        public override int GetId()
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