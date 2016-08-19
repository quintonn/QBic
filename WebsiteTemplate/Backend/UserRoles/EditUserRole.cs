using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.CustomMenuItems;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class EditUserRole : GetInput
    {
        public UserRole UserRole { get; set; }
        public override string Description
        {
            get
            {
                return "Edit User Role";
            }
        }

        public override IList<InputButton> InputButtons
        {
            get
            {
                return new List<InputButton>()
                {
                    new InputButton("Submit", 0),
                    new InputButton("Cancel", 1, false)
                };
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("Name", "Name", UserRole?.Name));
                list.Add(new StringInput("Description", "Description", UserRole?.Description));
                list.Add(new HiddenInput("Id", UserRole?.Id));

                var items = MainController.EventList.ToDictionary(e => e.Key, e => e.Value.Description)
                                                    .OrderBy(e => e.Value)
                                                    .ToDictionary(e => e.Key.ToString(), e => (object)e.Value);


                var existingItems = new List<string>();
                using (var session = Store.OpenSession())
                {
                    var events = session.CreateCriteria<EventRoleAssociation>()
                                        .CreateAlias("UserRole", "role")
                                        .Add(Restrictions.Eq("role.Id", UserRole?.Id))
                                        .List<EventRoleAssociation>()
                                        .Select(e => e.Event)
                                        .ToList();

                    existingItems = items.Where(e => events.Contains(Convert.ToInt32(e.Key))).Select(i => i.Key.ToString()).ToList();

                }
                var listSelection = new ListSelectionInput("Events", "Allowed Events", existingItems)
                {
                    AvailableItemsLabel = "List of Events:",
                    SelectedItemsLabel = "Chosen Events:",
                    ListSource = items
                };

                list.Add(listSelection);

                return list;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.EditUserRole;
        }

        public override Task<InitializeResult> Initialize(string data)
        {
            //var json = JObject.Parse(data);
            var json = JsonHelper.Parse(data);
            var id = json.GetValue("Id");
            
            using (var session = Store.OpenSession())
            {
                UserRole = session.Get<UserRole>(id);

                session.Flush();
            }
            return Task.FromResult<InitializeResult>(new InitializeResult(true));
        }

        public override async Task<IList<Event>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUserRoles)
                };
            }
            else if (actionNumber == 0)
            {
                var id = GetValue<string>("Id");
                var name = GetValue<string>("Name");
                var description = GetValue<string>("Description");

                var events = GetValue<List<string>>("Events");

                if (String.IsNullOrWhiteSpace(name))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("Name is mandatory and must be provided.")
                    };
                }
                if (String.IsNullOrWhiteSpace(description))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("Description is mandatory and must be provided.")
                    };
                }

                using (var session = Store.OpenSession())
                {
                    var dbUserRole = session.CreateCriteria<UserRole>()
                                             .Add(Restrictions.Eq("Name", name))
                                             .Add(Restrictions.Not(Restrictions.Eq("Id", id)))
                                             .UniqueResult<UserRole>();
                    if (dbUserRole != null)
                    {
                        return new List<Event>()
                        {
                            new ShowMessage("User role {0} already exists.", name)
                        };
                    }

                    dbUserRole = session.Get<UserRole>(id);
                    dbUserRole.Name = name;
                    dbUserRole.Description = description;
                    session.Save(dbUserRole);


                    var existingEvents = session.CreateCriteria<EventRoleAssociation>()
                                       .CreateAlias("UserRole", "role")
                                       .Add(Restrictions.Eq("role.Id", dbUserRole.Id))
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

                return new List<Event>()
                {
                    new ShowMessage("User role modified successfully."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUserRoles)
                };
            }

            return null;
        }
    }
}