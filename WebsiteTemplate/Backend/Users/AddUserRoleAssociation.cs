﻿using Newtonsoft.Json;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;


namespace WebsiteTemplate.Backend.Users
{
    public class AddUserRoleAssociation : GetInput
    {
        public override EventNumber GetId()
        {
            return EventNumber.AddUserRoleAssociation;
        }

        private string mDescription { get; set; }
        public override string Description
        {
            get
            {
                return mDescription;
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                var comboBoxInput = new ComboBoxInput("UserRole", "User Role");
                comboBoxInput.ListItems = ListItems;
                list.Add(comboBoxInput);

                list.Add(new HiddenInput("UserId", UserId));
                
                return list;
            }
        }

        private List<string> ListItems { get; set; }
        private string UserId { get; set; }

        public override IList<InputButton> InputButtons
        {
            get
            {
                return new List<InputButton>()
                {
                    new InputButton("Submit", 0),
                    new InputButton("Cancel", 1)
                };
            }
        }

        public override System.Threading.Tasks.Task<InitializeResult> Initialize(string data)
        {
            ListItems = new List<string>();

            UserId = data;

            using (var session = Store.OpenSession())
            {
                var user = session.Get<User>(UserId);
                mDescription = "Add User Role: " + user.UserName;

                var existingUserRoles = session.CreateCriteria<UserRoleAssociation>()
                                              .CreateAlias("User", "user")
                                              .Add(Restrictions.Eq("user.Id", UserId))
                                              .List<UserRoleAssociation>()
                                              .Select(r => r.UserRole.Name)
                                              .ToList();
                var userRoles = session.CreateCriteria<UserRole>()
                                       .List<UserRole>()
                                       .Select(u => u.Name)
                                       .ToList();

                //var userRoles = Enum.GetNames(typeof(UserRoleEnum))
                //                    .Where(u => !u.Equals("AnyOne", StringComparison.InvariantCultureIgnoreCase))
                //                    .ToList();

                ListItems = userRoles.Except(existingUserRoles).ToList();
                if (ListItems.Count == 0)
                {
                    return Task.FromResult<InitializeResult>(new InitializeResult(false, "There are no new user roles to add for the current user."));
                }
            }

            return Task.FromResult<InitializeResult>(new InitializeResult(true));
        }

        public override async System.Threading.Tasks.Task<IList<Event>> ProcessAction(string data, int actionNumber)
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            var userId = parameters["UserId"];

            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUserRoleAssociations, userId)
                };
            }
            else if (actionNumber == 0)
            {
               var role = parameters["UserRole"];

                //UserRoleEnum userRole;
                //if (!Enum.TryParse<UserRoleEnum>(role, out userRole))
                //{
                //    return new List<Event>()
                //    {
                //         new ShowMessage("Unable to create user role.\nUnknown user role '" + role + "'")
                //    };
                //}

                using (var session = Store.OpenSession())
                {
                    var userRole = session.CreateCriteria<UserRole>()
                                          .Add(Restrictions.Eq("Name", role))
                                          .UniqueResult<UserRole>();
                    if (userRole == null)
                    {
                        return new List<Event>()
                        {
                             new ShowMessage("Unable to create user role.\nUnknown user role '" + role + "'")
                        };
                    }

                    var user = session.Get<User>(userId);
                    var existingUserRole = session.CreateCriteria<UserRoleAssociation>()
                                                  .CreateAlias("User", "user")
                                                  .Add(Restrictions.Eq("user.Id", userId))
                                                  .Add(Restrictions.Eq("UserRole", userRole))
                                                  .UniqueResult<UserRoleAssociation>();
                    if (existingUserRole != null)
                    {
                        return new List<Event>()
                        {
                             new ShowMessage("Unable to add user role.\nUser already has '" + role + "' assinged.")
                        };
                    }
                    var newRole = new UserRoleAssociation()
                    {
                        User = user,
                        UserRole = userRole
                    };
                    session.Save(newRole);
                    session.Flush();
                }

                return new List<Event>()
                {
                    new ShowMessage("User role created successfully."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUserRoleAssociations, userId)
                };
            }

            return null;
        }
    }
}