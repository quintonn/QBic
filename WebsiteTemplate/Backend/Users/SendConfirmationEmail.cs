using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Users
{
    public class SendConfirmationEmail : DoSomething
    {
        private UserService UserService { get; set; }

        public SendConfirmationEmail(UserService service, DataService dataService) : base(dataService)
        {
            UserService = service;
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.SendConfirmationEmail;
        }

        public override string Description
        {
            get
            {
                return "Sends a confirmation email to a new user";
            }
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var results = new List<IEvent>();

            var emailSentResultMessage = String.Empty;
            var id = GetValue("Id");

            var user = UserService.RetrieveUser(id);
            emailSentResultMessage = await UserService.SendAcccountConfirmationEmail(user.Id, user.UserName, user.Email);

            if (String.IsNullOrWhiteSpace(emailSentResultMessage))
            {
                results.Add(new ShowMessage("Email confirmation sent successfully"));
            }
            else
            {
                var result = new ShowMessage("Email confirmation could not be sent\n" + emailSentResultMessage);
                results.Add(result);
            }
            
            return results;
        }
    }
}