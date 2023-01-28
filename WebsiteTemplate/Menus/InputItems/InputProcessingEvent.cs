using System;
using System.Collections.Generic;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class InputProcessingEvent : Event
    {
        protected DataService DataService { get; set; }

        protected InputProcessingEvent(DataService dataService)
        {
            DataService = dataService;
        }

        public Dictionary<string, object> InputData { get; set; } = new Dictionary<string, object>();

        public DateTime? GetDateFromString(string dateString, DateTime? defaultValue = null)
        {
            return InputProcessingMethods.GetDateFromString(dateString, defaultValue);
        }

        public decimal GetDecimalValue(string decimalString, decimal defaultValue = 0)
        {
            return InputProcessingMethods.GetDecimalValue(decimalString, defaultValue);
        }

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
    }
}