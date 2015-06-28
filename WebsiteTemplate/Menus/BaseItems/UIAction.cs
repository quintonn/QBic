using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using WebsiteTemplate.Data;
using WebsiteTemplate.SiteSpecific;
using System.Web.Http;

namespace WebsiteTemplate.Menus.BaseItems
{
    public abstract class UIAction
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
        public abstract int Id { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        /// <summary>
        /// This label will be displayed if a shortcut to this UIAction is to a menu item.
        /// </summary>
        public abstract string MenuLabel { get; }

        /// <summary>
        /// Authorized user roles that can execute this UI Action
        /// </summary>
        public abstract IList<UserRole> AuthorizedUserRoles { get; }

        public abstract UIActionType ActionType { get; }

        /// <summary>
        /// This is data that is passed between UIAction.
        /// Foreach UIAction in a series of events, there will be 1 item in this list.
        /// </summary>
        public IList<object> ActionData { get; set; }

        public UIAction()
        {
            ActionData = new List<object>();
        }
    }
}