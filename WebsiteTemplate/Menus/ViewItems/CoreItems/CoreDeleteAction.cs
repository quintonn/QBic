using NHibernate;
using QBic.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems.CoreItems
{
    public abstract class CoreDeleteAction<T> : CoreAction where T : DynamicClass
    {
        public CoreDeleteAction(DataService dataService)
            : base(dataService)
        {
        }

        public abstract string EntityName { get; }

        public override string Description => "Delete " + EntityName;

        public abstract EventNumber ViewNumber { get; }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var id = GetValue("Id");
            using (var session = DataService.OpenSession())
            {
                var dbItem = session.Get<T>(id);

                try
                {
                    DeleteOtherItems(session, dbItem);
                }
                catch (Exception error)
                {
                    return ErrorMessage(error.Message);
                }

                DataService.TryDelete(session, dbItem);

                session.Flush();
            }

            return new List<IEvent>()
            {
                new ShowMessage(EntityName+" deleted successfully"),
                new CancelInputDialog(),
                new ExecuteAction(ViewNumber)
            };
        }

        public virtual void DeleteOtherItems(ISession session, T mainItem)
        {
        }

        protected IList<IEvent> ErrorMessage(string message)
        {
            return new List<IEvent>()
            {
                new ShowMessage(message)
            };
        }
    }
}