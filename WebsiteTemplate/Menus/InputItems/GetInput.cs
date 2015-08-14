using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.InputItems
{
    /// <summary>
    /// The class can be inherited for UI actions that require input from the user.
    /// </summary>
    public abstract class GetInput : Event
    {
        public override UIActionType ActionType
        {
            get
            {
                return UIActionType.UserInput;
            }
        }

        public abstract IList<InputField> InputFields { get; }

        public abstract IList<Event> InputButtons { get; }
    }
}