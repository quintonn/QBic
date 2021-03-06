﻿namespace WebsiteTemplate.Menus.BaseItems
{
    public class CancelInputDialog : Event
    {
        public override EventNumber GetId()
        {
            return EventNumber.CancelInputDialog;
        }

        public override string Description
        {
            get
            {
                return "Cancel Input Dialog";
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.CancelInputDialog;
            }
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }
    }
}