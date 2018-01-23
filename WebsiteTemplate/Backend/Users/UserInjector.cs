using NHibernate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Users
{
    public abstract class UserInjector
    {
        protected DataService DataService { get; set; }
        internal IDictionary<string, object> InputData { get; set; }

        public UserInjector(DataService dataService)
        {
            DataService = dataService;
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

        protected T GetValue<T>(string propertyName, T defaultValue = default(T))
        {
            return InputProcessingMethods.GetValue(InputData, propertyName, defaultValue);
        }

        protected T GetDataSourceValue<T>(string propertyName)
        {
            return InputProcessingMethods.GetDataSourceValue<T>(InputData, DataService, propertyName);
        }

        protected string GetValue(string propertyName)
        {
            return InputProcessingMethods.GetValue<string>(InputData, propertyName);
        }

        protected DateTime? GetDateFromString(string dateString, DateTime? defaultValue = null)
        {
            return InputProcessingMethods.GetDateFromString(dateString, defaultValue);
        }

        protected decimal GetDecimalValue(string decimalString, decimal defaultValue = 0)
        {
            return InputProcessingMethods.GetDecimalValue(decimalString, defaultValue);
        }


    }
}