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
        private string mDescription = "Menus";
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

            columnConfig.AddButtonColumn("Sub Menus", "", ButtonTextSource.Fixed, "...", new ShowHideColumnSetting()
            {
                Display = ColumnDisplayType.Show,
                Conditions = new List<Condition>()
                {
                    new Condition("Event", Comparison.Equals, ""),
                }
            }, new ExecuteAction(EventNumber.ViewMenus, MenuId));

            columnConfig.AddLinkColumn("", "", "Id", "Edit", EventNumber.ModifyMenu);

            columnConfig.AddButtonColumn("", "", ButtonTextSource.Fixed, "X",
                columnSetting: new ShowHideColumnSetting()
                {
                    Display = ColumnDisplayType.Show,
                    Conditions = new List<Condition>()
                   {
                       new Condition("CanDelete", Comparison.Equals, "true")
                   }
                },
                eventItem: new UserConfirmation("Delete Menu Item?")
                {
                    OnConfirmationUIAction = EventNumber.DeleteMenu
                }
            );
        }

        public override IEnumerable GetData(string data, int currentPage, int linesPerPage)
        {
            MenuId = data;
            using (var session = Store.OpenSession())
            {
                var query = session.QueryOver<Menu>();
                if (!String.IsNullOrWhiteSpace(data))
                {
                    query = query.Where(m => m.ParentMenu.Id == data);

                    var parentMenu = session.Get<Menu>(data);
                    ParentId = parentMenu.ParentMenu != null ? parentMenu.ParentMenu.Id : "";
                    mDescription = "Menus: " + parentMenu.Name;
                }
                else
                {
                    mDescription = "Menus";
                    query = query.Where(m => m.ParentMenu == null);
                }
                var results = query
                       .Skip((currentPage-1)*linesPerPage)
                       .Take(linesPerPage)
                       .List<Menu>()
                       .ToList();

                var newList = results.Select(r => new
                {
                    Name = r.Name,
                    Id = r.Id,
                    Event = r.Event == null ? "" : MainController.EventList[r.Event.Value].Description,
                    ParentMenu = r.ParentMenu,
                    CanDelete = r.CanDelete,
                }).ToList();

                return newList;
            }
        }

        public override int GetDataCount(string data)
        {
            MenuId = data;
            using (var session = Store.OpenSession())
            {
                var query = session.QueryOver<Menu>();
                if (!String.IsNullOrWhiteSpace(data))
                {
                    query = query.Where(m => m.ParentMenu.Id == data);

                    var parentMenu = session.Get<Menu>(data);
                    ParentId = parentMenu.ParentMenu != null ? parentMenu.ParentMenu.Id : "";
                    mDescription = "Menus: " + parentMenu.Name;
                }
                else
                {
                    mDescription = "Menus";
                    query = query.Where(m => m.ParentMenu == null);
                }
                var count = query.RowCount();
                return count;
            }
        }

        public override int GetId()
        {
            return EventNumber.ViewMenus;
        }
    }
}