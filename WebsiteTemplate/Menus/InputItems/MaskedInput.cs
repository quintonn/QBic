using Newtonsoft.Json.Linq;

namespace WebsiteTemplate.Menus.InputItems
{
    public class MaskedInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputType.MaskedInput;
            }
        }

        public string InputMask { get; set; }

        public MaskedInput(string name, string label, string inputMask, object defaultValue = null, string tabName = null, bool mandatory = false)
            : base(name, label, defaultValue, tabName, mandatory)
        {
            InputMask = inputMask;
        }

        public override object GetValue(JToken jsonToken)
        {
            return jsonToken?.ToString();
        }
    }
}