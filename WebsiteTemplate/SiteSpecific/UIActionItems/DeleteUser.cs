﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.UIActionItems
{
    public class DeleteUser : DoSomething
    {
        public override int Id
        {
            get
            {
                return UIActionNumbers.DELETE_USER;
            }
        }

        public override string Name
        {
            get
            {
                return "Delete User";
            }
        }

        public override string Description
        {
            get
            {
                return "Deletes a user";
            }
        }

        public override string MenuLabel
        {
            get
            {
                return "";
            }
        }

        public override IList<UserRole> AuthorizedUserRoles
        {
            get
            {
                return new List<UserRole>();
            }
        }

        public override async Task<IList<UIAction>> ProcessAction(string data)
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            var id = parameters["Id"];

            try
            {
                using (var session = Store.OpenSession())
                {
                    var user = session.Get<User>(id);
                    session.Delete(user);
                    session.Flush();
                }
            }
            catch (Exception eee)
            {
                return new List<UIAction>()
                {
                    new ShowMessage(eee.Message)
                };
            }

            return new List<UIAction>()
            {
                new ShowMessage("User deleted successfully"),
                new CancelInputDialog(),
                new ExecuteAction(UIActionNumbers.VIEW_USERS)
            };
        }
    }
}