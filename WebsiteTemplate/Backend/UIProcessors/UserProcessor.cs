using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using System.Linq;
using WebsiteTemplate.Models;
using WebsiteTemplate.Backend.Users;

namespace WebsiteTemplate.Backend.UIProcessors
{
    public class UserProcessor : NHibernateDataItemService<User>
    {
        private UserService UserService { get; set; }
        private UserInjector Injector { get; set; }

        public UserProcessor(DataService dataService, UserService userService, UserInjector injector)
            :base(dataService)
        {
            UserService = userService;
            Injector = injector;
        }
        public override IQueryOver<User, User> CreateQueryForRetrieval(IQueryOver<User, User> query, string filter, IDictionary<string, object> additionalParameters)
        {
            return query.Where(Restrictions.On<User>(x => x.UserName).IsInsensitiveLike(filter, MatchMode.Anywhere) ||
                               Restrictions.On<User>(x => x.Email).IsInsensitiveLike(filter, MatchMode.Anywhere));
        }

        public override User RetrieveExistingItem(ISession session)
        {
            var userName = GetValue<string>("UserName");

            return UserService.FindUserByUserName(userName);
        }

        public override ProcessingResult PreDeleteActivities(ISession session, string itemId)
        {
            var userRoles = session.CreateCriteria<UserRoleAssociation>()
                                       .CreateAlias("User", "user")
                                       .Add(Restrictions.Eq("user.Id", itemId))
                                       .List<UserRoleAssociation>()
                                       .ToList();
            var auditEvents = session.CreateCriteria<AuditEvent>()
                                     .CreateAlias("User", "user")
                                     .Add(Restrictions.Eq("user.Id", itemId))
                                     .List<AuditEvent>()
                                     .ToList();
            auditEvents.ForEach(a =>
            {
                DataService.TryDelete(session, a);
            });
            userRoles.ForEach(u =>
            {
                DataService.TryDelete(session, u);
            });

            return Injector.DeleteItem(session, itemId);
        }

        public override async Task<ProcessingResult> SaveOrUpdate(string itemId)
        {
            var message = String.Empty;
            var email = GetValue<string>("Email");
            var userName = GetValue<string>("UserName");
            var password = GetValue<string>("Password");
            var confirmPassword = GetValue<string>("ConfirmPassword");

            var userRoles = GetValue<List<string>>("UserRoles");

            if (!String.IsNullOrWhiteSpace(itemId))
            {
                var existingUser = UserService.FindUserByUserName(userName);
                if (existingUser != null && existingUser.Id != itemId)
                {
                    return new ProcessingResult(false, String.Format("Unable to modify user. User with name {0} already exists.", userName));
                }

                UserService.UpdateUser(itemId, userName, email, userRoles);
            }
            else
            {
                message = await UserService.CreateUser(userName, email, password, userRoles);
            }

            var result = await Injector.SaveOrUpdateUser(InputData, userName);
            if (result.Success == false)
            {
                message = result.Message + "\n" + message;
                return new ProcessingResult(false, message);
            }

            return new ProcessingResult(true, message);
        }

        public override async Task<ProcessingResult> UpdateItem(User item)
        {
            return new ProcessingResult(true);
        }
    }
}