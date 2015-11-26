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
                var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

                using (var session = Store.OpenSession())
                {
                    var id = parameters["Id"];
                    var userName = parameters["UserName"];

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
                    dbUser.UserName = parameters["UserName"];
                    dbUser.Email = parameters["Email"];
                    session.Update(dbUser);
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

        public override EventNumber GetId()
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