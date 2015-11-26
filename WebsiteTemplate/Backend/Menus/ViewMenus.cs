using NHibernate.Criterion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            results.Add(new MenuItem("Add", EventNumber.AddMenu, MenuId));

            return results;
        }

        private string MenuId { get; set; }
        private string ParentId { get; set; }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Name", "Name");
            //columnConfig.AddStringColumn("IsView", "IsView");
            columnConfig.AddStringColumn("Event", "Event", new ShowHideColumnSetting()
            {
                Display = ColumnDisplayType.Hide,
                Conditions = new List<Condition>()
                {
                    new Condition("Event", Comparison.Equals, "")
                }
            });

            columnConfig.AddButtonColumn("Sub Menus", "", ButtonTextSource.Fixed, "...", new ShowHideColumnSetting()
            {
                Display = ColumnDisplayType.Show,
                Conditions = new List<Condition>()
                {
                    new Condition("Event", Comparison.Equals, ""),
                }
            }, new ExecuteAction(EventNumber.ViewMenus, MenuId));


            columnConfig.AddLinkColumn("", "Edit", "Id", "Edit", EventNumber.EditMenu);

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

        public override IEnumerable GetData(string data)
        {
            MenuId = data;
            using (var session = Store.OpenSession())
            {
                var query = session.CreateCriteria<Menu>();
                if (!String.IsNullOrWhiteSpace(data))
                {
                    query = query.CreateAlias("ParentMenu", "parent")
                                 .Add(Restrictions.Eq("parent.Id", data));

                    var parentMenu = session.Get<Menu>(data);
                    ParentId = parentMenu.ParentMenu != null ? parentMenu.ParentMenu.Id : "";
                    mDescription = "Menus: " + parentMenu.Name;
                }
                else
                {
                    mDescription = "Menus";
                    query = query.Add(Restrictions.IsNull("ParentMenu"));
                }
                var results = query
                       .List<Menu>()
                       .ToList();

                //var eventIds = results.Select(r => r.Event).ToList();
                //var events = Controllers.MainController.EventList.Where(e => eventIds.Contains(e.Key) && e.Value is ShowView).ToList();

                var newList = results.Select(r => new
                {
                    Name = r.Name,
                    Id = r.Id,
                    Event = r.Event == null ? "" : r.Event.Value.ToString(),
                    ParentMenu = r.ParentMenu,
                    CanDelete = r.CanDelete,
                }).ToList();

                return newList;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.ViewMenus;
        }
    }
}