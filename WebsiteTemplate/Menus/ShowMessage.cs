using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.Menus
{
    public class ShowMessage : UIAction
    {
        //public string Message { get; private set; }

        //private IList<UserRole> mAuthorizedUserRoles { get; set; }

        //public ShowMessage(string message, IList<UserRole> authorizedUserRoles)
        //{
        //    Message = message;
        //    mAuthorizedUserRoles = authorizedUserRoles;
        //}

        public override int Id
        {
            get
            {
                return UIActionNumbers.SHOW_MESSAGE;
            }
        }

        public override string Name
        {
            get
            {
                return "Show Message";
            }
        }

        public override string Description
        {
            get
            {
                return "Show a message to the user";
            }
        }

        public override UIActionType ActionType
        {
            get
            {
                return UIActionType.ShowMessage;
            }
        }

        public override string MenuLabel
        {
            get
            {
                return "Message";
            }
        }

        public override IList<SiteSpecific.UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>();
            }
        }
    }
}