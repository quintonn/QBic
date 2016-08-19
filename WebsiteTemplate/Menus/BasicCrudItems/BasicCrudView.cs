﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Utilities;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public class BasicCrudView<T> : ShowView, IBasicCrudView where T : BaseClass
    {
        public EventNumber Id { get; set; }

        public string ItemName { get; set; }

        public Dictionary<string, string> ColumnsToShowInView { get; set; }

        public override string Description
        {
            get
            {
                if (ItemName.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ItemName;
                }
                return ItemName + "s"; //TODO: Add properties for these sort of names
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            foreach (var col in ColumnsToShowInView)
            {
                columnConfig.AddStringColumn(col.Value, col.Key);
            }

            columnConfig.AddLinkColumn("", "Id", "Edit", Id + 1);

            columnConfig.AddButtonColumn("", "Id", "X",
                new UserConfirmation("Delete " + ItemName + "?")
                {
                    OnConfirmationUIAction = Id + 2
                },
                new ShowHideColumnSetting()
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
            using (var session = Store.OpenSession())
            {
                var result = session.QueryOver<T>()
                                    .Skip((currentPage-1)*linesPerPage)
                                    .Take(linesPerPage)
                                    .List<T>().ToList();
                return result;
            }

        }

        public override int GetDataCount(string data, string filter)
        {
            using (var session = Store.OpenSession())
            {
                var count = session.QueryOver<T>().RowCount();
                return count;
            }
        }

        public override EventNumber GetId()
        {
            return Id;
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            var results = new List<MenuItem>();

            var jsonObject = new JsonHelper();
            jsonObject.Add("IsNew", true);
            var json = jsonObject.ToString();
            results.Add(new MenuItem("Add", Id + 1, json));

            return results;
        }
    }
}