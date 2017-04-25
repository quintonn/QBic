﻿using System;
using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.BasicCrudItems;

namespace WebsiteTemplate.Backend.TestItems
{
    public class CauseChildMenuItem : BasicCrudMenuItem<CauseChild>
    {
        public override string GetBaseItemName()
        {
            return "Cause Child";
        }

        public override EventNumber GetBaseMenuId()
        {
            return 131;
        }

        public override bool AllowInMenu
        {
            get
            {
                return false;// return true;
            }
        }

        public override Dictionary<string, string> GetColumnsToShowInView()
        {
            var res = new Dictionary<string, string>();
            res.Add("ChildName", "Name");
            res.Add("SomeInt", "Number");
            return res;
        }

        public override Dictionary<string, string> GetInputProperties()
        {
            var res = new Dictionary<string, string>();
            res.Add("ChildName", "Name");
            res.Add("SomeInt", "Number");
            return res;
        }
    }
}