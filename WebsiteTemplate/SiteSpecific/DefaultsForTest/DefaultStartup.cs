using Unity;
using NHibernate.Criterion;
using System;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.SiteSpecific.DefaultsForTest
{
    public class DefaultStartup : ApplicationStartup
    {
        private DefaultUserManager UserManager { get; set;  }

        public DefaultStartup(DataService dataService, DefaultUserManager userManager)
            : base(dataService)
        {
            UserManager = userManager;
        }

        public override void RegisterUnityContainers(IUnityContainer container)
        {
            //UserManager = container.Resolve<DefaultUserManager>();
        }

        public override void SetupDefaults()
        {
            using (var session = DataService.OpenSession())
            {
                var adminUser = session.CreateCriteria<User>()
                                                   .Add(Restrictions.Eq("UserName", "Admin"))
                                                   .UniqueResult<User>();
                if (adminUser == null)
                {
                    adminUser = new User(false)
                    {
                        Email = "q10athome@gmail.com",
                        EmailConfirmed = true,
                        UserName = "Admin",
                    };
                    
                    var result = UserManager.CreateAsync(adminUser, "password");
                    result.Wait();

                    if (!result.Result.Succeeded)
                    {
                        throw new Exception("Unable to create user: " + "Admin");
                    }
                }


                var adminRole = session.CreateCriteria<UserRole>()
                                       .Add(Restrictions.Eq("Name", "Admin"))
                                       .UniqueResult<UserRole>();
                if (adminRole == null)
                {
                    adminRole = new UserRole()
                    {
                        Name = "Admin",
                        Description = "Administrator"
                    };
                    DataService.SaveOrUpdate(session, adminRole);
                }

                var adminRoleAssociation = session.CreateCriteria<UserRoleAssociation>()
                                                  .CreateAlias("User", "user")
                                                  .CreateAlias("UserRole", "role")
                                                  .Add(Restrictions.Eq("user.Id", adminUser.Id))
                                                  .Add(Restrictions.Eq("role.Id", adminRole.Id))
                                                  .UniqueResult<UserRoleAssociation>();
                if (adminRoleAssociation == null)
                {
                    adminRoleAssociation = new UserRoleAssociation()
                    {
                        User = adminUser,
                        UserRole = adminRole
                    };
                    DataService.SaveOrUpdate(session, adminRoleAssociation);
                }

                var menuList1 = session.CreateCriteria<Menu>()
                                       .Add(Restrictions.Eq("Event", (int)EventNumber.ViewUsers))
                                       .List<Menu>();

                if (menuList1.Count == 0)
                {
                    var menu1 = new Menu()
                    {
                        Event = EventNumber.ViewUsers,
                        Name = "Users",
                        Position = 0
                    };

                    DataService.SaveOrUpdate(session, menu1);
                }

                var menuList2 = session.CreateCriteria<Menu>()
                                       .Add(Restrictions.Eq("Event", (int)EventNumber.ViewMenus))
                                       .List<Menu>();

                if (menuList2.Count == 0)
                {
                    var menu2 = new Menu()
                    {
                        Event = EventNumber.ViewMenus,
                        Name = "Menus",
                        Position = 1
                    };
                    DataService.SaveOrUpdate(session, menu2);
                }

                var userRoleMenu = session.CreateCriteria<Menu>()
                                          .Add(Restrictions.Eq("Event", (int)EventNumber.ViewUserRoles))
                                          .Add(Restrictions.IsNull("ParentMenu"))
                                          .UniqueResult<Menu>();
                if (userRoleMenu == null)
                {
                    userRoleMenu = new Menu()
                    {
                        Event = EventNumber.ViewUserRoles,
                        Name = "User Roles",
                        Position = 2
                    };
                    DataService.SaveOrUpdate(session, userRoleMenu);
                }

                var allEvents = EventService.EventMenuList.Where(e => e.Value.ActionType != EventType.InputDataView).Select(e => e.Value.GetEventId())
                                      .ToList();

                var eras = session.CreateCriteria<EventRoleAssociation>()
                                  .CreateAlias("UserRole", "role")
                                  .Add(Restrictions.Eq("role.Id", adminRole.Id))
                                  .List<EventRoleAssociation>()
                                  .ToList();

                if (eras.Count != allEvents.Count)
                {
                    eras.ForEach(e =>
                    {
                        DataService.TryDelete(session, e);
                    });
                    session.Flush();
                    foreach (var evt in allEvents)
                    {
                        var era = new EventRoleAssociation()
                        {
                            Event = evt,
                            UserRole = adminRole
                        };
                        DataService.SaveOrUpdate(session, era);
                    }
                }
                session.Flush();
            }
        }
    }
}