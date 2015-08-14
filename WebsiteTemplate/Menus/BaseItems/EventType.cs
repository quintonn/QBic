﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.Menus.BaseItems
{
    public enum EventType
    {
        DataView = 0,
        UserInput = 1,
        SubMenu = 2,
        DoSomething = 3,
        CancelInputDialog = 4,
        ShowMessage = 5,
        ExecuteAction = 6
    }
}