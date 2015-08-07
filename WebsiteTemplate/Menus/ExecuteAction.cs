using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.Menus
{
    public class ExecuteAction : UIAction
    {
        public int UIActionId { get; private set; }

        public ExecuteAction()
        {

        }

        public ExecuteAction(int uiActionId)
        {
            UIActionId = uiActionId;
        }

        public override int Id
        {
            get
            {
                return UIActionNumbers.EXECUTE_ACTION;
            }
        }

        public override string Name
        {
            get
            {
                return "Execute Action";
            }
        }

        public override string Description
        {
            get
            {
                return "Execute Action";
            }
        }

        public override string MenuLabel
        {
            get
            {
                return "Execute Action";
            }
        }

        public override IList<SiteSpecific.UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>();
            }
        }

        public override UIActionType ActionType
        {
            get
            {
                return UIActionType.ExecuteAction;
            }
        }
    }
}