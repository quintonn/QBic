using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public class BasicCrudView<T> : ShowView where T : BaseClass
    {
        private int Id { get; set; }

        private string ItemName { get; set; }

        private Dictionary<string, string> ColumnsToShowInView { get; set; }

        public BasicCrudView(int id, string itemName, Dictionary<string, string> columnsToShowInView)
        {
            Id = id;
            ItemName = itemName;
            ColumnsToShowInView = columnsToShowInView;
        }

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

        public override int GetId()
        {
            return Id;
        }

        public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            var results = new List<MenuItem>();

            var jsonObject = new JObject();
            jsonObject.Add("IsNew", true);
            var json = jsonObject.ToString();
            results.Add(new MenuItem("Add", Id + 1, json));

            return results;
        }
    }
}