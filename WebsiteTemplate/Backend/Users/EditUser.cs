﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;
using System.Linq;


namespace WebsiteTemplate.Backend.Users
{
    public class EditUser : GetInput
    {
        public override System.Threading.Tasks.Task<InitializeResult> Initialize(string data)
        {
            var json = JObject.Parse(data);
            var id = json.GetValue("Id").ToString();
            var results = new List<Event>();
            using (var session = Store.OpenSession())
            {
                User = session.Get<User>(id);

                session.Flush();
            }
            return Task.FromResult<InitializeResult>(new InitializeResult(true));
        }

        public override async System.Threading.Tasks.Task<IList<Event>> ProcessAction(string data, int actionNumber)
        {
            if (actionNumber == 0)
            {
                var json = JObject.Parse(data);

                var userRoles = json.GetValue("UserRoles") as JArray;

                using (var session = Store.OpenSession())
                {
                    var id = json.GetValue("Id").ToString();
                    var userName = json.GetValue("UserName").ToString();

                    var existingUser = session.CreateCriteria<User>()
                                              .Add(Restrictions.Eq("UserName", userName))
                                              .Add(Restrictions.Not(Restrictions.Eq("Id", id)))
                                              .UniqueResult<User>();
                    if (existingUser != null)
                    {
                        return new List<Event>()
                        {
                            new ShowMessage("Unable to modify user. User with name {0} already exists.", userName)
                        };
                    }

                    var dbUser = session.Get<User>(id);
                    dbUser.UserName = json.GetValue("UserName").ToString();
                    dbUser.Email = json.GetValue("Email").ToString();
                    session.Update(dbUser);

                    var existingUserRoles = session.CreateCriteria<UserRoleAssociation>()
                                                   .CreateAlias("User", "user")
                                                   .Add(Restrictions.Eq("user.Id", dbUser.Id))
                                                   .List<UserRoleAssociation>()
                                                   .ToList();
                    existingUserRoles.ForEach(u =>
                    {
                        session.Delete(u);
                    });

                    foreach (var role in userRoles)
                    {
                        var dbUserRole = session.CreateCriteria<UserRole>()
                                                .Add(Restrictions.Eq("Name", role.ToString()))
                                                .UniqueResult<UserRole>();

                        var roleAssociation = new UserRoleAssociation()
                        {
                            User = dbUser,
                            UserRole = dbUserRole
                        };
                        session.Save(roleAssociation);
                    }

                    session.Flush();
                }

                return new List<Event>()
                {
                    new ShowMessage("User modified successfully."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUsers)
                };
            }

            return new List<Event>()
            {
                new CancelInputDialog()
            };
        }

        private User User { get; set; }

        public override int GetId()
        {
            return EventNumber.EditUser;
        }

        public override string Description
        {
            get
            {
                return "Edit User";
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var results = new List<InputField>();

                results.Add(new StringInput("UserName", "User Name", User.UserName));
                results.Add(new StringInput("Email", "Email", User.Email));
                results.Add(new HiddenInput("Id", User.Id));

                using (var session = Store.OpenSession())
                {
                    var items = session.CreateCriteria<UserRole>()
                                       .List<UserRole>()
                                       .OrderBy(u => u.Name)
                                       .ToDictionary(u => u.Name, u => (object)u.Description);

                    var existingItems = session.CreateCriteria<UserRoleAssociation>()
                                               .CreateAlias("User", "user")
                                               .Add(Restrictions.Eq("user.Id", User.Id))
                                               .List<UserRoleAssociation>()
                                               .Select(u => u.UserRole.Name)
                                               .OrderBy(u => u)
                                               .ToList();

                    var listSelection = new ListSelectionInput("UserRoles", "User Roles", existingItems)
                    {
                        AvailableItemsLabel = "List of User Roles:",
                        SelectedItemsLabel = "Chosen User Roles:",
                        ListSource = items
                    };

                    results.Add(listSelection);
                }

                return results;
            }
        }

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
    }
}