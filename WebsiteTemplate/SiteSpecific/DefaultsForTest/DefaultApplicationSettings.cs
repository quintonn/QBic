using BasicAuthentication.Users;
using Microsoft.Practices.Unity;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Linq;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.SiteSpecific.DefaultsForTest
{
    public class DefaultApplicationSettings : IApplicationSettings
    {
        public string GetApplicationName()
        {
            return "Website Template";
        }

        public void RegisterUnityContainers(IUnityContainer container)
        {
            
        }

        public void SetupDefaults(ISession session)
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
                var result = CoreAuthenticationEngine.UserManager.CreateAsync(adminUser, "password");
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
                session.Save(adminRole);
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
                session.Save(adminRoleAssociation);
            }

            var menuList1 = session.CreateCriteria<Menu>()
                                   .Add(Restrictions.Eq("Event", EventNumber.ViewUsers))
                                   .List<Menu>();

            if (menuList1.Count == 0)
            {
                var menu1 = new Menu()
                {
                    Event = EventNumber.ViewUsers,
                    Name = "Users",
                };

                session.Save(menu1);
            }

            var menuList2 = session.CreateCriteria<Menu>()
                                   .Add(Restrictions.Eq("Event", EventNumber.ViewMenus))
                                   .List<Menu>();

            if (menuList2.Count == 0)
            {
                var menu2 = new Menu()
                {
                    Event = EventNumber.ViewMenus,
                    Name = "Menus",
                };
                session.Save(menu2);
            }

            var userRoleMenu = session.CreateCriteria<Menu>()
                                      .Add(Restrictions.Eq("Event", EventNumber.ViewUserRoles))
                                      .UniqueResult<Menu>();
            if (userRoleMenu == null)
            {
                userRoleMenu = new Menu()
                {
                    Event = EventNumber.ViewUserRoles,
                    Name = "User Roles"
                };
                session.Save(userRoleMenu);
            }

            var allEvents = MainController.EventList.Where(e => e.Value.ActionType != EventType.InputDataView).Select(e => Convert.ToInt32(e.Value.GetEventId()))
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
                    session.Delete(e);
                });
                session.Flush();
                foreach (var evt in allEvents)
                {
                    var era = new EventRoleAssociation()
                    {
                        Event = evt,
                        UserRole = adminRole
                    };
                    session.Save(era);
                }
            }


            //var testMenuList = session.CreateCriteria<Menu>()
            //                                  .Add(Restrictions.Eq("Name", "Test1"))
            //                                  .List<Menu>();
            //if (testMenuList.Count == 0)
            //{
            //    var testMenu = new Menu()
            //    {
            //        Name = "Test1",
            //    };
            //    session.Save(testMenu);

            //    var testMenuList2 = session.CreateCriteria<Menu>()
            //                       .Add(Restrictions.Eq("Name", "Test2"))
            //                       .List<Menu>();
            //    if (testMenuList2.Count == 0)
            //    {
            //        var testMenu2 = new Menu()
            //        {
            //            Name = "Test2",
            //            ParentMenu = testMenu,
            //        };
            //        session.Save(testMenu2);

            //        var menu3 = new Menu()
            //        {
            //            Name = "Test3",
            //            ParentMenu = testMenu2,
            //            Event = EventNumber.ViewMenus
            //        };

            //        session.Save(menu3);
            //    }
            //}
        }
    }
}