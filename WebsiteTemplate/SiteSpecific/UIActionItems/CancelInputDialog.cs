using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.UIActionItems
{
    public class CancelInputDialog : UIAction
    {
        public override int Id
        {
            get
            {
                return UIActionNumbers.CANCEL_INPUT_DIALOG;
            }
        }

        public override string Name
        {
            get
            {
                return "Cancel";
            }
        }

        public override string Description
        {
            get
            {
                return "Cancel Input Dialog";
            }
        }

        public override string MenuLabel
        {
            get
            {
                return "Cancel";
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>()
                {
                    UserRole.AnyOne
                };
            }
        }

        public override UIActionType ActionType
        {
            get
            {
                return UIActionType.CancelInputDialog;
            }
        }
    }
}