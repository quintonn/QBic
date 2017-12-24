using Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Processing
{
    public class InitializationProcessor : CoreProcessor<IList<IEvent>>
    {
        public InitializationProcessor(IUnityContainer container)
            :base(container)
        {

        }
        public override async Task<IList<IEvent>> ProcessEvent(int eventId)
        {
            return new List<IEvent>();
        }
    }
}