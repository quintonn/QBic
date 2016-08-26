using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Data;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public class BasicCrudModify<T> : GetInput, IBasicCrudModify where T : BaseClass
    {
        private DataService DataService { get; set; }

        public BasicCrudModify(DataService dataService)
        {
            DataService = dataService;
        }

        private T Item { get; set; } = null;
        private bool IsNew { get; set; } = true;

        public int Id { get; set; }

        public string ItemName { get; set; }

        public Dictionary<string, string> InputProperties { get; set; }

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

        public override EventNumber GetId()
        {
            return Id;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            var json = JsonHelper.Parse(data);
            
            if (!String.IsNullOrWhiteSpace(json.GetValue("IsNew")))
            {
                IsNew = json.GetValue<bool>("IsNew");
                Item = Activator.CreateInstance<T>();
            }
            else
            {
                IsNew = false;
                var id = json.GetValue("Id");
                using (var session = DataService.OpenSession())
                {
                    Item = session.Get<T>(id);
                }
            }

            return new InitializeResult(true);
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(Id -1)
                };
            }
            else if (actionNumber == 0)
            {
                var isNew = Convert.ToBoolean(GetValue<string>("IsNew"));
                var itemId = GetValue<string>("Id");

                var inputs = new Dictionary<string, object>();
                foreach (var property in InputProperties)
                {
                    var value = GetValue<object>(property.Key);
                    inputs.Add(property.Key, value);

                    if (value == null || String.IsNullOrWhiteSpace(value.ToString()))
                    {
                        return new List<IEvent>()
                        {
                            new ShowMessage("{0} is mandatory and must be provided.", property.Key)
                        };
                    }
                }

                using (var session = DataService.OpenSession())
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
                        prop.SetValue(item, value.Value);
                    }

                    DataService.SaveOrUpdate(item);
                    session.Flush();
                }

                return new List<IEvent>()
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