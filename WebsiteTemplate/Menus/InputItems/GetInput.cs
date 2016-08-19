using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.InputItems
{
    /// <summary>
    /// The class can be inherited for UI actions that require input from the user.
    /// </summary>
    public abstract class GetInput : InputProcessingEvent
    {
        public override EventType ActionType
        {
            get
            {
                return EventType.UserInput;
            }
        }

        public abstract IList<InputField> InputFields { get; }

        /// <summary>
        /// Returns the title that will be displayed for this input screen.
        /// </summary>
        public virtual string Title
        {
            get
            {
                return Description;
            }
        }

        //public abstract IList<Event> InputButtons { get; } //TODO: This should be something else, not event.
        //public abstract IList<InputButton> InputButtons { get; }
        public virtual IList<InputButton> InputButtons
        {
            get
            {
                return new List<InputButton>()
                {
                    new InputButton("Submit", 0),
                    new InputButton("Cancel", 1, false)
                };
            }
        }

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
        public abstract Task<IList<Event>> ProcessAction(int actionNumber);

        /// <summary>
        /// Event called when an input property is changed when RaisePropertyChangedEvent is set to true.
        /// </summary>
        /// <param name="propertyName">Name of property changed as specified in the InputFields.</param>
        /// <returns>List of events to perform.</returns>
        public virtual async Task<IList<Event>> OnPropertyChanged(string propertyName, object propertyValue)
        {
            return await Task.FromResult<IList<Event>>(new List<Event>());
        }
    }
}