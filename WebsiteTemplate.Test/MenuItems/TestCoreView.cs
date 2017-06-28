using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus.ViewItems.CoreItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class TestCoreView : CoreView<CauseChild>
    {
        public TestCoreView(DataService dataService) : base(dataService)
        {
        }
        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }
        public override string Description
        {
            get
            {
                return "AAAA";
            }
        }

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("ChildName", "ChildName");
            columnConfig.AddStringColumn("SuperName", "SuperName");
        }

        SuperCause cause = null;

        public override List<KeyValuePair<Expression<Func<CauseChild, object>>, Expression<Func<object>>>> GetAliases()
        {
            return new List<KeyValuePair<Expression<Func<CauseChild, object>>, Expression<Func<object>>>>()
            {
                new KeyValuePair<Expression<Func<CauseChild, object>>, Expression<Func<object>>>(x => x.SuperCause, () => cause)
            };
        }

        public override IEnumerable TransformData(IList<CauseChild> data)
        {
            return data.Select(d => new
            {
                ChildName = d.ChildName,
                SuperName = d.SuperCause?.SuperName
            });
        }

        public override List<Expression<Func<CauseChild, object>>> GetFilterItems()
        {
            return new List<Expression<Func<CauseChild, object>>>()
            {
                x => x.ChildName,
                x => cause.SuperName
            };
        }

        public override EventNumber GetId()
        {
            return 8803;
        }
    }
}