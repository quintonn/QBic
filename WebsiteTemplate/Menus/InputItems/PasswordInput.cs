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

        public PasswordInput(string name, string label, object defaultValue = null)
            : base(name, label, defaultValue)
        {
        }
    }
}