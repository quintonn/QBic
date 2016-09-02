using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Users
{
    public class DeleteUser : DoSomething
    {
        private UserService UserService { get; set; }

        public DeleteUser(UserService service)
        {
            UserService = service;
        }

        public override EventNumber GetId()
        {
            return EventNumber.DeleteUser;
        }

        public override string Description
        {
            get
            {
                return "Delete User";
            }
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var id = GetValue<string>("Id");

            UserService.DeleteUser(id);

            return new List<IEvent>()
            {
                new ShowMessage("User deleted successfully"),
                new CancelInputDialog(),
                new ExecuteAction(EventNumber.ViewUsers, String.Empty)
            };
        }
    }
}