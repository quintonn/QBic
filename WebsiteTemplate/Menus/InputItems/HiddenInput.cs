namespace WebsiteTemplate.Menus.InputItems
{
    public class HiddenInput : InputField
    {
        public HiddenInput(string name, object defaultValue = null)
            : base(name, "", defaultValue)
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