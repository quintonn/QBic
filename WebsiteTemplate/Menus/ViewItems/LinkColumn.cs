using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class LinkColumn : ViewColumn
    {
        public LinkColumn(string columnLabel, string dbColumnName, string columnName)
            : base(columnLabel, dbColumnName, columnName)
        {
        }

        public LinkColumn(string columnLabel, string dbColumnName, string columnName, string keyColumn, string linkLabel, EventNumber eventNumber)
            : this(columnLabel, dbColumnName, columnName)
        {
            KeyColumn = keyColumn;
            LinkLabel = linkLabel;
            EventNumber = eventNumber;
        }

        /// <summary>
        /// The column of the result set who's value will be passed when the link is clicked.
        /// </summary>
        public string KeyColumn { get; set; }

        /// <summary>
        /// This value to display as the link label (innerHTML).
        /// </summary>
        public string LinkLabel { get; set; }

        public override ColumnType ColumnType
        {
            get
            {
                return ViewItems.ColumnType.Link;
            }
        }

        /// <summary>
        /// The Event Number to execute when the link is clicked.
        /// </summary>
        public EventNumber EventNumber { get; set; }
    }
}