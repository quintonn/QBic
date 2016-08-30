using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class ModifyItemUsingDataService<TDataItemService, TBaseClass> : GetInput where TDataItemService : InputProcessingCore<TBaseClass> where TBaseClass : BaseClass
    {
        protected TDataItemService DataItemService { get; set; }

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

        public ModifyItemUsingDataService(TDataItemService dataItemService, bool isNew)
        {
            DataItemService = dataItemService;
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
                DataItem = DataItemService.RetrieveItem(id);
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

        public abstract Task<IList<IEvent>> ValidateInputs();

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            DataItemService.InputData = InputData;
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

                var existingItem = DataItemService.RetrieveExistingItem();
                if (IsNew && existingItem != null)
                {
                    return new List<IEvent>()
                    {
                        new ShowMessage("{0} already exists", ItemNameForDisplay)
                    };
                }

                var validationErrors = await ValidateInputs();
                if (validationErrors != null && validationErrors.Count > 0)
                {
                    return validationErrors;
                }

                DataItemService.SaveOrUpdate(itemId);

                return new List<IEvent>()
                {
                    new ExecuteAction(ViewToShowAfterModify, ParametersToPassToViewAfterModify),
                    new CancelInputDialog(),
                    new ShowMessage("{0} {1} successfully.", ItemNameForDisplay, IsNew ? "created" : "modified"),
                };
            }
            return null;
        }
    }
}