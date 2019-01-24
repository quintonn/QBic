using Newtonsoft.Json;
using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using Unity;

namespace WebsiteTemplate.Menus.PropertyChangedEvents
{
    public class UpdateComboBoxSource : Event
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
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public override EventType ActionType
        {
            get
            {
                return EventType.UpdateComboBox;
            }
        }

        [InjectionConstructor]
        public UpdateComboBoxSource()
        {

        }

        public UpdateComboBoxSource(string inputName, Dictionary<string, object> listItems)
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