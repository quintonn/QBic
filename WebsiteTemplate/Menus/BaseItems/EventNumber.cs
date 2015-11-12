using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.BaseItems
{
    public enum EventNumber
    {
        Nothing = -1,
        ViewUsers = 1000,
        AddUser = 1010,
        SendConfirmationEmail = 1011,
        EditUser = 1012,
        DeleteUser = 1020,
        
        ViewUserRoleAssociations = 1100,
        AddUserRoleAssociation = 1110,
        DeleteUserRoleAssociation = 1120,

        ShowMessage = 9000,
        UserConfirmation = 9500,
        ExecuteAction = 10000,
        CancelInputDialog = 0,
    }
}