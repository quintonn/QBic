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

        public void RemoveColumn(ViewColumn column)
        {
            Columns.Remove(column);
        }

        public void AddColumn(ViewColumn column)
        {
            Columns.Add(column);
        }

        public void AddStringColumn(string columnLabel, string columnName, int columnSpan)
        {
            Columns.Add(new StringColumn(columnLabel, columnName, columnSpan));
        }

        public void AddStringColumn(string columnLabel, string columnName)
        {
            Columns.Add(new StringColumn(columnLabel, columnName));
        }

        public void AddStringColumn(string columnLabel, string columnName, ColumnSetting columnSetting)
        {
            Columns.Add(new StringColumn(columnLabel, columnName)
            {
                ColumnSetting = columnSetting
            });
        }

        public void AddDateColumn(string columnLabel, string columnName, ColumnSetting columnSetting)
        {
            Columns.Add(new DateColumn(columnLabel, columnName)
            {
                ColumnSetting = columnSetting
            });
        }

        public void AddDateColumn(string columnLabel, string columnName)
        {
            AddDateColumn(columnLabel, columnName, null);
        }

        public void AddHiddenColumn(string columnName)
        {
            Columns.Add(new HiddenColumn("", columnName));
        }

        public void AddBooleanColumn(string columnLabel, string columnName, string trueValue = "True", string falseValue = "False")
        {
            Columns.Add(new BooleanColumn(columnLabel, columnName, trueValue, falseValue));
        }

        public void AddLinkColumn(string columnLabel, string keyColumn, string linkLabel, int eventNumber, ColumnSetting columnSetting = null, string parametersToPass = null)
        {
            Columns.Add(new LinkColumn(columnLabel, keyColumn, linkLabel, eventNumber, parametersToPass)
            {
                ColumnSetting = columnSetting
            });
        }

        public void AddLinkColumn(string columnLabel, string keyColumn, string linkLabel, Event eventItem, ColumnSetting columnSetting = null, string parametersToPass = null)
        {
            Columns.Add(new LinkColumn(columnLabel, keyColumn, linkLabel, eventItem, parametersToPass)
            {
                ColumnSetting = columnSetting
            });
        }

        public void AddButtonColumn(string columnLabel, string keyColumn, string buttonText, int eventNumber, ColumnSetting columnSetting = null, string parametersToPass = null)
        {
            Columns.Add(new ButtonColumn(columnLabel, keyColumn, buttonText, eventNumber, parametersToPass)
            {
                ColumnSetting = columnSetting
            });
        }

        public void AddButtonColumn(string columnLabel, string keyColumn, string linkLabel, Event eventItem, ColumnSetting columnSetting = null, string parametersToPass = null)
        {
            Columns.Add(new ButtonColumn(columnLabel, keyColumn, linkLabel, eventItem, parametersToPass)
            {
                ColumnSetting = columnSetting
            });
        }
    }
}