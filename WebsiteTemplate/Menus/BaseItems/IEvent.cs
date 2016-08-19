using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using WebsiteTemplate.Data;

namespace WebsiteTemplate.Menus.BaseItems
{
    public interface IEvent
    {
        DataStore Store { get; set; }
        HttpRequestMessage Request { get; set; }
        EventNumber GetId();
        string GetEventId();
        Dictionary<string, object> GetEventParameters();
        Dictionary<string, object> EventParameters { get; }
        int Id { get; }
        string Description { get; }
        EventType ActionType { get; }
        string Parameters { get; set; }
    }
}