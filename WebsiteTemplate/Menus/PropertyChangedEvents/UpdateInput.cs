using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.PropertyChangedEvents
{
    public class UpdateInput : Event
    {
        public string InputName { get; set; }
        public object InputValue { get; set; }

        public override EventType ActionType
        {
            get
            {
                return EventType.UpdateInput;
            }
        }

        public override string Description
        {
            get
            {
                return "Update Input";
            }
        }

        public override int GetId()
        {
            return EventNumber.UpdateInput;
        }

        public UpdateInput()
        {

        }

        public UpdateInput(string inputName, object inputValue)
        {
            InputName = inputName;
            InputValue = inputValue;
        }
    }
}