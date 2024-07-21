using Newtonsoft.Json.Linq;

namespace WebsiteTemplate.Menus.InputItems
{
    //TODO: We could just have a 'hidden' field on other inputs and that way we can have input type logic.
    //      Although, leaving it up to the user to convert to string solves many potential issues for us
    public class HiddenInput : InputField
    {
        public HiddenInput(string name, string defaultValue = null, string tabName = null, bool mandatory = false)
            : base(name, "", defaultValue, tabName, mandatory)
        {

        }

        public override InputType InputType
        {
            get
            {
                return InputItems.InputType.Hidden;
            }
        }

        public override object GetValue(JToken jsonToken)
        {
            return jsonToken?.ToString();
        }
    }
}