using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class StringColumn : ViewColumn
    {
        public StringColumn(string columnLabel, string dbColumnName, string columnName)
            : base(columnLabel, dbColumnName, columnName)
        {
        }

        public override ColumnType ColumnType
        {
            get
            {
                return ViewItems.ColumnType.String;
            }
        }
    }
}