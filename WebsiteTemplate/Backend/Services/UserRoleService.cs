using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Data;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class UserRoleService
    {
        private DataStore DataStore { get; set; }

        public UserRoleService(DataStore store)
        {
            DataStore = store;
        }

        public UserRole FindUserRoleByName(string name)
        {
            using (var session = DataStore.OpenSession())
            {
                return session.CreateCriteria<UserRole>()
                              .Add(Restrictions.Eq("Name", name))
                              .UniqueResult<UserRole>();
            }
        }

        public void AddUserRole(string name, string description, List<string> events)
        {
            using (var session = DataStore.OpenSession())
            {
                var dbUserRole = new UserRole()
                {
                    Name = name,
                    Description = description
                };

                session.Save(dbUserRole);

                foreach (var item in events)
                {
                    var eventItem = new EventRoleAssociation()
                    {
                        Event = Convert.ToInt32(item),
                        UserRole = dbUserRole
                    };
                    session.Save(eventItem);
                }

                session.Flush();
            }
        }

        public void UpdateUserRole(string id, string name, string description, List<string> events)
        {
            using (var session = DataStore.OpenSession())
            {
                var dbUserRole = session.Get<UserRole>(id);
                dbUserRole.Name = name;
                dbUserRole.Description = description;
                session.Save(dbUserRole);


                var existingEvents = session.CreateCriteria<EventRoleAssociation>()
                                   .CreateAlias("UserRole", "role")
                                   .Add(Restrictions.Eq("role.Id", id))
                                   .List<EventRoleAssociation>()
                                   .ToList();
                existingEvents.ForEach(e =>
                {
                    session.Delete(e);
                });
                foreach (var item in events)
                {
                    var eventItem = new EventRoleAssociation()
                    {
                        Event = Convert.ToInt32(item),
                        UserRole = dbUserRole
                    };
                    session.Save(eventItem);
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


            var items = MainController.EventList.Where(e => e.Key != null && !eventTypesEveryoneCanDo.Contains(e.Key))
                                                .ToDictionary(e => e.Key, e => e.Value.Description)
                                                .OrderBy(e => e.Value)
                                                .ToDictionary(e => e.Key.ToString(), e => (object)e.Value);
            return items;
        }

        public bool UserRoleIsAssigned(string userRoleId)
        {
            using (var session = DataStore.OpenSession())
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
            using (var session = DataStore.OpenSession())
            {
                var eventRoles = session.CreateCriteria<EventRoleAssociation>()
                                       .CreateAlias("UserRole", "role")
                                       .Add(Restrictions.Eq("role.Id", userRoleId))
                                       .List<EventRoleAssociation>()
                                       .ToList();
                eventRoles.ForEach(e =>
                {
                    session.Delete(e);
                });

                var userRole = session.Get<UserRole>(userRoleId);

                session.Delete(userRole);
                session.Flush();
            }
        }

        public UserRole RetrieveUserRole(string userRoleId)
        {
            using (var session = DataStore.OpenSession())
            {
                return session.Get<UserRole>(userRoleId);

            }
        }

        public List<string> RetrieveEventRoleAssociationsForUserRole(string userRoleId)
        {
            var items = MainController.EventList.ToDictionary(e => e.Key, e => e.Value.Description)
                                                    .OrderBy(e => e.Value)
                                                    .ToDictionary(e => e.Key, e => (object)e.Value);

            using (var session = DataStore.OpenSession())
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
    }
}