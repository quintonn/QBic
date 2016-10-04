using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class GetDataSettings
    {
        public string ViewData { get; set; }

        public string Filter { get; set; }

        public int CurrentPage { get; set; }

        public int LinesPerPage { get; set; }

        //public string SortColumn { get; set; }

        public GetDataSettings(string viewData, string filter, int currentPage, int linesPerPage)
        {
            ViewData = viewData;
            Filter = filter;
            CurrentPage = currentPage;
            LinesPerPage = linesPerPage;
        }
    }
}