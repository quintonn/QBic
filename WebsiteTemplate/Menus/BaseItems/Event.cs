using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using WebsiteTemplate.Data;
using WebsiteTemplate.SiteSpecific;
using System.Web.Http;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.Menus.BaseItems
{
    public abstract class Event
    {
        internal DataStore Store { get; set; }

        //
        // Summary:
        //     Gets or sets the HttpRequestMessage of the current System.Web.Http.ApiController.
        //
        // Returns:
        //     The HttpRequestMessage of the current System.Web.Http.ApiController.
        internal HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Hard coded Id.
        /// </summary>
        public abstract EventNumber GetId();

        public string GetEventId()
        {
            return ((int)GetId()).ToString();
        }

        public EventNumber Id { get { return GetId(); } }

        public abstract string Description { get; }

        /// <summary>
        /// This label will be displayed if a shortcut to this event is to a menu item.
        /// </summary>
        //public abstract string MenuLabel { get; }

        /// <summary>
        /// Authorized user roles that can execute this UI Action
        /// </summary>
        public abstract IList<UserRole> AuthorizedUserRoles { get; }

        public abstract EventType ActionType { get; }

        /// <summary>
        /// This is data that is passed between events.
        /// Foreach event in a series of events, there will be 1 item in this list.
        /// </summary>
        public Dictionary<int, object> ActionData { get; set; }

        public Event()
        {
            ActionData = new Dictionary<int, object>();
        }
    }
}