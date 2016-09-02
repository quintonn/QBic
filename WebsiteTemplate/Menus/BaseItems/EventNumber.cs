﻿using Newtonsoft.Json;

namespace WebsiteTemplate.Menus.BaseItems
{
    [JsonConverter(typeof(EventNumberConverter))]
    public class EventNumber
    {
        private int Value { get; set; }

        public EventNumber(int value)
        {
            Value = value;
        }

        public static EventNumber Nothing = new EventNumber(0);
        public static EventNumber ViewUsers = new EventNumber(1000);
        public static EventNumber AddUser = new EventNumber(1001);
        public static EventNumber SendConfirmationEmail = new EventNumber(1002);
        public static EventNumber EditUser = new EventNumber(1003);
        public static EventNumber DeleteUser = new EventNumber(1004);

        public static EventNumber ViewMenus = new EventNumber(1007);
        public static EventNumber AddMenu = new EventNumber(1008);
        public static EventNumber EditMenu = new EventNumber(1009);

        public static EventNumber DeleteMenu = new EventNumber(1010);

        public static EventNumber ViewUserRoles = new EventNumber(1011);
        public static EventNumber AddUserRole = new EventNumber(1012);
        public static EventNumber EditUserRole = new EventNumber(1013);
        public static EventNumber DeleteUserRole = new EventNumber(1014);

        public static EventNumber ShowMessage = new EventNumber(1100);
        public static EventNumber UserConfirmation = new EventNumber(1101);

        public static EventNumber ExecuteAction = new EventNumber(1200);
        public static EventNumber CancelInputDialog = new EventNumber(1500);

        public static EventNumber UpdateInputView = new EventNumber(1600);
        public static EventNumber DeleteInputViewItem = new EventNumber(1601);
        public static EventNumber UpdateDataSourceComboBox = new EventNumber(1602);
        public static EventNumber UpdateInput = new EventNumber(1603);

        public static implicit operator EventNumber(int value)
        {
            return new EventNumber(value);
        }

        public static implicit operator int(EventNumber eventNumber)
        {
            return eventNumber.Value;
        }
    }
}