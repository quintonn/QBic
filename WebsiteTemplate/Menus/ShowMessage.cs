using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.Menus
{
    public class ShowMessage : UserConfirmation
    {
        public override string Name
        {
            get
            {
                return "Show Message";
            }
        }

        public override EventNumber Id
        {
            get
            {
                return EventNumber.ShowMessage;
            }
        }

        public override string Description
        {
            get
            {
                return "Show a message to the user";
            }
        }

        public override IList<SiteSpecific.UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>();
            }
        }

        public ShowMessage()
            : base()
        {

        }

        public ShowMessage(string message)
            : base(message, "Ok", "")
        {

        }
    }
}