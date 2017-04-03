using Newtonsoft.Json.Linq;

namespace WebsiteTemplate.Menus.InputItems
{
    public class LabelInput : InputField
    {
        public LabelInput(string name, string label, string tabName = null)
            : base(name, label, label, tabName, false)
        {
        }

        public override InputType InputType
        {
            get
            {
                return InputType.Label;
            }
        }

        public override object GetValue(JToken jsonToken)
        {
            return jsonToken?.ToString();
        }
    }
}