using Newtonsoft.Json.Linq;

namespace WebsiteTemplate.Menus.InputItems
{
    public class PasswordInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputItems.InputType.Password;
            }
        }

        public PasswordInput(string name, string label, string defaultValue = null, string tabName = null, bool mandatory = false)
            : base(name, label, defaultValue, tabName, mandatory)
        {
        }

        public override object GetValue(JToken jsonToken)
        {
            return jsonToken?.ToString();
        }
    }
}