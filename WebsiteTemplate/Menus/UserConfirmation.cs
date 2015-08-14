using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.SiteSpecific;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.Menus
{
    public class UserConfirmation : Event
    {
        public override EventNumber GetId()
        {
            return EventNumber.UserConfirmation;
        }

        public override string Name
        {
            get
            {
                return "User Confirmation";
            }
        }

        public override string Description
        {
            get
            {
                return "Asks the user for confirmation";
            }
        }

        public override IList<SiteSpecific.UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>();
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.ShowMessage;
            }
        }

        public string ConfirmationMessage { get; set; }

        public string CancelButtonText { get; set; }

        public string ConfirmationButtonText { get; set; }

        public EventNumber OnConfirmationUIAction { get; set; }

        public EventNumber OnCancelUIAction { get; set; }

        public UserConfirmation()
        {

        }

        public UserConfirmation(string confirmationMessage)
            : this(confirmationMessage, "Yes", "No")
        {

        }

        public UserConfirmation(string confirmationMessage, string confirmationButtonText)
            :this(confirmationMessage, confirmationButtonText, "No")
        {

        }

        public UserConfirmation(string confirmationMessage, string confirmationButtonText, string cancelButtonText)
        {
            ConfirmationMessage = confirmationMessage;
            ConfirmationButtonText = confirmationButtonText;
            CancelButtonText = cancelButtonText;

            OnConfirmationUIAction = EventNumber.Nothing;
            OnCancelUIAction = EventNumber.Nothing;
        }
    }
}