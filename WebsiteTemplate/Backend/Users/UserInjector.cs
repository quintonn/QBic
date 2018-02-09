using NHibernate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Users
{
    public abstract class UserInjector : InjectorBase
    {
        public UserInjector(DataService dataService)
            : base(dataService)
        {
        }

        public abstract IList<InputField> GetInputFields(User user);
        public abstract Task<ProcessingResult> SaveOrUpdate(ISession session, string username);

        public abstract ProcessingResult DeleteItem(ISession session, string itemId);

        internal async Task<ProcessingResult> SaveOrUpdateUser(IDictionary<string, object> inputData, string username)
        {
            ProcessingResult result;
            using (var session = DataService.OpenSession())
            {
                InputData = inputData;

                result = await SaveOrUpdate(session, username);

                session.Flush();
            }
            return result;
        }
    }
}