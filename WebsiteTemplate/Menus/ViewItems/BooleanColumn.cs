using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class BooleanColumn : ViewColumn
    {
        public BooleanColumn(string columnLabel, string dbColumnName, string columnName)
            : base(columnLabel, dbColumnName, columnName)
        {
        }

        public BooleanColumn(string columnLabel, string dbColumnName, string columnName, string trueValue, string falseValue)
            : this(columnLabel, dbColumnName, columnName)
        {
            TrueValueDisplay = trueValue;
            FalseValueDisplay = falseValue;
        }

        public override ColumnType ColumnType
        {
            get
            {
                return ViewItems.ColumnType.Boolean;
            }
        }

        public string TrueValueDisplay { get; set; }

        public string FalseValueDisplay { get; set; }
    }
}