using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class Condition
    {
        public string ColumnName { get; set; }

        public Comparison Comparison { get; set; }

        public string ColumnValue { get; set; }

        public Condition(string columnName, Comparison comparison, string columnValue = null)
        {
            ColumnName = columnName;
            Comparison = comparison;
            ColumnValue = columnValue;
        }
    }
}