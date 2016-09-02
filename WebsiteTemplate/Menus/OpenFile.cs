﻿using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Menus
{
    public abstract class OpenFile : Event
    {
        private string Data { get; set; }

        internal void SetData(string data)
        {
            Data = data;
        }

        public string DataUrl
        {
            get
            {
                return "/GetFile/" + GetEventId();
            }
        }

        public string RequestData
        {
            get
            {
                return Data;
            }
        }

        public abstract FileInfo GetFileInfo(string data);

        public override EventType ActionType
        {
            get
            {
                return EventType.ViewFile;
            }
        }
    }
}