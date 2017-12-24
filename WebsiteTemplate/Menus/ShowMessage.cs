using System;
using WebsiteTemplate.Menus.BaseItems;
using Unity.Attributes;

namespace WebsiteTemplate.Menus
{
    public class ShowMessage : UserConfirmation
    {
        public override EventNumber GetId()
        {
            return EventNumber.ShowMessage;
        }

        public override string Description
        {
            get
            {
                return "Show a message to the user";
            }
        }

        [InjectionConstructor]
        public ShowMessage()
            : base()
        {

        }

        public ShowMessage(string message)
            : base(message, "Ok", "")
        {

        }

        public ShowMessage(string format, params object[] args)
            : base(String.Format(format, args), "Ok", "")
        {

        }
    }
}