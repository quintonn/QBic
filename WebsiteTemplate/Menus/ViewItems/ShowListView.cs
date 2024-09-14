using System;
using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ShowListView : ShowView
    {

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddLinkColumn(String.Empty, GetIdColumnName(), String.Empty, GetDetailViewEventNumber());
            columnConfig.AddStringColumn(String.Empty, GetHeadingColumnName());
            var detailColumns = GetDetailColumnNames() ?? new List<string>();
            foreach (var col in detailColumns)
            {
                columnConfig.AddStringColumn(String.Empty, col);
            }
        }

        /// <summary>
        /// Returns the name of the column/field of the id. 
        /// This is used when the user selects the item to load the details view.
        /// </summary>
        /// <returns></returns>
        public abstract string GetIdColumnName();

        /// <summary>
        /// The event number of the detail view to show when user selects item.
        /// </summary>
        /// <returns></returns>
        public abstract EventNumber GetDetailViewEventNumber();

        /// <summary>
        /// Returns the column/field that will be the main item shown in bold.
        /// This must return a valid field/column.
        /// </summary>
        /// <returns></returns>
        public abstract string GetHeadingColumnName();

        /// <summary>
        /// Returns the list of columns/fields to show on the view.
        /// Each item will be shown beneath each other in the view.
        /// This can return an empty list or null as it's not mandatory.
        /// </summary>
        /// <returns></returns>
        public abstract IList<string> GetDetailColumnNames();

        public override int GetDataCount(GetDataSettings settings)
        {
            return 0; // don't think it matters
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.ListView;
            }
        }
    }
}