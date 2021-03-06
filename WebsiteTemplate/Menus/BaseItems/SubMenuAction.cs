﻿using System.Collections.Generic;

namespace WebsiteTemplate.Menus.BaseItems
{
    /// <summary>
    /// This class is for sub menus. 
    /// This allows creating a sub menu
    /// </summary>
    public abstract class SubMenuAction : Event
    {
        /// <summary>
        /// This contains the id's of the UIActions that should appear in this SubMenu.
        /// This could potentially be other SubMenuAction id's as well to create sub-sub-sub menus etc.
        /// </summary>
        public abstract IList<int> ChildActionIds { get; }

        public IList<IEvent> ChildUIActions
        {
            get
            {
                /// Use reflection here to get the list of all the UIAction items based on their ids from ChildActionIds
                /// This is so that we can get the Name, Label, Etc, Etc
                return new List<IEvent>();
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.SubMenu;
            }
        }
    }
}