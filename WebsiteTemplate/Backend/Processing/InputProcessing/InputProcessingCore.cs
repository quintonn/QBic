using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Processing.InputProcessing
{
    public abstract class InputProcessingCore<T>  where T : BaseClass
    {
        protected DataService DataService { get; set; }

        internal IDictionary<string, object> InputData { get; set; }

        public InputProcessingCore(DataService dataService)
        {
            DataService = dataService;
            InputData = new Dictionary<string, object>();
        }

        public abstract T RetrieveExistingItem();

        public abstract T RetrieveItem(string itemId);

        public abstract int RetrieveItemCountWithFilter(string filter, IDictionary<string, object> additionalParameters);

        public abstract IList<T> RetrieveItemsWithFilter(int currentPage, int linesPerPage, string filter, IDictionary<string, object> additionalParameters);

        public abstract Task<ProcessingResult> SaveOrUpdate(string itemId);

        public abstract ProcessingResult DeleteItem(string itemId);

        public T GetValue<T>(string propertyName, T defaultValue = default(T))
        {
            return InputProcessingMethods.GetValue(InputData, propertyName, defaultValue);
        }

        public T GetDataSourceValue<T>(string propertyName)
        {
            return InputProcessingMethods.GetDataSourceValue<T>(InputData, DataService, propertyName);
        }

        public string GetValue(string propertyName)
        {
            return InputProcessingMethods.GetValue<string>(InputData, propertyName);
        }

        public DateTime? GetDateFromString(string dateString, DateTime? defaultValue = null)
        {
            return InputProcessingMethods.GetDateFromString(dateString, defaultValue);
        }

        public decimal GetDecimalValue(string decimalString, decimal defaultValue = 0)
        {
            return InputProcessingMethods.GetDecimalValue(decimalString, defaultValue);
        }
    }
}