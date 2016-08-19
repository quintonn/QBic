using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public interface IBasicCrudMenuItem
    {
        Type InnerType { get; }

        EventNumber GetId();

        Dictionary<string, string> GetColumnsToShowInView();

        Dictionary<string, string> GetInputProperties();

        EventNumber GetBaseMenuId();

        string GetBaseItemName();
    }

    public abstract class BasicCrudMenuItem<T> : Event, IBasicCrudMenuItem where T : BaseClass
    {
        public override EventType ActionType
        {
            get
            {
                return EventType.CancelInputDialog;
            }
        }

        public Type InnerType
        {
            get
            {
                return typeof(T);
            }
        }

        public override string Description
        {
            get
            {
                return String.Empty;
            }
        }

        public override EventNumber GetId()
        {
            return 0;
        }

        public abstract Dictionary<string, string> GetColumnsToShowInView();

        public abstract Dictionary<string, string> GetInputProperties();

        public abstract EventNumber GetBaseMenuId();

        public abstract string GetBaseItemName();

    }
}