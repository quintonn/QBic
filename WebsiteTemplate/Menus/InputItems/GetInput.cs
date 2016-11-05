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

        public abstract IList<InputField> GetInputFields();

        public IList<InputField> InputFields { get; internal set; }

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

        /// <summary>
        /// By default this returns 2 buttons, 0-Submit and 1-Cancel
        /// </summary>
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
        /// By default this method does nothing except return a succesfull result.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Returns an object containing success status as well as an error message if not successful.</returns>
        public virtual Task<InitializeResult> Initialize(string data)
        {
            return Task.FromResult(new InitializeResult(true));
        }


        /// <summary>
        /// This is called once the input is obtained and a button is clicked.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="actionNumber">This is the even number of the button pressed</param>
        /// <returns></returns>
        public abstract Task<IList<IEvent>> ProcessAction(int actionNumber);

        /// <summary>
        /// Event called when an input property is changed when RaisePropertyChangedEvent is set to true.
        /// </summary>
        /// <param name="propertyName">Name of property changed as specified in the InputFields.</param>
        /// <returns>List of events to perform.</returns>
        public virtual async Task<IList<IEvent>> OnPropertyChanged(string propertyName, object propertyValue)
        {
            return await Task.FromResult<IList<IEvent>>(new List<IEvent>());
        }
    }
}