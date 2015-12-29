namespace WebsiteTemplate.Menus.InputItems
{
    public class InputButton
    {
        public string Label { get; set; }

        public int ActionNumber { get; set; }

        public bool ValidateInput { get; set; }

        public InputButton(string label, int actionNumber, bool validateInput = true)
        {
            Label = label;
            ActionNumber = actionNumber;
            ValidateInput = validateInput;
        }
    }
}