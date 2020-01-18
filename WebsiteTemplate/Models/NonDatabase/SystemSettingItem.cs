using System.Collections.Generic;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Models.NonDatabase
{
    public class SystemSettingItem
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public object DefaultValue { get; set; }
        public InputType InputType { get; set; }
        public string TabNameOnInputScreen { get; set; }
        public bool Mandatory { get; set; }
        public Dictionary<string, object> ComboBoxListSource { get; set; }

        public SystemSettingItem(string key, string description, InputType inputType, bool mandatory, string tabNameOnInputScreen = "Other Settings", object defaultValue = null)
        {
            Key = key;
            Description = description;
            InputType = inputType;
            TabNameOnInputScreen = tabNameOnInputScreen;
            DefaultValue = defaultValue;
            Mandatory = mandatory;
            ComboBoxListSource = new Dictionary<string, object>();
        }
    }
}