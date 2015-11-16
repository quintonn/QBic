namespace WebsiteTemplate.Menus.InputItems
{
    public class StringInput : InputField
    {
        public override InputType InputType
        {
            get
            {
                return InputItems.InputType.Text;
            }
        }

        public StringInput(string name, string label, object defaultValue = null)
            : base(name, label, defaultValue)
        {
        }
    }
}