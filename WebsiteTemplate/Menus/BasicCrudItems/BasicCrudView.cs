using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Data;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public class BasicCrudView<T> : ShowView, IBasicCrudView where T : BaseClass
    {
        private DataStore Store { get; set; }

        public BasicCrudView(DataStore store)
        {
            Store = store;
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public EventNumber Id { get; set; }

        public string ItemName { get; set; }

        public IList<ViewColumn> AdditionalColumns { get; set; }

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

            foreach (var col in AdditionalColumns)
            {
                columnConfig.AddColumn(col);
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

        public override IEnumerable GetData(GetDataSettings settings)
        {
            using (var session = Store.OpenSession())
            {
                var result = session.QueryOver<T>()
                                    .Skip((settings.CurrentPage-1)*settings.LinesPerPage)
                                    .Take(settings.LinesPerPage)
                                    .List<T>().ToList();
                return result;
            }
        }

        public override int GetDataCount(GetDataSettings settings)
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