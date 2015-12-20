namespace WebsiteTemplate.Menus.InputItems
{
    public class HiddenInput : InputField
    {
        public HiddenInput(string name, object defaultValue = null, string tabName = null)
            : base(name, "", defaultValue, tabName)
        {

        }

        public override InputType InputType
        {
            get
            {
                return InputItems.InputType.Hidden;
            }
        }
    }
}