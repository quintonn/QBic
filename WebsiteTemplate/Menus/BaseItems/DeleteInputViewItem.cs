using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.BaseItems
{
    public class DeleteInputViewItem : Event
    {
        public override EventType ActionType
        {
            get
            {
                return EventType.DeleteInputViewItem;
            }
        }

        public DeleteInputViewItem()
        {

        }

        public DeleteInputViewItem(int rowId)
        {
            RowId = rowId;
        }

        public int RowId { get; set; }

        public override string Description
        {
            get
            {
                return "Delete Input View Item";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.DeleteInputViewItem;
        }
    }
}