using System.Collections.Generic;

namespace WebsiteTemplate.Menus.BaseItems
{
    public interface IEvent
    {
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