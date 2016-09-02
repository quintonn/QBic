using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.InputItems
{
    public class ListSelectionInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputType.ListSelection;
            }
        }

        [JsonProperty(Required = Required.Always), JsonConverter(typeof(DictionaryJsonConverter))]
        public Dictionary<string, object> ListSource { get; set; }

        public string SelectedItemsLabel { get; set; }

        public string AvailableItemsLabel { get; set; }

        public ListSelectionInput(string name, string label, object defaultValue = null, string tabName = null, bool mandatory = false)
            :base(name, label, defaultValue, tabName, mandatory)
        {
            ListSource = new Dictionary<string, object>();
        }

        public override object GetValue(JToken jsonToken)
        {
            var jArray = jsonToken as JArray;
            if (jArray == null)
            {
                return null;
            }
            return (jsonToken as JArray).Select(x => x.ToString()).ToList();
        }
    }

    public class DictionaryJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var result = objectType.IsAssignableFrom(typeof(Dictionary<string, object>));
            return result;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var json = jObject.ToString();

            var item = JsonHelper.DeserializeObject<Dictionary<string, object>>(json);
            return item;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            var items = value as Dictionary<string, object>;
            foreach (var entry in items)
            {
                var jsonItem = new
                {
                    Key = entry.Key,
                    Value = entry.Value
                };
                serializer.Serialize(writer, jsonItem);
            }
            writer.WriteEndArray();
        }
    }
}