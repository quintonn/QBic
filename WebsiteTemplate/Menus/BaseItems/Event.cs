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
using System.Web.Script.Serialization;
using System.Collections;

namespace WebsiteTemplate.Menus.BaseItems
{
    public abstract class Event : IEvent
    {
        [ScriptIgnore]
        [System.Xml.Serialization.XmlIgnore]
        [JsonIgnore]
        public DataStore Store { get; set; }

        //
        // Summary:
        //     Gets or sets the HttpRequestMessage of the current System.Web.Http.ApiController.
        //
        // Returns:
        //     The HttpRequestMessage of the current System.Web.Http.ApiController.
        [ScriptIgnore]
        [System.Xml.Serialization.XmlIgnore]
        [JsonIgnore]
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Hard coded Id.
        /// </summary>
        public abstract EventNumber GetId();

        public string GetEventId()
        {
            return GetId().ToString();
        }

        public virtual Dictionary<string, object> GetEventParameters()
        {
            return null;
        }

        public Dictionary<string, object> EventParameters
        {
            get
            {
                return GetEventParameters();
            }
        }

        public int Id { get { return GetId(); } }

        public abstract string Description { get; }

        /// <summary>
        /// This label will be displayed if a shortcut to this event is to a menu item.
        /// </summary>
        //public abstract string MenuLabel { get; }

        public abstract EventType ActionType { get; }

        public string Parameters { get; set; }
    }
}