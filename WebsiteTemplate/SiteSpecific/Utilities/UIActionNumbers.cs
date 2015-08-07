﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.SiteSpecific.Utilities
{
    public class UIActionNumbers
    {
        public static readonly int VIEW_USERS = 1000;

        public static readonly int ADD_USER = 1010;
        public static readonly int CREATE_USER = 1011;
        public static readonly int EDIT_USER = 1012;
        public static readonly int DO_EDIT_USER = 1013;
        public static readonly int PROCESS_EDIT_USER = 1014;

        public static readonly int DELETE_USER = 1020;

        public static readonly int SEND_CONFIRMATION_EMAIL = 2000;

        public static readonly int SHOW_MESSAGE = 9000;

        public static readonly int USER_CONFIRMATION = 9500;

        public static readonly int EXECUTE_ACTION = 10000;

        public static readonly int CANCEL_INPUT_DIALOG = 0;
    }
}