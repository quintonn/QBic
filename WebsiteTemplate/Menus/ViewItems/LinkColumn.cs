using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class LinkColumn : ViewColumn
    {
        public LinkColumn(string columnLabel, string columnName)
            : base(columnLabel, columnName)
        {
        }

        public LinkColumn(string columnLabel, string columnName, string keyColumn, string linkLabel, int eventNumber)
            : this(columnLabel, columnName)
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
        public int EventNumber { get; set; }
    }
}