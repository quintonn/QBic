﻿using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class TestConfirmation : UserConfirmation
    {
        public TestConfirmation()
            :base("Create a new one?", "Yes", "No")
        {
            OnConfirmationUIAction = EventNumber.TestCreate;
            OnCancelUIAction = EventNumber.CancelInputDialog;
        }

        public override string Description
        {
            get
            {
                return "Test Confirmation";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.TestConfirmation;
        }
    }
}