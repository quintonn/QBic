using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class ButtonColumn : ViewColumn
    {
        public ButtonColumn(string columnLabel, string dbColumnName, string columnName)
            : base(columnLabel, dbColumnName, columnName)
        {
        }

        public ButtonColumn(string columnLabel, string dbColumnName, string columnName, ButtonTextSource buttonTextSource, string buttonText)
            : base(columnLabel, dbColumnName, columnName)
        {
            ButtonTextSource = buttonTextSource;
            ButtonText = buttonText;
        }

        public override ColumnType ColumnType
        {
            get
            {
                return ViewItems.ColumnType.Button;
            }
        }

        public string ButtonText { get; set; }

        public ButtonTextSource ButtonTextSource { get; set; }

        public Event Event { get; set; }
    }
}