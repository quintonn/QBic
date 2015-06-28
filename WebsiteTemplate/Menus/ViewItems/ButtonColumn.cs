using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class ButtonColumn : ViewColumn
    {
        public ButtonColumn(string columnLabel, string dbColumnName, string columnName, int uiActionId)
            : base(columnLabel, dbColumnName, columnName)
        {
            UIActionId = uiActionId;
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

        public int UIActionId { get; set; }
    }
}