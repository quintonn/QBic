using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using NHibernate;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Menus.ViewItems.CoreItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class TestCoreView : CoreView<CauseChild>
    {
        public TestCoreView(DataService dataService, ILogger<TestCoreView> logger) : base(dataService, logger)
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
            columnConfig.AddStringColumn("Some Number", "SomeInt");
        }

        SuperCause cause = null;

        public override List<KeyValuePair<Expression<Func<CauseChild, object>>, Expression<Func<object>>>> GetAliases()
        {
            return new List<KeyValuePair<Expression<Func<CauseChild, object>>, Expression<Func<object>>>>()
            {
                new KeyValuePair<Expression<Func<CauseChild, object>>, Expression<Func<object>>>(x => x.SuperCause, () => cause)
            };
        }

        public override IEnumerable TransformData(IList<CauseChild> data, ISession session)
        {
            return data.Select(d => new
            {
                ChildName = d.ChildName,
                SuperName = d.SuperCause?.SuperName,
                SomeInt = d.SomeInt
            });
        }

        //public override IQueryOver<CauseChild> CreateQuery(ISession session, GetDataSettings settings, Expression<Func<CauseChild, bool>> additionalCriteria = null)
        //{
        //    var result = base.CreateQuery(session, settings, x => x.ChildName == "xxxxx");
        //    return result;
        //}

        public override List<Expression<Func<CauseChild, object>>> GetFilterItems()
        {
            return new List<Expression<Func<CauseChild, object>>>()
            {
                x => x.ChildName,
                x => cause.SuperName
            };
        }

        public override List<KeyValuePair<Type, Expression<Func<CauseChild, object>>>> GetNonStringFilterItems()
        {
            return new List<KeyValuePair<Type, Expression<Func<CauseChild, object>>>>()
            {
                new KeyValuePair<Type, Expression<Func<CauseChild, object>>>(typeof(int), x => x.SomeInt)
            };
        }

        public override EventNumber GetId()
        {
            return 8803;
        }
    }
}