namespace WebsiteTemplate.Menus.BaseItems
{
    public enum EventNumber
    {
        Nothing = 0,
        ViewUsers = 1000,
        AddUser = 1010,
        SendConfirmationEmail = 1011,
        EditUser = 1012,
        DeleteUser = 1020,
        
        ViewUserRoleAssociations = 1100,
        AddUserRoleAssociation = 1110,
        DeleteUserRoleAssociation = 1120,

        ViewMenus = 1200,
        AddMenu = 1210,
        EditMenu = 1212,
        DeleteMenu = 1220,
        View_ViewMenus = 1230,

        ViewEventRoleAssociation = 1300,
        AddEventRoleAssociation = 1310,
        EditEventRoleAssociation = 1320,
        DeleteEventRoleAssociation = 1330,


        ShowMessage = 9000,
        UserConfirmation = 9500,
        ExecuteAction = 10000,
        CancelInputDialog = 10001,
    }
}