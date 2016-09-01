using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class DeleteItemUsingDataService<TItemProcessor, TBaseClass> : DoSomething where TItemProcessor : InputProcessingCore<TBaseClass> where TBaseClass : BaseClass
    {
        protected TItemProcessor ItemProcessor { get; set; }

        public DeleteItemUsingDataService(TItemProcessor itemProcessor)
        {
            ItemProcessor = itemProcessor;
        }

        public abstract EventNumber ViewToShowAfterModify { get; }

        public virtual string ParametersToPassToViewAfterModify
        {
            get
            {
                return String.Empty;
            }
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            ItemProcessor.InputData = InputData;

            var itemId = GetValue<string>("Id");

            ItemProcessor.DeleteItem(itemId);

            return new List<IEvent>()
            {
                new CancelInputDialog(),
                new ExecuteAction(ViewToShowAfterModify, ParametersToPassToViewAfterModify),
                new ShowMessage("Item deleted successfully"),
            };
        }
    }
}