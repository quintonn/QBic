using System;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ClickableColumn : ViewColumn
    {
        public ClickableColumn(string columnLabel, string keyColumn, string clickItemLabel, int eventNumber, ClickableType clickableType, string parametersToPass = null)
            : base(columnLabel, String.Empty)
        {
            KeyColumn = keyColumn;
            LinkLabel = clickItemLabel;
            EventNumber = eventNumber;
            Event = null;
            ClickableType = clickableType;
            ParametersToPass = parametersToPass;
        }

        public ClickableColumn(string columnLabel, string keyColumn, string clickItemLabel, Event eventItem, ClickableType clickableType, string parametersToPass = null)
            : base(columnLabel, String.Empty)
        {
            KeyColumn = keyColumn;
            LinkLabel = clickItemLabel;
            EventNumber = -1;
            Event = eventItem;
            ClickableType = clickableType;
            ParametersToPass = parametersToPass;
        }

        /// <summary>
        /// The column of the result set who's value will be passed when the link is clicked.
        /// </summary>
        public string KeyColumn { get; set; }

        /// <summary>
        /// This value to display as the link label (innerHTML).
        /// </summary>
        public string LinkLabel { get; set; }
        /// <summary>
        /// The Event Number to execute when the link is clicked.
        /// </summary>
        public int EventNumber { get; set; }
        public ClickableType ClickableType { get; set; }

        public string ParametersToPass { get; private set; }

        public Event Event { get; private set; } 

        public override ColumnType ColumnType
        {
            get
            {
                switch (ClickableType)
                {
                    case ClickableType.Button:
                        return ColumnType.Button;
                    case ClickableType.Link:
                        return ColumnType.Link;
                    default:
                        throw new Exception("Unknown clickable type: " + ClickableType);
                }
            }
        }
    }
}