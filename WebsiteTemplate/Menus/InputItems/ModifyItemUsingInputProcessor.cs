using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class ModifyItemUsingInputProcessor<TItemProcessor, TBaseClass> : GetInput where TItemProcessor : InputProcessingCore<TBaseClass> where TBaseClass : BaseClass
    {
        protected TItemProcessor ItemProcessor { get; set; }

        protected bool IsNew { get; set; }

        protected TBaseClass DataItem { get; set; }

        public abstract string ItemNameForDisplay { get; }

        public override string Description
        {
            get
            {
                var result = IsNew ? "Add {0}" : "Edit {0}";
                return String.Format(result, ItemNameForDisplay);
            }
        }

        public ModifyItemUsingInputProcessor(TItemProcessor itemProcessor, bool isNew)
        {
            ItemProcessor = itemProcessor;
            IsNew = isNew;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            var json = JsonHelper.Parse(data);
            if (IsNew)
            {
                DataItem = Activator.CreateInstance<TBaseClass>();
            }
            else
            {
                var id = json.GetValue("Id");
                DataItem = ItemProcessor.RetrieveItem(id);
            }

            return new InitializeResult(true);
        }

        public abstract EventNumber ViewToShowAfterModify { get; }

        public virtual string ParametersToPassToViewAfterModify
        {
            get
            {
                return String.Empty;
            }
        }

        public abstract Task<ProcessingResult> ValidateInputs();

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            ItemProcessor.InputData = InputData;
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                    new ExecuteAction(ViewToShowAfterModify, ParametersToPassToViewAfterModify)
                };
            }
            else if (actionNumber == 0)
            {
                var itemId = GetValue("Id");

                var existingItem = ItemProcessor.RetrieveExistingItem();
                if ((IsNew && existingItem != null) || (!IsNew && existingItem != null && existingItem.Id != itemId))
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("{0} already exists", ItemNameForDisplay)
                    };
                }

                var validationError = await ValidateInputs();
                if (validationError != null && validationError.Success == false)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage(validationError.Message)
                    };
                }

                var result = await ItemProcessor.SaveOrUpdate(itemId);

                if (result.Success == false)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage(result.Message)
                    };
                }

                var message = String.Format("{0} {1} successfully.", ItemNameForDisplay, IsNew ? "created" : "modified");
                if (!String.IsNullOrWhiteSpace(result.Message))
                {
                    message = result.Message;
                }

                return new List<IEvent>()
                {
                    new ExecuteAction(ViewToShowAfterModify, ParametersToPassToViewAfterModify),
                    new CancelInputDialog(),
                    new ShowMessage(message),
                };
            }
            return null;
        }
    }
}