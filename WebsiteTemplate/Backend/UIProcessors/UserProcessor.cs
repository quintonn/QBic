using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.UIProcessors
{
    public class UserProcessor : NHibernateDataItemService<User>
    {
        private UserService UserService { get; set; }

        public UserProcessor(DataService dataService, UserService userService)
            :base(dataService)
        {
            UserService = userService;
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

        public override async Task SaveOrUpdate(string itemId)
        {
            var message = String.Empty;
            using (var session = DataService.OpenSession())
            {
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
                        throw new Exception(String.Format("Unable to modify user. User with name {0} already exists.", userName));
                    }

                    UserService.UpdateUser(itemId, userName, email, userRoles);
                }
                else
                {
                    message = await UserService.CreateUser(session, userName, email, password, userRoles);
                }
                session.Flush();
            }

            if (!String.IsNullOrWhiteSpace(message))
            {
                throw new Exception(message);
            }
        }

        public override async Task UpdateItem(ISession session, User item)
        {
            
        }
    }
}