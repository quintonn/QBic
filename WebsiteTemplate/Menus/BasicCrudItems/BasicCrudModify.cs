using QBic.Core.Data.BaseTypes;
using QBic.Core.Models;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public class BasicCrudModify<T> : GetInput, IBasicCrudModify where T : BaseClass
    {
        public BasicCrudModify(DataService dataService)
        {
            DataService = dataService;
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public string UniquePropertyName { get; set; }

        private T Item { get; set; } = null;
        private bool IsNew { get; set; } = true;

        public int Id { get; set; }

        public string ItemName { get; set; }

        public Dictionary<string, string> InputProperties { get; set; }

        public Action<object, bool> OnModifyInternal { get; set; }

        public override string Description
        {
            get
            {
                return IsNew ? "Add " + ItemName : "Edit " + ItemName;
            }
        }

        public override IList<InputField> GetInputFields()
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
                else if (baseType == typeof(int))
                {
                    list.Add(new NumericInput<int>(input.Key, input.Value, defaultValue));
                }
                else if (baseType == typeof(DateTime) || baseType == typeof(DateTime?))
                {
                    list.Add(new DateInput(input.Key, input.Value, defaultValue));
                }
                else if (baseType == typeof(bool))
                {
                    list.Add(new BooleanInput(input.Key, input.Value, defaultValue));
                }
                else if (baseType == typeof(LongString))
                {
                    list.Add(new StringInput(input.Key, input.Value, defaultValue)
                    {
                        MultiLineText = true
                    });
                }
                else if (baseType.IsEnum == true)
                {
                    var cmbBaseType = typeof(EnumComboBoxInput<>);
                    var cmbType = cmbBaseType.MakeGenericType(baseType);
                    var ctor  = cmbType.GetConstructors().First();
                    var item = ctor.Invoke(new object[] { input.Key, input.Value, false, null, null, defaultValue?.ToString(), null });
                    list.Add(item as InputField);
                }
                else if (baseType.IsSubclassOf(typeof(DynamicClass)))
                {
                    var type = typeof(DataSourceComboBoxInput<>);
                    //new DataSourceComboBoxInput<DynamicClass>(input.Key, input.Value, x => x.Id, x => x.ToString(), defaultValue);
                    var comboType = type.MakeGenericType(baseType);
                    Func<DynamicClass, string> keyFunc = x => x.Id;
                    Func<dynamic, object> valueFunc = x => x.ToString();
                    var ctor = comboType.GetConstructors()[0];
                    var item = ctor.Invoke(new object[] { input.Key, input.Value, keyFunc, valueFunc, defaultValue, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing });
                    var comboInstance = item as InputField;
                    //var enumInstance = Activator.CreateInstance(comboType, input.Key, input.Value, keyFunc, valueFunc, defaultValue) as InputField;
                    list.Add(comboInstance);
                    //var enumInstance = enumComboType.GetConstructors()[0].Invoke(enumComboType, input.Key, input.Value);
                    //list.Add(new EnumComboBoxInput<string>(input.Key, input.Value));
                }
                else
                {
                    throw new NotImplementedException($"Input type {baseType.ToString()} is not handled yet");
                }

            }

            list.Add(new HiddenInput("IsNew", IsNew));
            list.Add(new HiddenInput("Id", Item?.Id));


            return list;
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
                    var dateFormat = String.Empty;
                    T item;
                    if (!isNew)
                    {
                        item = session.Get<T>(itemId);
                    }
                    else
                    {
                        item = Activator.CreateInstance<T>();
                    }

                    if (!String.IsNullOrWhiteSpace(UniquePropertyName))
                    {
                        var uniqueValue = GetValue(UniquePropertyName);

                        var existingItem = session.CreateCriteria<T>()
                                                  .Add(Restrictions.Eq(UniquePropertyName, uniqueValue))
                                                  .UniqueResult<T>();

                        if (existingItem != null && existingItem.Id != item.Id)
                        {
                            return new List<IEvent>()
                            {
                                new ShowMessage(this.ItemName + " " + uniqueValue + " already exists")
                            };
                        }
                    }

                    foreach (var value in inputs)
                    {
                        var prop = typeof(T).GetProperty(value.Key);
                        if (prop.PropertyType == typeof(LongString))
                        {
                            prop.SetValue(item, new LongString(value.Value?.ToString()));
                        }
                        else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        {
                            DateTime date;
                            if (String.IsNullOrWhiteSpace(dateFormat))
                            {
                                var appSettings = session.QueryOver<SystemSettings>().List<SystemSettings>().FirstOrDefault();
                                dateFormat = appSettings.DateFormat;
                            }
                            if (DateTime.TryParseExact(value.Value?.ToString(), dateFormat, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                            {
                                prop.SetValue(item, date);
                            }
                        }
                        else
                        {
                            prop.SetValue(item, value.Value);
                        }
                    }

                    OnModifyInternal(item, isNew);

                    DataService.SaveOrUpdate(session, item);
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