using Newtonsoft.Json;

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
        public static EventNumber Logout = new EventNumber(10);
        public static EventNumber ViewUsers = new EventNumber(1000);
        public static EventNumber AddUser = new EventNumber(1001);
        public static EventNumber SendConfirmationEmail = new EventNumber(1002);
        public static EventNumber EditUser = new EventNumber(1003);
        public static EventNumber DeleteUser = new EventNumber(1004);

        public static EventNumber ViewMenus = new EventNumber(1007);
        public static EventNumber AddMenu = new EventNumber(1008);
        public static EventNumber EditMenu = new EventNumber(1009);
        public static EventNumber IncrementMenuOrder = new EventNumber(901);
        public static EventNumber DecrementMenuOrder = new EventNumber(902);
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
        public static EventNumber UpdateInputVisibility = new EventNumber(1604);

        public static EventNumber AuditReportFilter = new EventNumber(1610);
        public static EventNumber CreateAuditReport = new EventNumber(1611);

        public static EventNumber ModifySystemSettings = new EventNumber(1620);

        public static EventNumber CreateBackup = new EventNumber(1700);
        public static EventNumber RestoreBackup = new EventNumber(1701);

        public static EventNumber ResetPassword = new EventNumber(1710);

        public static EventNumber KeepAliveBackgroundTask = new EventNumber(1720);
        public static EventNumber ViewBackgroundErrors = new EventNumber(1721);
        public static EventNumber ClearBackgroundErrors = new EventNumber(1722);
        public static EventNumber ViewBackgroundStatusInfo = new EventNumber(1723);
        public static EventNumber ViewBackgroundDetail = new EventNumber(1724);
        public static EventNumber ClearBackgroundStatusInfo = new EventNumber(1725);

        public static EventNumber ViewSystemLog = new EventNumber(1730);
        public static EventNumber ClearSystemLog = new EventNumber(1731);

        public static EventNumber CsvUploadColumnsView = new EventNumber(1740);
        public static EventNumber EditCsvColumnMapping = new EventNumber(1741);
        public static EventNumber ShowCsvProcessResult = new EventNumber(1742);

        public static implicit operator EventNumber(int value)
        {
            return new EventNumber(value);
        }

        public static implicit operator int(EventNumber eventNumber)
        {
            if (eventNumber == null)
            {
                return 0;
            }
            return eventNumber.Value;
        }

        public static implicit operator int?(EventNumber eventNumber)
        {
            if (eventNumber == null)
            {
                return null;
            }
            return eventNumber.Value;
        }
    }
}