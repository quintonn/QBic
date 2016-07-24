using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public ComboBoxInput(string name, string label, object defaultValue = null, string tabName = null, bool mandatory = false)
            : base(name, label, defaultValue, tabName, mandatory)
        {
            ListItems = new Dictionary<string, object>();
        }

        [JsonProperty(Required = Required.Always), JsonConverter(typeof(DictionaryJsonConverter))]
        public Dictionary<string, object> ListItems { get; set; }

        public override object GetValue(JToken jsonToken)
        {
            return jsonToken?.ToString();
        }
    }
}