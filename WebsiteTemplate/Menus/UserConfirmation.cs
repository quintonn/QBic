﻿using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus
{
    public class UserConfirmation : Event
    {
        public override EventNumber GetId()
        {
            return EventNumber.UserConfirmation;
        }

        public override string Description
        {
            get
            {
                return "Asks the user for confirmation";
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.ShowMessage;
            }
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public string ConfirmationMessage { get; set; }

        public string CancelButtonText { get; set; }

        public string ConfirmationButtonText { get; set; }

        public EventNumber OnConfirmationUIAction { get; set; }

        public EventNumber OnCancelUIAction { get; set; }

        public object Data { get; set; }

        //[InjectionConstructor]
        public UserConfirmation()
        {

        }

        public UserConfirmation(string confirmationMessage)
            : this(confirmationMessage, "Yes", "No")
        {

        }

        public UserConfirmation(string confirmationMessage, int confirmationAction)
            : this(confirmationMessage, "Yes", "No")
        {
            OnConfirmationUIAction = confirmationAction;
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