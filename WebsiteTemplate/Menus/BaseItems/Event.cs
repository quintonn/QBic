using System.Collections.Generic;

namespace WebsiteTemplate.Menus.BaseItems
{
    public abstract class Event : IEvent
    {
        /// <summary>
        /// Hard coded Id.
        /// </summary>
        public abstract EventNumber GetId();

        public int GetEventId()
        {
            return GetId();
        }

        /// <summary>
        /// Determines whether or not this event allows anonymous users to execute the event without being logged in.
        /// </summary>
        public virtual bool RequiresAuthorization
        {
            get
            {
                return true;
            }
        }

        public virtual bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns data to be passed from the front end when the user actions an event
        /// </summary>
        public virtual Dictionary<string, object> GetEventParameters()
        {
            return null;
        }

        /// <summary>
        /// Returns data to be passed from the front end when the user actions an event
        /// </summary>
        public Dictionary<string, object> EventParameters
        {
            get
            {
                return GetEventParameters();
            }
        }

        public int Id { get { return GetId(); } }

        public abstract string Description { get; }

        public abstract EventType ActionType { get; }

        public string Parameters { get; set; } //TODO: not sure what the different is between this and event parameters
    }
}