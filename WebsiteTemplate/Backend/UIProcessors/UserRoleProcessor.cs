using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.UIProcessors
{
    public class UserRoleProcessor : NHibernateDataItemService<UserRole>
    {
        private UserRoleService UserRoleService { get; set; }

        public UserRoleProcessor(DataService dataService, UserRoleService userRoleService)
            : base(dataService)
        {
            UserRoleService = userRoleService;
        }
        public override IQueryOver<UserRole, UserRole> CreateQueryForRetrieval(IQueryOver<UserRole, UserRole> query, string filter, IDictionary<string, object> additionalParameters)
        {
            return query.Where(Restrictions.On<UserRole>(x => x.Name).IsInsensitiveLike(filter, MatchMode.Anywhere) ||
                               Restrictions.On<UserRole>(x => x.Description).IsInsensitiveLike(filter, MatchMode.Anywhere));
        }

        public override UserRole RetrieveExistingItem(ISession session)
        {
            var name = GetValue<string>("Name");

            return UserRoleService.FindUserRoleByName(name);
        }

        public override async Task<ProcessingResult> UpdateItem(UserRole item)
        {
            return new ProcessingResult(true);
        }

        public override ProcessingResult PreDeleteActivities(ISession session, string itemId)
        {
            var isAssigned = UserRoleService.UserRoleIsAssigned(itemId);
            if (isAssigned)
            {
                return new ProcessingResult(false, "Cannot delete user role, it is assigned to users.");
            }

            var eventRoles = session.CreateCriteria<EventRoleAssociation>()
                                       .CreateAlias("UserRole", "role")
                                       .Add(Restrictions.Eq("role.Id", itemId))
                                       .List<EventRoleAssociation>()
                                       .ToList();
            eventRoles.ForEach(e =>
            {
                DataService.TryDelete(session, e);
            });

            return new ProcessingResult(true);
        }

        public override async Task<ProcessingResult> SaveOrUpdate(string itemId)
        {
            var name = GetValue<string>("Name");
            var description = GetValue<string>("Description");

            var events = GetValue<List<string>>("Events");

            if (String.IsNullOrWhiteSpace(itemId))
            {
                UserRoleService.AddUserRole(name, description, events);
            }
            else {
                UserRoleService.UpdateUserRole(itemId, name, description, events);
            }

            return new ProcessingResult(true);
        }
    }
}