using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.UserRoles
{
    public abstract class ModifyUserRole : ModifyItemUsingInputProcessor<UserRoleProcessor, UserRole>
    {
        private UserRoleService UserRoleService { get; set; }
        public ModifyUserRole(UserRoleProcessor itemProcessor, UserRoleService userRoleService, bool isNew) 
            : base(itemProcessor, isNew)
        {
            UserRoleService = userRoleService;
        }

        public override string ItemNameForDisplay
        {
            get
            {
                return "User Role";
            }
        }

        public override string Title
        {
            get
            {
                return IsNew ? "New User Role" : DataItem.Name;
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("Name", "Name", DataItem?.Name, mandatory: true));
                list.Add(new StringInput("Description", "Description", DataItem?.Description, mandatory: true));

                var existingItems = UserRoleService.RetrieveEventRoleAssociationsForUserRole(DataItem?.Id);
                var listSelection = new ListSelectionInput("Events", "Allowed Events", existingItems)
                {
                    AvailableItemsLabel = "List of Events:",
                    SelectedItemsLabel = "Chosen Events:",
                    ListSource = UserRoleService.GetListOfEvents()
                };

                list.Add(listSelection);

                list.Add(new HiddenInput("Id", DataItem?.Id));

                return list;
            }
        }

        public override async Task<ProcessingResult> ValidateInputs()
        {
            var name = GetValue<string>("Name");
            var description = GetValue<string>("Description");

            if (String.IsNullOrWhiteSpace(name))
            {
                return new ProcessingResult(false, "Name is mandatory and must be provided.");
            }
            if (String.IsNullOrWhiteSpace(description))
            {
                return new ProcessingResult(false, "Description is mandatory and must be provided.");
            }
            
            return new ProcessingResult(true);
        }
    }
}