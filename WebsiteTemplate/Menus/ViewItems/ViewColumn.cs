using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ViewColumn
    {
        public ViewColumn(string columnLabel, string columnName)
        {
            ColumnLabel = columnLabel;
            ColumnName = columnName;
            ColumnSetting = null;
        }

        /// <summary>
        /// This will be column heading/label in the view
        /// </summary>
        public string ColumnLabel { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// This is the name of the column in the result set / results from DB query.
        /// Leave blank if the column does not obtain info from the result set or table.
        /// </summary>
        public string DbColumnName { get; set; }

        /// <summary>
        /// The type of column
        /// </summary>
        public abstract ColumnType ColumnType { get; }

        public ColumnSetting ColumnSetting { get; set; }
        // show this column if another column == ???
        // hide this column if another column == ???
        // hide this column if no rows are populated

        /// Can add other stuff like formating, styling, etc
    }
}