using Benoni.Core.Data;
using System.Threading.Tasks;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Data
{
    public class UserContext : UserContextBase<User>
    {
        public UserContext(DataStore dataStore) 
            : base(dataStore)
        {
        }

        public override Task<bool> UserCanLogIn(string userId)
        {
            using (var session = DataStore.OpenSession())
            {
                var user = session.Get<User>(userId);
                if (user == null)
                {
                    return Task.FromResult(false);
                }
                if (user.UserStatus == UserStatus.Locked)
                {
                    return Task.FromResult(false);
                }

                session.Flush();
            }
            return Task.FromResult(true);
        }
    }
}