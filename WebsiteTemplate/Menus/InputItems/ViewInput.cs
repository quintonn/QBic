using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.InputItems
{
    /// <summary>
    /// A view input allows for a repeating list of inputs on an input screen.
    /// It is recommended that the defaultValue is sorted in a meaningful manner so that the data always appears in the same order
    /// </summary>
    /// <typeparam name="T">The type of data in the input view</typeparam>
    public class ViewInput<T> : InputField, IViewInput where T : IViewInputValue
    {
        public override InputType InputType
        {
            get
            {
                return InputType.View;
            }
        }

        public ViewForInput ViewForInput { get; set; }

        public ViewInput(string name, string label, ViewForInput viewForInput, List<IViewInputValue> defaultValue = null, string tabName = null, bool mandatory = false)
            : base(name, label, GetOrderedDefaults(defaultValue), tabName, mandatory)
        {
            ViewForInput = viewForInput;
        }

        // This could be all done inline but I want this to be explicit so it's not missed when reading the code
        private static List<IViewInputValue> GetOrderedDefaults(List<IViewInputValue> defaultValue)
        {
            if (defaultValue == null)
            {
                return new List<IViewInputValue>();
            }

            return defaultValue.OrderBy(x => x.rowId).ToList();
        }

        public override object GetValue(JToken jsonToken)
        {
            return ((jsonToken) as JArray)?.ToObject<List<T>>();
        }
    }
}