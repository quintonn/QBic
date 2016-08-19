using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Menus.PropertyChangedEvents
{
    public class UpdateDataSourceComboBoxSource : Event
    {
        public string InputName { get; set; }

        [JsonProperty(Required = Required.Always), JsonConverter(typeof(DictionaryJsonConverter))]
        public Dictionary<string, object> ListItems { get; set; }

        public override string Description
        {
            get
            {
                return "Update data source combo box source";
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.UpdateDataSourceComboBox;
            }
        }

        public UpdateDataSourceComboBoxSource()
        {

        }

        public UpdateDataSourceComboBoxSource(string inputName, Dictionary<string, object> listItems)
        {
            InputName = inputName;
            ListItems = listItems;
        }

        public override EventNumber GetId()
        {
            return EventNumber.UpdateDataSourceComboBox;
        }
    }
}