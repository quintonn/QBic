using Unity;

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

        [InjectionConstructor]
        public DeleteInputViewItem()
        {

        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
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