using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class UserRoleService
    {
        private DataService DataService { get; set; }
        private EventService EventService { get; set; }

        public UserRoleService(DataService dataService, EventService eventService)
        {
            DataService = dataService;
            EventService = eventService;
        }

        public UserRole FindUserRoleByName(string name)
        {
            using (var session = DataService.OpenSession())
            {
                return session.CreateCriteria<UserRole>()
                              .Add(Restrictions.Eq("Name", name))
                              .UniqueResult<UserRole>();
            }
        }

        public void AddUserRole(string name, string description, List<string> events)
        {
            using (var session = DataService.OpenSession())
            {
                var dbUserRole = new UserRole()
                {
                    Name = name,
                    Description = description
                };

                DataService.SaveOrUpdate(dbUserRole);

                foreach (var item in events)
                {
                    var eventItem = new EventRoleAssociation()
                    {
                        Event = Convert.ToInt32(item),
                        UserRole = dbUserRole
                    };
                    DataService.SaveOrUpdate(eventItem);
                }

                session.Flush();
            }
        }

        public void UpdateUserRole(string id, string name, string description, List<string> events)
        {
            using (var session = DataService.OpenSession())
            {
                var dbUserRole = session.Get<UserRole>(id);
                dbUserRole.Name = name;
                dbUserRole.Description = description;
                DataService.SaveOrUpdate(dbUserRole);


                var existingEvents = session.CreateCriteria<EventRoleAssociation>()
                                   .CreateAlias("UserRole", "role")
                                   .Add(Restrictions.Eq("role.Id", id))
                                   .List<EventRoleAssociation>()
                                   .ToList();
                existingEvents.ForEach(e =>
                {
                    DataService.TryDelete(e);
                });
                foreach (var item in events)
                {
                    var eventItem = new EventRoleAssociation()
                    {
                        Event = Convert.ToInt32(item),
                        UserRole = dbUserRole
                    };
                    DataService.SaveOrUpdate(eventItem);
                }
                session.Flush();
            }
        }

        public Dictionary<string, object> GetListOfEvents()
        {
            var eventTypesEveryoneCanDo = new List<int>()
                {
                    EventNumber.CancelInputDialog,
                    EventNumber.DeleteInputViewItem,
                    EventNumber.ExecuteAction,
                    EventNumber.Nothing,
                    EventNumber.ShowMessage,
                    EventNumber.UpdateDataSourceComboBox,
                    EventNumber.UpdateInput,
                    EventNumber.UpdateInputView,
                    EventNumber.UserConfirmation,
                };

            var items = EventService.EventList.Where(e => e.Key != null && !eventTypesEveryoneCanDo.Contains(e.Key))
                                                .ToDictionary(e => e.Key, e => e.Value.Description)
                                                .OrderBy(e => e.Value)
                                                .ToDictionary(e => e.Key.ToString(), e => (object)e.Value);
            return items;
        }

        public bool UserRoleIsAssigned(string userRoleId)
        {
            using (var session = DataService.OpenSession())
            {
                var userRoleAssociations = session.CreateCriteria<UserRoleAssociation>()
                                                  .CreateAlias("UserRole", "role")
                                                  .Add(Restrictions.Eq("role.Id", userRoleId))
                                                  .List<UserRoleAssociation>()
                                                  .ToList();
                return userRoleAssociations.Count > 0;
            }
        }

        public void DeleteUserRole(string userRoleId)
        {
            using (var session = DataService.OpenSession())
            {
                var eventRoles = session.CreateCriteria<EventRoleAssociation>()
                                       .CreateAlias("UserRole", "role")
                                       .Add(Restrictions.Eq("role.Id", userRoleId))
                                       .List<EventRoleAssociation>()
                                       .ToList();
                eventRoles.ForEach(e =>
                {
                    DataService.TryDelete(e);
                });

                var userRole = session.Get<UserRole>(userRoleId);

                DataService.TryDelete(userRole);
                session.Flush();
            }
        }

        public UserRole RetrieveUserRole(string userRoleId)
        {
            using (var session = DataService.OpenSession())
            {
                return session.Get<UserRole>(userRoleId);
            }
        }

        public List<string> RetrieveEventRoleAssociationsForUserRole(string userRoleId)
        {
            var items = EventService.EventList.ToDictionary(e => e.Key, e => e.Value.Description)
                                                    .OrderBy(e => e.Value)
                                                    .ToDictionary(e => e.Key, e => (object)e.Value);

            using (var session = DataService.OpenSession())
            {
                var events = session.CreateCriteria<EventRoleAssociation>()
                                    .CreateAlias("UserRole", "role")
                                    .Add(Restrictions.Eq("role.Id", userRoleId))
                                    .List<EventRoleAssociation>()
                                    .Select(e => e.Event)
                                    .ToList();

                var results = items.Where(e => events.Contains(e.Key)).Select(i => i.Key.ToString()).ToList();
                return results;
            }
        }

        public List<UserRole> RetrieveUserRoles(int currentPage, int linesPerPage, string filter)
        {
            using (var session = DataService.OpenSession())
            {
                return CreateUserRoleQuery(session, filter)
                                     .Skip((currentPage - 1) * linesPerPage)
                                     .Take(linesPerPage)
                                     .List<UserRole>()
                                     .ToList();
            }
        }

        public int RetrieveUserRoleCount(string filter)
        {
            using (var session = DataService.OpenSession())
            {
                return CreateUserRoleQuery(session, filter).RowCount();
            }
        }

        private IQueryOver<UserRole> CreateUserRoleQuery(ISession session, string filter)
        {
            return session.QueryOver<UserRole>().Where(Restrictions.On<UserRole>(x => x.Name).IsInsensitiveLike(filter, MatchMode.Anywhere) ||
                                                       Restrictions.On<UserRole>(x => x.Description).IsInsensitiveLike(filter, MatchMode.Anywhere));
        }
    }
}