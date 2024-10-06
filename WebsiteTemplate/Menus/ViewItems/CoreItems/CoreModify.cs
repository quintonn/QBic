using NHibernate;
using QBic.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.ViewItems.CoreItems
{
    public abstract class CoreModify<T> : GetInput where T : DynamicClass
    {
        protected bool IsNew { get; set; }

        protected T Item { get; set; }

        public CoreModify(DataService dataService, bool isNew) : base(dataService) 
        {
            IsNew = isNew;
        }

        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public sealed override IList<InputField> GetInputFields()
        {
            var result = new List<InputField>();

            foreach (var item in InputParameters.Keys)
            {
                result.Add(new HiddenInput(item, InputParameters[item]));
            }
            result.AddRange(InputFields());

            if (!IsNew && result.Count(r => r.InputName == "Id") == 0)
            {
                result.Add(new HiddenInput("Id", Item?.Id));
            }
            return result;
        }

        public abstract List<InputField> InputFields();

        public abstract string EntityName { get; }
        protected Dictionary<string, string> InputParameters { get; set; } = new Dictionary<string, string>();

        public override string Description
        {
            get
            {
                return (IsNew ? "Add" : "Edit") + " " + EntityName;
            }
        }

        public virtual T CreateDefaultItem()
        {
            return Activator.CreateInstance<T>();
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            if (IsNew)
            {
                Item = CreateDefaultItem();
            }
            else
            {
                using (var session = DataService.OpenSession())
                {
                    var json = JsonHelper.Parse(data);
                    var id = json.GetValue("Id");
                    Item = session.Get<T>(id);
                }
            }
            InputParameters = GetInputParameters(data);
            return new InitializeResult(true);
        }

        public virtual Dictionary<string, string> GetInputParameters(string data)
        {
            return new Dictionary<string, string>();
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                };
            }
            else if (actionNumber == 0)
            {
                var result = new List<IEvent>();
                var id = GetValue("Id");
                var isNew = String.IsNullOrWhiteSpace(id);

                //using (var transaction = new TransactionScope())
                using (var session = DataService.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    var res = await PerformModify(isNew, id, session);
                    if (res != null && res.Count > 0)
                    {
                        return res;
                    }

                    //session.Flush();
                    transaction.Commit();
                    result.Add(new ShowMessage(EntityName + " successfully " + (isNew ? "added" : "modified")));
                    if (GetViewNumber() != null)
                    {
                        result.Add(new ExecuteAction(GetViewNumber(), GetParameterToPassToView()));
                    }
                    result.Add(new CancelInputDialog());
                }
                return result;
            }
            return null;
        }

        protected string GetParameter(string parameterName, string alternateName, string data, bool allowNull = true)
        {
            var item = GetParameter(parameterName, data, true);
            var json = JsonHelper.Parse(item).ToString();
            if (String.IsNullOrWhiteSpace(item) || !String.IsNullOrWhiteSpace(json))
            {
                item = GetParameter(alternateName, data, allowNull);
            }
            return item;
        }

        protected string GetParameter(string parameterName, string data, bool allowNull = true)
        {
            var json = JsonHelper.Parse(data);
            var result = String.Empty;
            if (!String.IsNullOrWhiteSpace(data))
            {
                var x = JsonHelper.Parse(data);
                result = x.GetValue(parameterName);
                if (String.IsNullOrWhiteSpace(result))// && String.IsNullOrWhiteSpace(x.ToString()))
                {
                    result = data;
                }
            }
            else
            {
                var eventParams = json.GetValue("eventParameters");
                if (!String.IsNullOrWhiteSpace(eventParams))
                {
                    var x = JsonHelper.Parse(eventParams);
                    result = x.GetValue(parameterName);
                }
                else if (!String.IsNullOrWhiteSpace(data))
                {
                    result = data;
                }
            }

            if (!allowNull && String.IsNullOrWhiteSpace(result))
            {
                throw new Exception("Parameter " + parameterName + " should not be null");
            }

            return result;
        }

        public virtual string GetParameterToPassToView()
        {
            return String.Empty;
        }

        public abstract EventNumber GetViewNumber();

        public abstract Task<IList<IEvent>> PerformModify(bool isNew, string id, ISession session);

        protected IList<IEvent> ErrorMessage(string message)
        {
            return new List<IEvent>()
            {
                new ShowMessage(message)
            };
        }
    }
}