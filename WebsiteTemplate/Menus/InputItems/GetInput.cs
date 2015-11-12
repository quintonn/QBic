using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.InputItems
{
    /// <summary>
    /// The class can be inherited for UI actions that require input from the user.
    /// </summary>
    public abstract class GetInput : Event
    {
        public override EventType ActionType
        {
            get
            {
                return EventType.UserInput;
            }
        }

        public abstract IList<InputField> InputFields { get; }

        //public abstract IList<Event> InputButtons { get; } //TODO: This should be something else, not event.
        public abstract IList<InputButton> InputButtons { get; }

        /// <summary>
        /// This is called right before obtaining InputFields and InputButtons.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Returns an object containing success status as well as an error message if not successful.</returns>
        public abstract Task<InitializeResult> Initialize(string data);

        /// <summary>
        /// This is called once the input is obtained and a button is clicked.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="actionNumber">This is the even number of the button pressed</param>
        /// <returns></returns>
        public abstract Task<IList<Event>> ProcessAction(string data, int actionNumber);
    }
}