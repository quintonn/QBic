using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.Users;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Test.Models;

namespace WebsiteTemplate.Test.MenuItems.Users
{
    public class TestUserInjector : UserInjector
    {
        public TestUserInjector(DataService dataService) : base(dataService)
        {
            Console.WriteLine("X");
        }

        public override ProcessingResult DeleteItem(ISession session, string itemId)
        {
            var dbUser = session.Get<User>(itemId);

            var userInfo = session.QueryOver<UserExtraInfo>().Where(u => u.User.Id == itemId).List();
            foreach (var item in userInfo)
            {
                DataService.TryDelete(session, item);
            }

            return new ProcessingResult(true);
        }

        public override IList<InputField> GetInputFields(User user)
        {
            var result = new List<InputField>();

            UserExtraInfo userExtraInfo = null;

            using (var session = DataService.OpenSession())
            {
                if (user != null && !String.IsNullOrWhiteSpace(user.Id))
                {
                    userExtraInfo = session.QueryOver<UserExtraInfo>().Where(u => u.User.Id == user.Id).SingleOrDefault();
                    result.Add(new StringInput("ExtraCode", "Extra Code", userExtraInfo?.ExtraCode, "Extra", false));
                }

                result.Add(new EnumComboBoxInput<TestUserRole>("OdysseyUserRole", "School User Role", defaultValue: userExtraInfo?.UserRole.ToString(), tabName: "Extra")
                {
                    Mandatory = true
                });

                var subjectInput = new StringInput("Subjects", "Subject", "", "Extra", true)
                {
                    VisibilityConditions = new List<Condition>()
                    {
                        new Condition("OdysseyUserRole", Comparison.Equals, TestUserRole.Teacher.ToString()),
                    },
                };
                result.Add(subjectInput);

                var classInput = new StringInput("Classes", "Class", "", "Extra", true)
                {
                    VisibilityConditions = new List<Condition>()
                    {
                        new Condition("OdysseyUserRole", Comparison.Equals, TestUserRole.HeadOfGrade.ToString()),
                    },
                };
                result.Add(classInput);
            }

            return result;
        }

        public override async Task<ProcessingResult> SaveOrUpdate(ISession session, string username)
        {
            var extraCode = GetValue("ExtraCode");
            if (string.IsNullOrWhiteSpace(extraCode))
            {
                return new ProcessingResult(false, "Extra code is mandatory");
            }

            var dbUser = session.QueryOver<User>().Where(x => x.UserName == username).SingleOrDefault();

            var userInfo = session.QueryOver<UserExtraInfo>().Where(x => x.User.Id == dbUser.Id).SingleOrDefault();

            if (userInfo == null)
            {
                userInfo = new UserExtraInfo()
                {
                    User = dbUser
                };
            }

            userInfo.ExtraCode = extraCode;

            DataService.SaveOrUpdate(session, userInfo);

            return new ProcessingResult(true);
        }
    }
}