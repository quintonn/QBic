using QCumber.Core.Models;
using System.Collections;
using System.Collections.Generic;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.ViewItems
{
    public abstract class ShowViewUsingInputProcessor<TItemProcessor, TBaseClass> : ShowView where TItemProcessor : InputProcessingCore<TBaseClass> where TBaseClass : BaseClass
    {
        protected TItemProcessor ItemProcessor { get; set; }

        public ShowViewUsingInputProcessor(TItemProcessor itemProcessor)
        {
            ItemProcessor = itemProcessor;
        }

        /// <summary>
        /// This method can be overridden to extract values from the user input and make it available to the <see cref="TItemProcessor" /> when the <see cref="TItemProcessor.RetrieveItemCountWithFilter" /> method is called.
        /// </summary>
        /// <param name="data">Data passed to the view from any previous activities.</param>
        /// <returns>Collection of key/value pairs with any parameters that needs to be extracted from the view.</returns>
        public virtual IDictionary<string, object> RetrieveAdditionalParametersForDataQuery(string data)
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Override this method in order to map the list of data elements from <see cref="TBaseClass" /> to any other class or dynamic type./>
        /// </summary>
        /// <param name="data">The data being returned by the <see cref="TItemProcessor" /></param>
        /// <returns>Custom enumerable to be used when displaying results. Values must match what is configured in the <see cref="ShowViewUsingDataService.ConfigureColumns"/> method.</returns>
        public virtual IEnumerable MapResultsToCustomData(IList<TBaseClass> data)
        {
            return data;
        }

        /// <summary>
        /// Override this method to perform any actions or data extractions prior to calling the <see cref="GetData"/> and <See cref="GetDataCount"/> methods./>
        /// </summary>
        /// <param name="data">Data passed to the view from any previous activities.</param>
        /// <param name="obtainingDataCountOnly"></param>
        public virtual void PerformAdditionalProcessingOnDataRetrieval(string data, bool obtainingDataCountOnly)
        {
            
        }

        /// <summary>
        /// This method returns the results from a datasource.
        /// This should take into consideration the <paramref name="linesPerPage"/> and <paramref name="currentPage"/>.
        /// </summary>
        /// <param name="data">Data passed to the view from any previous activities.</param>
        /// <param name="currentPage">The current page to display items for.</param>
        /// <param name="linesPerPage">The number of lines that can be displayed per page.</param>
        /// <param name="filter">A value provided by the user in an attempt the filter the results.</param>
        /// <returns></returns>
        public override IEnumerable GetData(GetDataSettings settings)
        {
            PerformAdditionalProcessingOnDataRetrieval(settings.ViewData, false);
            var results = ItemProcessor.RetrieveItemsWithFilter(settings, RetrieveAdditionalParametersForDataQuery(settings.ViewData));
            return MapResultsToCustomData(results);
        }

        /// <summary>
        /// This method should return the number of items
        /// </summary>
        /// <param name="data">Data passed to the view from any previous activities.</param>
        /// <param name="filter">A value provided by the user in an attempt the filter the results.</param>
        /// <returns></returns>
        public override int GetDataCount(GetDataSettings settings)
        {
            PerformAdditionalProcessingOnDataRetrieval(settings.ViewData, true);
            return ItemProcessor.RetrieveItemCountWithFilter(settings.Filter, RetrieveAdditionalParametersForDataQuery(settings.ViewData));
        }
    }
}