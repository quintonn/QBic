using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Utilities;

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

        /// <summary>
        /// Setting to set whether sorting on the page is allowed.
        /// This is true by default
        /// </summary>
        public virtual bool AllowSorting => true;
        

        /// <summary>
        /// Returns the title that will be displayed when this view is rendered.
        /// </summary>
        public virtual string Title
        {
            get
            {
                return Description;
            }
        }

        /// <summary>
        /// If greater than 0, will auto refresh the screen every X milliseconds
        /// </summary>
        public virtual int AutoRefreshScreenTime
        {
            get
            {
                return 0;
            }
        }

        internal IList<ViewColumn> DoConfigureColumns(IList<int> allowedUserEvents)
        {
            var config = new ColumnConfiguration();
            ConfigureColumns(config);

            var columns = config.GetColumns().Where(c => AllowColumn(c, allowedUserEvents)).ToList();
            return columns;
        }

        private bool AllowColumn(ViewColumn column, IList<int> allowedEvents)
        {
            var col = column as ClickableColumn;
            if (col == null)
            {
                return true;
            }
            
            var eventNumber = col.Event == null ? col.EventNumber : col.Event.GetEventId();
            if (col.Event != null && col.Event is UserConfirmation)
            {
                eventNumber = (col.Event as UserConfirmation).OnConfirmationUIAction;
            }
            return allowedEvents.Contains(eventNumber);
        }

        /// <summary>
        /// The columns to show in the view.
        /// </summary>
        public IList<ViewColumn> Columns { get; internal set; }

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
        public abstract IEnumerable GetData(GetDataSettings settings);

        /// <summary>
        /// Retrieves the total number of records in the data set.
        /// This is used to calculate the number of pages that will be available in the view.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract int GetDataCount(GetDataSettings settings);

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

        public virtual IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        {
            return new List<MenuItem>();
        }

        public IList<MenuItem> ViewMenu { get; set; }

        /// <summary>
        /// Retrieves data that will be passed to the view when retrieving the view menu.
        /// There is a separate web call to retrieve the menu, so that the system can determine which menus the user is allowed
        /// to access and see.
        /// </summary>
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

        protected string GetParameter(string parameterName, string alternateName, GetDataSettings settings, bool allowNull = true)
        {
            var item = GetParameter(parameterName, settings, true);
            if (String.IsNullOrWhiteSpace(item))
            {
                item = GetParameter(alternateName, settings, allowNull);
            }
            return item;
        }

        protected string GetParameter(string parameterName, GetDataSettings settings, bool allowNull = true)
        {
            var data = settings.ViewData;
            var json = JsonHelper.Parse(data);
            var tmpData = json.GetValue("data");
            var result = String.Empty;
            if (!String.IsNullOrWhiteSpace(tmpData))
            {
                var x = JsonHelper.Parse(tmpData);
                result = x.GetValue(parameterName);
                //if (String.IsNullOrWhiteSpace(result))
                //{
                //    var parameters = json.GetValue("parameters");
                //    if (!String.IsNullOrWhiteSpace(parameters))
                //    {
                //        var paramJson = JsonHelper.Parse(parameters);
                //        result = paramJson.GetValue(parameterName);
                //    }
                //    else if (!String.IsNullOrWhiteSpace(data))
                //    {
                //        result = tmpData;
                //    }
                //    //result = tmpData;
                //}
            }
            else
            {
                var eventParams = json.GetValue("eventParameters");
                if (!String.IsNullOrWhiteSpace(eventParams))
                {
                    var x = JsonHelper.Parse(eventParams);
                    result = x.GetValue(parameterName);
                }
                //else if (!String.IsNullOrWhiteSpace(data))
                //{
                //    result = data;
                //}
            }

            if (String.IsNullOrWhiteSpace(result))
            {
                var parameters = json.GetValue("parameters");
                if (!String.IsNullOrWhiteSpace(parameters))
                {
                    var paramJson = JsonHelper.Parse(parameters);
                    result = paramJson.GetValue(parameterName);
                }
                else if (!String.IsNullOrWhiteSpace(tmpData))
                {
                    result = tmpData;
                }
                else
                {
                    // this is not right i don't think, if the data is just 1 value then the calling code should check for it i think.

                    //result = data; // I don't think this is working
                                   // If I expect to have "ClientId" for example, and i open a edit screen and come back, 
                                   // it does not parse the client id correctly
                }
                //result = tmpData;
            }

            if (!allowNull && String.IsNullOrWhiteSpace(result))
            {
                throw new Exception("Parameter " + parameterName + " should not be null");
            }

            return result;
        }

        public int Pages { get; set; }

        public int LinesPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalLines { get; set; }
        public string Filter { get; set; }
        public string SortColumn { get; set; }
        public bool SortAscending { get; set; }
    }
}