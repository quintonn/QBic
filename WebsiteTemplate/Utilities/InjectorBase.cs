using System;
using System.Collections.Generic;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Utilities
{
    public abstract class InjectorBase
    {
        protected DataService DataService { get; set; }
        internal IDictionary<string, object> InputData { get; set; }

        public InjectorBase(DataService dataService)
        {
            DataService = dataService;
        }

        protected T GetValue<T>(string propertyName, T defaultValue = default(T))
        {
            return InputProcessingMethods.GetValue(InputData, propertyName, defaultValue);
        }

        protected T GetDataSourceValue<T>(string propertyName)
        {
            return InputProcessingMethods.GetDataSourceValue<T>(InputData, DataService, propertyName);
        }

        protected string GetValue(string propertyName)
        {
            return InputProcessingMethods.GetValue<string>(InputData, propertyName);
        }

        protected DateTime? GetDateFromString(string dateString, DateTime? defaultValue = null)
        {
            return InputProcessingMethods.GetDateFromString(dateString, defaultValue);
        }

        protected decimal GetDecimalValue(string decimalString, decimal defaultValue = 0)
        {
            return InputProcessingMethods.GetDecimalValue(decimalString, defaultValue);
        }
    }
}