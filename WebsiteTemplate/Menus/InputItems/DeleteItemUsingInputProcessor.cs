using QCumber.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class DeleteItemUsingInputProcessor<TItemProcessor, TBaseClass> : DoSomething where TItemProcessor : InputProcessingCore<TBaseClass> where TBaseClass : BaseClass
    {
        protected TItemProcessor ItemProcessor { get; set; }

        public DeleteItemUsingInputProcessor(TItemProcessor itemProcessor)
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

            var result = ItemProcessor.DeleteItem(itemId);

            if (result.Success == false)
            {
                return new List<IEvent>()
                {
                    new ShowMessage(result.Message)
                };
            }

            var message = "Item deleted successfully";
            if (!String.IsNullOrWhiteSpace(result.Message))
            {
                message = result.Message;
            }

            var events = new List<IEvent>();
            events.Add(new CancelInputDialog());
            if (ViewToShowAfterModify != null)
            {
                events.Add(new ExecuteAction(ViewToShowAfterModify, ParametersToPassToViewAfterModify));
            }
            events.Add(new ShowMessage(message));

            return events;
        }
    }
}