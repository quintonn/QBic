using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class InputProcessingEvent : Event
    {
        public Dictionary<string, object> InputData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// This method assumes you know the return type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public T GetValue<T>(string propertyName)
        {
            var value = default(T);

            if (InputData.ContainsKey(propertyName))
            {
                value = (T)InputData[propertyName];
            }
            return value;
        }

        public string GetValue(string propertyName)
        {
            return GetValue<string>(propertyName);
        }
    }
}