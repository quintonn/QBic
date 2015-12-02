using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using WebsiteTemplate.Data;
using WebsiteTemplate.SiteSpecific;
using System.Linq;

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
        public abstract int GetId();

        public string GetEventId()
        {
            return ((int)GetId()).ToString();
        }

        public int Id { get { return GetId(); } }

        public abstract string Description { get; }

        /// <summary>
        /// This label will be displayed if a shortcut to this event is to a menu item.
        /// </summary>
        //public abstract string MenuLabel { get; }

        public abstract EventType ActionType { get; }
    }
}