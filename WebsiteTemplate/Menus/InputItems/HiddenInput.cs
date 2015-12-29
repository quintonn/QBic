namespace WebsiteTemplate.Menus.InputItems
{
    public class HiddenInput : InputField
    {
        public HiddenInput(string name, object defaultValue = null, string tabName = null, bool mandatory = false)
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
    }
}