using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.IO;
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

                //list.Add(new StringInput("xxx", "xxx", "", "", true));
                //list.Add(new StringInput("xxx2", "xxx2", "", "x", false)
                //{
                //    MandatoryConditions = new List<WebsiteTemplate.Menus.ViewItems.Condition>()
                //    {
                //        new WebsiteTemplate.Menus.ViewItems.Condition("Name", WebsiteTemplate.Menus.ViewItems.Comparison.Equals, "q")
                //    },
                //    VisibilityConditions = new List<WebsiteTemplate.Menus.ViewItems.Condition>()
                //    {
                //        new WebsiteTemplate.Menus.ViewItems.Condition("Name", WebsiteTemplate.Menus.ViewItems.Comparison.Equals, "q")
                //    },
                //});

                //list.Add(new FileInput("File", "File", null, "x"));

                //list.Add(new ViewInput("view", "View", new TestView(), null, "zadsf", false));

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
                //list.Add(new StringInput("abc", "abc"));

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

        public override async Task<IList<Event>> ProcessAction(Dictionary<string, object> inputData, int actionNumber)
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
                var name = inputData["Name"].ToString();
                var description = inputData["Description"].ToString();
                var events = (inputData["Events"] as JArray).ToList();

                //var viewData = inputData["view"] as List<JToken>;
                //var x = viewData[0];

                //var binData = (byte[])inputData["File"];
                //var binData = Convert.FromBase64String(fileData);

                //using (var stream = new MemoryStream(binData))
                //using (var output = File.Create("D:\\abc.xlsx"))
                //{
                //    stream.CopyTo(output);
                //}

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