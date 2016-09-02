﻿using System;
using WebsiteTemplate.Mappings;

namespace WebsiteTemplate.Backend.TestItem
{
    public class SuperCause : DynamicClass
    {
        public virtual string SuperName { get; set; }

        public virtual bool SuperOk { get; set; }

        public virtual DateTime? SuperDate { get; set; }
    }
}