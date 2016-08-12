using System;
using System.Collections;
using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ShowView : Event
    {
        /// <summary>
        /// The data that will be displayed in the view. Don't modify this property.
        /// It is public so that this class can be serialized to json.
        /// </summary>
        public object ViewData { get; set; }

        /// <summary>
        /// Configure columns for the view.
        /// </summary>
        /// <param name="columnConfig"></param>
        public abstract void ConfigureColumns(ColumnConfiguration columnConfig);

        private IList<ViewColumn> DoConfigureColumns()
        {
            var config = new ColumnConfiguration();
            ConfigureColumns(config);
            return config.GetColumns();
        }

        /// <summary>
        /// The columns to show in the view.
        /// </summary>
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
        /// This method is called to obtain all the information
        /// </summary>
        /// <param name="data">This is data passed from the web request.</param>
        /// <param name="currentPage">This is the current page to retrieve data for.</param>
        /// <param name="linesPerPage">This is the number of lines per page to retrieve.</param>
        /// <returns></returns>
        public abstract IEnumerable GetData(string data, int currentPage, int linesPerPage, string filter);

        /// <summary>
        /// Retrieves the total number of records in the data set.
        /// This is used to calculate the number of pages that will be available in the view.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract int GetDataCount(string data, string filter);

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

        public abstract IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu);

        public IList<MenuItem> ViewMenu { get; set; }

        public virtual Dictionary<string, string> DataForGettingMenu
        {
            get
            {
                return new Dictionary<string, string>();
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.DataView;
            }
        }

        public virtual string GetViewMessage()
        {
            return "";
        }

        public string ViewMessage
        {
            get
            {
                //TODO: This might need a more complex parser to handle other special characters.
                //   OR
                // I just allow css/html and display that as is.
                //   OR
                // If this all becomes editable in the browser, have a proper editor
                var result =  GetViewMessage();
                result = result.Replace("\r", "<br/>").Replace("\n", "<br/>");
                return result;
            }
        }

        public int Pages { get; set; }

        public int LinesPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalLines { get; set; }
        public string Filter { get; set; }
    }
}