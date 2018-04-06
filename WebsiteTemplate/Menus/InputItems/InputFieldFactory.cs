using System;

namespace WebsiteTemplate.Menus.InputItems
{
    public class InputFieldFactory
    {
        public static InputField CreateInputField(InputType inputType, string name, string label, string tabName, bool mandatory, object defaultValue = null)
        {
            switch (inputType)
            {
                case InputType.Boolean:
                    var defaultBool = Convert.ToBoolean(defaultValue);
                    return new BooleanInput(name, label, defaultBool, tabName, mandatory);
                case InputType.ComboBox:
                    return new ComboBoxInput(name, label, defaultValue, tabName, mandatory);
                case InputType.Date:
                    return new DateInput(name, label, defaultValue, tabName, mandatory);
                case InputType.File:
                    return new FileInput(name, label, defaultValue, tabName, mandatory);
                case InputType.Hidden:
                    return new HiddenInput(name, defaultValue, tabName, mandatory);
                case InputType.Label:
                    return new LabelInput(name, label, defaultValue?.ToString(), tabName);
                case InputType.Numeric:
                    return new NumericInput<float>(name, label, defaultValue, tabName, mandatory);
                case InputType.Password:
                    return new PasswordInput(name, label, defaultValue, tabName, mandatory);
                case InputType.Text:
                    return new StringInput(name, label, defaultValue, tabName, mandatory);
                default:
                    throw new Exception("Unknown input type in input field factory: " + inputType);
            }
        }
    }
}