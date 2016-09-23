using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ShowDetailView : ShowView
    {
        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            //do nothing
            columnConfig.AddHiddenColumn("val"); // just to get 1 column
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.DetailView;
            }
        }

        /// <summary>
        /// Specifies if the data returned by <see cref="GetDetailItems"/> is plain text or html.
        /// </summary>
        public abstract bool DetailIsHtml { get; }

        /// <summary>
        /// Returns the detail to be displayed in the view.
        /// This can be either plain text or html, depending on the value returned by <see cref="DetailIsHtml"/>.
        /// </summary>
        /// <returns></returns>
        public abstract IList<String> GetDetailItems();

        public override IEnumerable GetData(string data, int currentPage, int linesPerPage, string filter)
        {
            return GetDetailItems().Select(x => new
            {
               val = x
            }).ToList();
        }

        public override int GetDataCount(string data, string filter)
        {
            return 0; // don't think it matters
        }
    }
}