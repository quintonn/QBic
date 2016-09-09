using System;
using System.Collections.Generic;
using System.Linq;

namespace WebsiteTemplate.Utilities
{
    public class JsonArray : List<JsonHelper>, IList<JsonHelper>
    {
        public JsonArray()
        {

        }

        public JsonArray(int capacity)
            : base(capacity)
        {

        }

        public JsonArray(IEnumerable<JsonHelper> collection)
            : base(collection)
        {

        }

        //public static JsonArray ParseStringList(string jsonString)
        //{
        //    var list = Newtonsoft.Json.Linq.JArray.Parse(jsonString).ToList();
        //    return new JsonArray(list);
        //}

        public static JsonArray FromObject(object collection)
        {
            try
            {
                var tmp = collection as System.Collections.IEnumerable;
                var list = tmp.Cast<object>().ToList();
                var items = list.Select(l => JsonHelper.FromObject(l));
                return new JsonArray(items);
            }
            catch (Exception e)
            {
                return new JsonArray();
            }
        }
    }
}