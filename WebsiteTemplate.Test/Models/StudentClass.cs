﻿using QBic.Core.Models;

namespace WebsiteTemplate.Test.Models
{
    public class StudentClass : DynamicClass
    {
        public virtual string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}