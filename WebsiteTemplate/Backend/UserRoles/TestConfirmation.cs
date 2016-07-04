using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class TestConfirmation : UserConfirmation
    {
        public TestConfirmation()
            :base("Create a new one?", "Yes", "No")
        {
            OnConfirmationUIAction = EventNumber.TestCreate;
            OnCancelUIAction = WebsiteTemplate.Menus.BaseItems.EventNumber.CancelInputDialog;
        }

        public override int GetId()
        {
            return EventNumber.TestConfirmation;
        }
    }
}