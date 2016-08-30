using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class ModifyItemUsingDataService<D, T> : GetInput where D : DataItemServiceCore<T> where T : BaseClass
    {
        protected D DataItemService { get; set; }

        protected bool IsNew { get; set; }

        protected T DataItem { get; set; }

        public abstract string ItemNameForDisplay { get; }

        public override string Description
        {
            get
            {
                var result = IsNew ? "Add {0}" : "Edit {0}";
                return String.Format(result, ItemNameForDisplay);
            }
        }

        public ModifyItemUsingDataService(D dataItemService, bool isNew)
        {
            DataItemService = dataItemService;
            IsNew = isNew;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            var json = JsonHelper.Parse(data);
            if (IsNew)
            {
                DataItem = null;
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

                DataItemService.SaveOrUpdate(itemId);

                return new List<IEvent>()
                {
                    new ShowMessage("{0} {1} successfully.", ItemNameForDisplay, IsNew ? "created" : "modified"),
                    new CancelInputDialog(),
                    new ExecuteAction(ViewToShowAfterModify, ParametersToPassToViewAfterModify)
                };
            }
            return null;
        }
    }
}