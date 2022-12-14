using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.PropertyChangedEvents
{
    public class UpdateInputVisibility : Event
    {
        public string InputName { get; set; }
        public bool InputIsVisible { get; set; }

        public override EventType ActionType
        {
            get
            {
                return EventType.UpdateInputVisibility;
            }
        }

        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public override string Description
        {
            get
            {
                return "Update Input Visibility";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.UpdateInputVisibility;
        }

        //[InjectionConstructor]
        public UpdateInputVisibility()
        {

        }

        public UpdateInputVisibility(string inputName, bool inputIsVisible)
        {
            InputName = inputName;
            InputIsVisible = inputIsVisible;
        }
    }
}