using System;
using WebsiteTemplate.Models.NonDatabase;

namespace WebsiteTemplate.Menus.InputItems
{
    public class InputFieldFactory
    {
        public static InputField CreateInputField(SystemSettingItem setting, object defaultValue = null)
        {
            var name = setting.Key;
            var tabName = setting.TabNameOnInputScreen;
            var mandatory = setting.Mandatory;
            var label = setting.Description;
            switch (setting.InputType)
            {
                case InputType.Boolean:
                    var defaultBool = Convert.ToBoolean(defaultValue);
                    return new BooleanInput(name, label, defaultBool, tabName, mandatory);
                case InputType.ComboBox:
                    return new ComboBoxInput(name, label, defaultValue, tabName, mandatory)
                    {
                        ListItems = setting.ComboBoxListSource
                    };
                case InputType.Date:
                    return new DateInput(name, label, defaultValue as DateTime?, tabName, mandatory);
                case InputType.File:
                    return new FileInput(name, label, tabName, mandatory);
                case InputType.Hidden:
                    return new HiddenInput(name, defaultValue as string, tabName, mandatory);
                case InputType.Label:
                    return new LabelInput(name, label, defaultValue?.ToString(), tabName);
                case InputType.Numeric:
                    return new NumericInput<float>(name, label, defaultValue as float?, tabName, mandatory);
                case InputType.Password:
                    return new PasswordInput(name, label, defaultValue as string, tabName, mandatory);
                case InputType.Text:
                    return new StringInput(name, label, defaultValue as string, tabName, mandatory);
                default:
                    throw new Exception("Unknown input type in input field factory: " + setting.InputType);
            }
        }
    }
}