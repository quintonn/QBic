using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;


namespace WebsiteTemplate.Backend.Menus
{
    public class DeleteMenu : DoSomething
    {
        public override string Description
        {
            get
            {
                return "Deletes a menu";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.DeleteMenu;
        }

        public void DeleteChildMenus(string menuId, ISession session)
        {
            var childMenuItems = session.CreateCriteria<Menu>()
                                            .CreateAlias("ParentMenu", "parent")
                                            .Add(Restrictions.Eq("parent.Id", menuId))
                                            .List<Menu>();
            foreach (var childMenu in childMenuItems)
            {
                DeleteChildMenus(childMenu.Id, session);
                session.Delete(childMenu);
            }
        }

        public override async Task<IList<Event>> ProcessAction(string data)
        {
            var json = JObject.Parse(data);
            
            var id = json.GetValue("Id").ToString();

            var confirmationString = json.GetValue("Confirmation") + "";
            var confirmed = !String.IsNullOrWhiteSpace(confirmationString);

            var parentId = String.Empty;

            using (var session = Store.OpenSession())
            {
                var menu = session.Get<Menu>(id);
                parentId = menu.ParentMenu == null ? String.Empty : menu.ParentMenu.Id;

                var childMenuItems = session.CreateCriteria<Menu>()
                                            .CreateAlias("ParentMenu", "parent")
                                            .Add(Restrictions.Eq("parent.Id", menu.Id))
                                            .List<Menu>();
                if (childMenuItems.Count > 0 && !confirmed)
                {
                    json.Add("Confirmation", true);

                    return new List<Event>()
                    {
                        new UserConfirmation("Menu has sub-menus.\nThey will be deleted as well?")
                        {
                            OnConfirmationUIAction = EventNumber.DeleteMenu,
                            CancelButtonText = "Cancel",
                            ConfirmationButtonText = "Ok",
                            Data = json
                        }
                    };
                }
                else
                {
                    DeleteChildMenus(menu.Id, session);
                }

                session.Delete(menu);
                session.Flush();
            }

            return new List<Event>()
            {
                new ShowMessage("Menu deleted successfully"),
                new CancelInputDialog(),
                new ExecuteAction(EventNumber.ViewMenus, parentId)
            };
        }
    }
}