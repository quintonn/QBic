namespace WebsiteTemplate.Menus.BaseItems
{
    public static class EventNumber
    {
        public const int Nothing = 0;
        public const int ViewUsers = 1000;
        public const int AddUser = 1010;
        public const int SendConfirmationEmail = 1011;
        public const int EditUser = 1012;
        public const int DeleteUser = 1020;
 
        public const int ViewUserRoleAssociations = 1100;
        public const int AddUserRoleAssociation = 1110;
        public const int DeleteUserRoleAssociation = 1120;
        
        public const int ViewMenus = 1200;
        public const int ModifyMenu = 1201;
        public const int AddMenu = 1210;
        
        public const int DeleteMenu = 1220;
        
        public const int ViewEventRoleAssociations = 1300;
        public const int AddEventRoleAssociation = 1310;
        public const int EditEventRoleAssociation = 1320;
        public const int DeleteEventRoleAssociation = 1330;
        
        public const int ViewUserEvents = 1400;
        
        public const int ViewUserRoles = 1500;
        public const int AddUserRole = 1510;
        public const int EditUserRole = 1520;
        public const int DeleteUserRole = 1530;
        
        public const int ShowMessage = 9000;
        public const int UserConfirmation = 9500;
        public const int ExecuteAction = 10000;
        public const int CancelInputDialog = 10001;
    }
}