using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ShowView : Event
    {
        public Object ViewData { get; set; }
        /// <summary>
        /// The columns to show in the view.
        /// </summary>
        //public abstract IList<ViewColumn> Columns{ get; }

        /// <summary>
        /// Add columns for the view.
        /// </summary>
        /// <param name="columnConfig"></param>
        public abstract void ConfigureColumns(ColumnConfiguration columnConfig);

        private IList<ViewColumn> DoConfigureColumns()
        {
            var config = new ColumnConfiguration();
            ConfigureColumns(config);
            return config.GetColumns();
        }

        public IList<ViewColumn> Columns { get { return DoConfigureColumns(); } }

        /// <summary>
        /// Name of database table from which to retrieve the columns.
        /// </summary>
        //public abstract string TableName { get; }

        /// <summary>
        /// The DB query to retrieve the data for the columns.
        /// </summary>
        //public abstract string DbQuery { get; }


        /// <summary>
        /// The type/class to retrieve from the DB. (Using NHibernat -> this is what i need at this point).
        /// </summary>
        public abstract Type GetDataType();

        /// <summary>
        /// This should contain stuff like:
        ///    * Highlight the row if a certain column has a certain value
        ///    * Enable or specify entire row on click events
        ///    * etc...
        /// </summary>
        //public abstract IList<object> RowSettings { get; }

        /// <summary>
        /// What if we need to limit the results in the view based on user name, role, etc, etc.
        ///   * This might have to include stuff like WHERE clause for db query
        ///   * Or, a function to manually filter the results
        ///   * etc...
        /// </summary>
        //public abstract IList<object> OtherSettings { get; }

        public abstract IList<MenuItem> ViewMenu { get; }

        public override EventType ActionType
        {
            get
            {
                return EventType.DataView;
            }
        }

        public virtual string ViewMessage { get { return ""; } }
    }
}