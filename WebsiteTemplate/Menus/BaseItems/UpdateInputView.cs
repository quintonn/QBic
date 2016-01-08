using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.BaseItems
{
    public class UpdateInputView : Event
    {
        public UpdateInputView()
        {

        }

        public UpdateInputView(string jsonDataToUpdate)
        {
            JsonDataToUpdate = jsonDataToUpdate;
        }

        public string JsonDataToUpdate { get; set; }

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

        public override int GetId()
        {
            return EventNumber.UpdateInputView;
        }
    }
}