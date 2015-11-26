﻿using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;

namespace WebsiteTemplate.Backend.EventRoleAssociations
{
    public class AddEventRoleAssociation : GetInput
    {
        public override string Description
        {
            get
            {
                return "Add Event Role Association";
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

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                using (var session = Store.OpenSession())
                {
                    var eventNumber = (EventNumber)Convert.ToInt32(EventId);
                    var eventRoles = session.CreateCriteria<EventRoleAssociation>()
                                            .Add(Restrictions.Eq("Event", eventNumber))
                                            .List<EventRoleAssociation>()
                                            .Select(r => r.UserRole.ToString())
                                            .ToList();

                    var userRoles = Enum.GetNames(typeof(UserRoleEnum))
                                    .OrderBy(u => u)
                                    .Where(u => !eventRoles.Contains(u.ToString()))
                                    .ToList();

                    list.Add(new ComboBoxInput("UserRole", "Allowed User Role")
                    {
                        ListItems = userRoles
                    });
                }
                
                list.Add(new HiddenInput("EventId", EventId));

                return list;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.AddEventRoleAssociation;
        }

        private string EventId { get; set; }

        public override async Task<InitializeResult> Initialize(string data)
        {
            EventId = data;
            return new InitializeResult(true);
        }

        public override async Task<IList<Event>> ProcessAction(string data, int actionNumber)
        {
            if (String.IsNullOrWhiteSpace(data))
            {
                return new List<Event>()
                    {
                        new ShowMessage("There was an error creating the event role association. No input was received.")
                    };
            };

            var json = JObject.Parse(data);

            var eventId = json.GetValue("EventId").ToString();

            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewEventRoleAssociations, eventId)
                };
            }
            else if (actionNumber == 0)
            {
                var userRoleName = json.GetValue("UserRole").ToString();

                var theEvent = (EventNumber)Convert.ToInt32(eventId);
                var userRole = (UserRoleEnum)Enum.Parse(typeof(UserRoleEnum), userRoleName);

                var eventRoleAssociation = new EventRoleAssociation()
                {
                    Event = theEvent,
                    UserRole = userRole
                };

                using (var session = Store.OpenSession())
                {
                    var existingItem = session.CreateCriteria<EventRoleAssociation>()
                                              .Add(Restrictions.Eq("Event", theEvent))
                                              .Add(Restrictions.Eq("UserRole", userRole))
                                              .UniqueResult<EventRoleAssociation>();
                    if (existingItem != null)
                    {
                        return new List<Event>()
                        {
                            new ShowMessage("Event role association not added.\nEvent and role already exist.")
                        };
                    }

                    session.Save(eventRoleAssociation);
                    session.Flush();
                }
                
                return new List<Event>()
                {
                    new ShowMessage("Event role association created successfully."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewEventRoleAssociations, eventId)
                };
            }
            return null;
        }
    }
}