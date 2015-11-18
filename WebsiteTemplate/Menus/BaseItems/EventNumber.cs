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
        DeleteMenu = 1220,

        ShowMessage = 9000,
        UserConfirmation = 9500,
        ExecuteAction = 10000,
        CancelInputDialog = 10001,
    }
}