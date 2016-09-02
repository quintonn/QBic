using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Users
{
    public abstract class ModifyUser : ModifyItemUsingDataService<UserProcessor, User>
    {
        private UserService UserService { get; set; }

        public ModifyUser(UserService service, UserProcessor userProcessor, bool isNew)
            : base(userProcessor, isNew)
        {
            UserService = service;
        }

        public override string ItemNameForDisplay
        {
            get
            {
                return "User";
            }
        }

        public override string Title
        {
            get
            {
                return IsNew ? "New User" : DataItem.UserName;
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var list = new List<InputField>();

                list.Add(new StringInput("UserName", "User Name", DataItem?.UserName, mandatory: true));
                list.Add(new StringInput("Email", "Email", DataItem?.Email));
                if (IsNew)
                {
                    list.Add(new PasswordInput("Password", "Password"));
                    list.Add(new PasswordInput("ConfirmPassword", "Confirm Password"));
                }

                var items = UserService.GetUserRoles()
                                       .OrderBy(u => u.Description)
                                       .ToDictionary(u => u.Id, u => (object)u.Description);

                var existingItems = new List<string>();
                if (!IsNew)
                {
                    existingItems = UserService.RetrieveUserRoleAssocationsForUserId(DataItem?.Id)
                                               .OrderBy(u => u.UserRole.Description)
                                               .Select(u => u.UserRole.Id)
                                               .ToList();
                }

                var listSelection = new ListSelectionInput("UserRoles", "User Roles", existingItems)
                {
                    AvailableItemsLabel = "List of User Roles:",
                    SelectedItemsLabel = "Chosen User Roles:",
                    ListSource = items
                };

                list.Add(listSelection);

                list.Add(new HiddenInput("Id", DataItem?.Id));

                return list;
            }
        }

        public override EventNumber ViewToShowAfterModify
        {
            get
            {
                return EventNumber.ViewUsers;
            }
        }

        public override async Task<ProcessingResult> ValidateInputs()
        {
            var email = GetValue<string>("Email");
            var userName = GetValue<string>("UserName");
            var password = GetValue<string>("Password");
            var confirmPassword = GetValue<string>("ConfirmPassword");

            if (String.IsNullOrWhiteSpace(email))
            {
                return new ProcessingResult(false, "Email is mandatory.");
            }
            if (String.IsNullOrWhiteSpace(userName))
            {
                return new ProcessingResult(false, "User name is mandatory.");
            }

            if (IsNew && password != confirmPassword)
            {
                return new ProcessingResult(false, "Password and password confirmation do not match");
            }

            return null;
        }
    }
}