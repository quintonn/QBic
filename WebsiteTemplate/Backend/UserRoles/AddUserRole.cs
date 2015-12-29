using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Backend.Users;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class AddUserRole : GetInput
    {
        public override string Description
        {
            get
            {
                return "Add User Role";
            }
        }

        public override IList<InputButton> InputButtons
        {
            get
            {
                return new List<InputButton>()
                {
                    new InputButton("Submit", 0),
                    new InputButton("Cancel", 1, false),
                };
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("Name", "Name", mandatory:true));
                list.Add(new StringInput("Description", "Description"));

                var items = MainController.EventList.ToDictionary(e => e.Key, e => e.Value.Description)
                                                   .OrderBy(e => e.Value)
                                                   .ToDictionary(e => e.Key.ToString(), e => (object)e.Value);


                var listSelection = new ListSelectionInput("Events", "Allowed Events")
                {
                    AvailableItemsLabel = "List of Events:",
                    SelectedItemsLabel = "Chosen Events:",
                    ListSource = items
                };

                list.Add(listSelection);

                return list;
            }
        }

        public override int GetId()
        {
            return EventNumber.AddUserRole;
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
                    new ExecuteAction(EventNumber.ViewUserRoles, "")
                };
            }
            else if (actionNumber == 0)
            {
                if (String.IsNullOrWhiteSpace(data))
                {
                    return new List<Event>()
                    {
                        new ShowMessage("There was an error creating the user role. No input was received.")
                    };
                };

                var json = JObject.Parse(data);

                var name = json.GetValue("Name").ToString();
                var description = json.GetValue("Description").ToString();
                var events = (json.GetValue("Events") as JArray).ToList();

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
                                              .UniqueResult<UserRole>();
                    if (dbUserRole != null)
                    {
                        return new List<Event>()
                        {
                            new ShowMessage("User role {0} already exists.", name)
                        };
                    }

                    dbUserRole = new UserRole()
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

                return new List<Event>()
                {
                    new ShowMessage("User role created successfully."),
                    new CancelInputDialog(),
                    new ExecuteAction(EventNumber.ViewUserRoles)
                };
            }
            return null;

        }
    }
}