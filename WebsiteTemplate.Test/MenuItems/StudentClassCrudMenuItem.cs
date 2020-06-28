using NHibernate;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.BasicCrudItems;
using WebsiteTemplate.Test.Models;

namespace WebsiteTemplate.Test.MenuItems
{
    public class StudentClassCrudMenuItem : BasicCrudMenuItem<StudentClass>
    {
        private DataService DataService { get; set; }

        public StudentClassCrudMenuItem(DataService dataService)
        {
            DataService = dataService;
        }
        public override bool AllowInMenu => true;

        public override string UniquePropertyName => "Name"; // Will prevent duplicate items using Description as check

        public override string GetBaseItemName()
        {
            return "Student Class";
        }

        public override EventNumber GetBaseMenuId()
        {
            return 56332;
        }

        public override Dictionary<string, string> GetColumnsToShowInView()
        {
            return new Dictionary<string, string>()
            {
                { "Name", "Name" },
            };
        }

        public override Dictionary<string, string> GetInputProperties()
        {
            return new Dictionary<string, string>()
            {
                { "Name", "Name" },
            };
        }

        public override void OnModify(StudentClass item, bool isNew)
        {
            base.OnModify(item, isNew);
        }

        public override IQueryOver<StudentClass> OrderQuery(IQueryOver<StudentClass, StudentClass> query)
        {
            using (var session = DataService.OpenSession())
            {
                var aClass = session.QueryOver<StudentClass>().Take(1).List().FirstOrDefault();
                if (aClass != null)
                {
                    var aStudent = session.QueryOver<Student>().Where(x => x.Class.Id == aClass.Id).Take(1).List().FirstOrDefault();
                    if (aStudent == null)
                    {
                        aStudent = new Student()
                        {
                            Name = "John",
                            Class = aClass
                        };
                        session.SaveOrUpdate(aStudent);
                        session.Flush();
                    }
                }
            }

            return base.OrderQuery(query);
        }
    }
}