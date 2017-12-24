using Unity.Attributes;

namespace WebsiteTemplate.Menus.BaseItems
{
    public class UpdateInputView : Event
    {
        [InjectionConstructor]
        public UpdateInputView()
        {
            RowId = -1;
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public UpdateInputView(InputViewUpdateType updateType)
            : this()
        {
            UpdateType = updateType;
        }

        public InputViewUpdateType UpdateType { get; private set; }

        public string JsonDataToUpdate { get; internal set; }

        public int RowId { get; internal set; }

        public override EventType ActionType
        {
            get
            {
                return EventType.UpdateInputView;
            }
        }

        public override string Description
        {
            get
            {
                return "Update Input View";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.UpdateInputView;
        }
    }
}