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
        CreateUser = 1011,
        EditUser = 1012,
        DoEditUser = 1013,
        ProcessEditUser = 1014,
        DeleteUser = 1020,
        SendConfirmationEmail = 2000,
        ShowMessage = 9000,
        UserConfirmation = 9500,
        ExecuteAction = 10000,
        CancelInputDialog = 0,
    }
}