using Newtonsoft.Json.Linq;
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

                var events = Enum.GetNames(typeof(EventNumber))
                                    .Where(u => !u.Equals("Nothing", StringComparison.InvariantCultureIgnoreCase))
                                    .OrderBy(e => e)
                                    .ToList();

                list.Add(new ComboBoxInput("Event", "Event")
                {
                    ListItems = events,
                });

                var userRoles = Enum.GetNames(typeof(UserRole))
                                    .OrderBy(u => u)
                                    .ToList();

                list.Add(new ComboBoxInput("UserRole", "Allowed User Role")
                {
                    ListItems = userRoles
                });

                return list;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.AddEventRoleAssociation;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            return new InitializeResult(true);
        }

        public override async Task<IList<Event>> ProcessAction(string data, int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewEventRoleAssociation, "")
                };
            }
            else if (actionNumber == 0)
            {
                if (String.IsNullOrWhiteSpace(data))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("There was an error creating the event role association. No input was received.")
                    };
                };

                var json = JObject.Parse(data);

                var eventName = json.GetValue("Event").ToString();
                var userRoleName = json.GetValue("UserRole").ToString();

                var theEvent = (EventNumber)Enum.Parse(typeof(EventNumber), eventName);
                var userRole = (UserRole)Enum.Parse(typeof(UserRole), userRoleName);

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
                    new ExecuteAction(EventNumber.ViewEventRoleAssociation)
                };
            }
            return null;
        }
    }
}