using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public class BasicCrudModify<T> : GetInput where T : BaseClass
    {
        private T Item { get; set; } = null;
        private bool IsNew { get; set; } = true;

        private int Id { get; set; }

        private string ItemName { get; set; }

        private Dictionary<string, string> InputProperties { get; set; }

        public BasicCrudModify(int id, string itemName, Dictionary<string, string> inputProperties)
        {
            Id = id;
            ItemName = itemName;
            InputProperties = inputProperties;
        }

        public override string Description
        {
            get
            {
                return IsNew ? "Add " + ItemName : "Edit " + ItemName;
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                foreach (var input in InputProperties)
                {
                    var property = typeof(T).GetProperty(input.Key);
                    var baseType = property.PropertyType;

                    object defaultValue = null;
                    if (!IsNew && Item != null)
                    {
                        defaultValue = property.GetValue(Item);
                    }

                    if (baseType == typeof(String))
                    {
                        list.Add(new StringInput(input.Key, input.Value, defaultValue));
                    }
                    else if (baseType == typeof(DateTime))
                    {
                        list.Add(new DateInput(input.Key, input.Value, defaultValue));
                    }
                    else if (baseType == typeof(bool))
                    {
                        list.Add(new BooleanInput(input.Key, input.Value, defaultValue));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                }

                list.Add(new HiddenInput("IsNew", IsNew));
                list.Add(new HiddenInput("Id", Item?.Id));


                return list;
            }
        }

        public override int GetId()
        {
            return Id;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            var jobject = JObject.Parse(data);
            JToken tmp;
            if (jobject.TryGetValue("IsNew", out tmp))
            {
                IsNew = Convert.ToBoolean(tmp);
                Item = Activator.CreateInstance<T>();
            }
            else
            {
                IsNew = false;
                var id = jobject.GetValue("Id").ToString();
                using (var session = Store.OpenSession())
                {
                    Item = session.Get<T>(id);
                }
            }

            return new InitializeResult(true);
        }

        public override async Task<IList<Event>> ProcessAction(Dictionary<string, object> inputData, int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(Id -1)
                };
            }
            else if (actionNumber == 0)
            {
                var isNew = Convert.ToBoolean(inputData["IsNew"]);
                var itemId = inputData["Id"].ToString();

                var inputs = new Dictionary<string, object>();
                foreach (var property in InputProperties)
                {
                    var value = inputData[property.Key].ToString();
                    inputs.Add(property.Key, value);

                    if (value == null || String.IsNullOrWhiteSpace(value.ToString()))
                    {
                        return new List<Event>()
                        {
                            new ShowMessage("{0} is mandatory and must be provided.", property.Key)
                        };
                    }
                }

                using (var session = Store.OpenSession())
                {
                    T item;
                    if (!isNew)
                    {
                        item = session.Get<T>(itemId);
                    }
                    else
                    {
                        item = Activator.CreateInstance<T>();
                    }

                    foreach (var value in inputs)
                    {
                        var prop = typeof(T).GetProperty(value.Key);
                        if (prop.PropertyType == typeof(bool))
                        {
                            prop.SetValue(item, Convert.ToBoolean(value.Value));
                        }
                        else
                        {
                            prop.SetValue(item, value.Value);
                        }
                    }

                    session.Save(item);
                    session.Flush();
                }

                return new List<Event>()
                {
                    new ShowMessage("{1} {0} successfully.", isNew ? "created" : "modified", ItemName),
                    new CancelInputDialog(),
                    new ExecuteAction(Id - 1)
                };
            }
            return null;
        }
    }
}