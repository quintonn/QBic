using Newtonsoft.Json.Linq;

namespace WebsiteTemplate.Menus.InputItems
{
    public class DateInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputType.Date;
            }
        }

        public DateInput(string name, string label, object defaultValue = null, string tabName = null, bool mandatory = false)
            : base(name, label, defaultValue, tabName, mandatory)
        {
        }

        public override object GetValue(JToken jsonToken)
        {
            return jsonToken?.ToString();
        }
    }
}