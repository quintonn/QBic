using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Menus
{
    public class AddMenu : ModifyMenu
    {
        internal override bool IsNew
        {
            get
            {
                return true;
            }
        }

        public override int GetId()
        {
            return EventNumber.AddMenu;
        }
    }
}