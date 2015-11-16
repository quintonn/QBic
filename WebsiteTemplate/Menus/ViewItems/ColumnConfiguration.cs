using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class ColumnConfiguration
    {
        private IList<ViewColumn> Columns { get; set; }

        public ColumnConfiguration()
        {
            Columns = new List<ViewColumn>();
        }

        public IList<ViewColumn> GetColumns()
        {
            return Columns;
        }

        public void AddStringColumn(string columnLabel, string columnName)
        {
            Columns.Add(new StringColumn(columnLabel, columnName));
        }

        public void AddBooleanColumn(string columnLabel, string columnName, string trueValue = "True", string falseValue = "False")
        {
            Columns.Add(new BooleanColumn(columnLabel, columnName, trueValue, falseValue));
        }

        public void AddLinkColumn(string columnLabel, string columnName, string keyColumn, string linkLabel, EventNumber eventNumber)
        {
            AddLinkColumn(columnLabel, columnName, keyColumn, linkLabel, eventNumber, null);
        }

        public void AddLinkColumn(string columnLabel, string columnName, string keyColumn, string linkLabel, EventNumber eventNumber, ColumnSetting columnSetting)
        {
            Columns.Add(new LinkColumn(columnLabel, columnName, keyColumn, linkLabel, eventNumber)
            {
                ColumnSetting = columnSetting
            });
        }

        public void AddButtonColumn(string columnLabel, string columnName, ButtonTextSource buttonTextSource, string buttonText)
        {
            Columns.Add(new ButtonColumn(columnLabel, columnName, buttonTextSource, buttonText));
        }

        public void AddButtonColumn(string columnLabel, string columnName, ButtonTextSource buttonTextSource, string buttonText, ColumnSetting columnSetting, Event eventItem)
        {
            Columns.Add(new ButtonColumn(columnLabel, columnName, buttonTextSource, buttonText)
                {
                    ColumnSetting = columnSetting,
                    Event = eventItem
                });
        }
    }
}