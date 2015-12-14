using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebsiteTemplate.Menus.InputItems
{
    public class ComboBoxInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputItems.InputType.ComboBox;
            }
        }

        public ComboBoxInput(string name, string label, object defaultValue = null)
            : base(name, label, defaultValue)
        {
            ListItems = new Dictionary<string, object>();
        }

        [JsonProperty(Required = Required.Always), JsonConverter(typeof(DictionaryJsonConverter))]
        public Dictionary<string, object> ListItems { get; set; }
    }
}