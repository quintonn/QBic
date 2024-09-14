using Newtonsoft.Json.Linq;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.InputItems
{
    public class FileInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputType.File;
            }
        }
        public FileInput(string name, string label, string tabName = null, bool mandatory = false)
            : base(name, label, null, tabName, mandatory) // File can't have a default value
        {
        }

        public override object GetValue(JToken jsonToken)
        {
            if (jsonToken == null || jsonToken.ToString() == "[]")
            {
                return null;
            }
            var result = new FileInfo(JsonHelper.FromObject(jsonToken));
            return result;
        }
    }
}