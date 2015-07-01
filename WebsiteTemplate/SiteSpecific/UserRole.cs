﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.SiteSpecific
{
    public enum UserRole
    {
        //Admin = 0,
        ViewUsers = 0,
        SendConfirmationEmail = 1,
        AddUser = 2,

        AnyOne = 999, /// Any user can do this...
    }
}