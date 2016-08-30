using System.Collections;
using System.Collections.Generic;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ShowViewUsingDataService<TDataItemService, TBaseClass> : ShowView where TDataItemService : InputProcessingCore<TBaseClass> where TBaseClass : BaseClass
    {
        protected TDataItemService DataItemService { get; set; }

        public ShowViewUsingDataService(TDataItemService dataItemService)
        {
            DataItemService = dataItemService;
        }

        public virtual IDictionary<string, object> RetrieveAdditionalParametersForDataQuery(string data)
        {
            return new Dictionary<string, object>();
        }

        public virtual IEnumerable MapResultsToCustomData(IList<TBaseClass> data)
        {
            return data;
        }

        public virtual void PerformAdditionalProcessingOnDataRetrieval(string data, bool obtainingDataCountOnly)
        {
            
        }

        public override IEnumerable GetData(string data, int currentPage, int linesPerPage, string filter)
        {
            PerformAdditionalProcessingOnDataRetrieval(data, false);
            var results = DataItemService.RetrieveItemsWithFilter(currentPage, linesPerPage, filter, RetrieveAdditionalParametersForDataQuery(data));
            return MapResultsToCustomData(results);
        }

        public override int GetDataCount(string data, string filter)
        {
            PerformAdditionalProcessingOnDataRetrieval(data, true);
            return DataItemService.RetrieveItemCountWithFilter(filter, RetrieveAdditionalParametersForDataQuery(data));
        }
    }
}