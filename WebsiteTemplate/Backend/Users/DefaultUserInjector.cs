using NHibernate;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Users
{
    public class DefaultUserInjector : UserInjector
    {
        public DefaultUserInjector(DataService dataService) : base(dataService)
        {
            
        }

        public override ProcessingResult DeleteItem(ISession session, string itemId)
        {
            return new ProcessingResult(true);
        }

        public override IList<InputField> GetInputFields(User user)
        {
            return new List<InputField>();
        }

        public override async Task<ProcessingResult> SaveOrUpdate(ISession session, string username)
        {
            return new ProcessingResult(true);
        }
    }
}