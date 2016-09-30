using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.SystemSettings
{
    public class ModiffySystemSettings : GetInput
    {
        private Models.SystemSettings SystemSettings { get; set; }
        private ApplicationSettingsCore AppSettings { get; set; }
        private DataService DataService { get; set; }

        public ModiffySystemSettings(DataService dataService, ApplicationSettingsCore appSettings)
        {
            DataService = dataService;
            AppSettings = appSettings;
        }
        public override string Description
        {
            get
            {
                return "System Settings";
            }
        }

        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public override IList<InputField> InputFields
        {
            get
            {
                var result = new List<InputField>();

                result.Add(new StringInput("EmailFromAddress", "From Email", SystemSettings?.EmailFromAddress, "Mail Settings", true));
                result.Add(new StringInput("EmailHost", "Email Host", SystemSettings?.EmailHost, "Mail Settings", true));
                result.Add(new StringInput("EmailUserName", "Username", SystemSettings?.EmailUserName, "Mail Settings", true));
                result.Add(new PasswordInput("EmailPassword", "Password", SystemSettings?.EmailPassword, "Mail Settings", true));
                result.Add(new NumericInput<int>("EmailPort", "Port", SystemSettings?.EmailPort, "Mail Settings", true));
                result.Add(new BooleanInput("EmailEnableSsl", "Enable Ssl", SystemSettings?.EmailEnableSsl, "Mail Settings", true));

                result.Add(new StringInput("DateFormat", "Date Format", SystemSettings?.DateFormat, "Formats", true));

                result.Add(new StringInput("WebsiteUrl", "Website Base Url", SystemSettings?.WebsiteBaseUrl, "Website", true));

                return result;
            }
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            using (var session = DataService.OpenSession())
            {
                SystemSettings = session.QueryOver<Models.SystemSettings>().List<Models.SystemSettings>().FirstOrDefault();
                if (!String.IsNullOrWhiteSpace(SystemSettings?.EmailPassword))
                {
                    SystemSettings.EmailPassword = Encryption.Decrypt(SystemSettings.EmailPassword, AppSettings.ApplicationPassPhrase);
                }
            }
            return new InitializeResult(true);
        }

        public override EventNumber GetId()
        {
            return EventNumber.ModifySystemSettings;
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                };
            }
            else if (actionNumber == 0)
            {
                var fromEmail = GetValue("EmailFromAddress");
                var host = GetValue("EmailHost");
                var username = GetValue("EmailUserName");
                var password = GetValue("EmailPassword");
                var port = GetValue<int>("EmailPort");
                var enableSsl = GetValue<bool>("EmailEnableSsl");

                var dateFormat = GetValue("DateFormat");

                var websiteBaseUrl = GetValue("WebsiteUrl");

                using (var session = DataService.OpenSession())
                {
                    var systemSettings = session.QueryOver<Models.SystemSettings>().List<Models.SystemSettings>().FirstOrDefault();
                    if (systemSettings == null)
                    {
                        systemSettings = new Models.SystemSettings();
                    }

                    systemSettings.EmailFromAddress = fromEmail;
                    systemSettings.EmailHost = host;
                    systemSettings.EmailUserName = username;
                    systemSettings.EmailPassword = Encryption.Encrypt(password, AppSettings.ApplicationPassPhrase);
                    systemSettings.EmailPort = port;
                    systemSettings.EmailEnableSsl = enableSsl;

                    systemSettings.DateFormat = dateFormat;

                    SystemSettings.WebsiteBaseUrl = websiteBaseUrl;

                    DataService.SaveOrUpdate(session, systemSettings);

                    session.Flush();
                }

                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                    new ShowMessage("System settings modified successfully")
                };
            }
            return new List<IEvent>();
        }
    }
}