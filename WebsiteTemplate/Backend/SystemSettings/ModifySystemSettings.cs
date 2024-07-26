using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.SystemSettings
{
    public class ModifySystemSettings : GetInput
    {
        private Models.SystemSettings SystemSettings { get; set; }

        /// <summary>
        /// This is for additional settings inputs obtained from ApplicationSettingsCore instance for each project.
        /// </summary>
        //private Dictionary<string, object> SystemSettingValues { get; set; }
        private ApplicationSettingsCore AppSettings { get; set; }
        
        public ModifySystemSettings(DataService dataService, ApplicationSettingsCore appSettings) : base(dataService)
        {
            AppSettings = appSettings;
            //SystemSettingValues = new Dictionary<string, object>();
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

        public override IList<InputField> GetInputFields()
        {
            var result = new List<InputField>();

            // TODO:
            // TODO: can get rid of date format, and maybe other settings ??

            result.Add(new StringInput("EmailFromAddress", "From Email", SystemSettings?.EmailFromAddress, "Mail Settings", true));
            result.Add(new StringInput("EmailHost", "Email Host", SystemSettings?.EmailHost, "Mail Settings", true));
            result.Add(new StringInput("EmailUserName", "Username", SystemSettings?.EmailUserName, "Mail Settings", false));
            result.Add(new PasswordInput("EmailPassword", "Password", null, "Mail Settings", false));
            result.Add(new NumericInput<int>("EmailPort", "Port", SystemSettings?.EmailPort, "Mail Settings", true));
            result.Add(new BooleanInput("EmailEnableSsl", "Enable Ssl", SystemSettings?.EmailEnableSsl, "Mail Settings", true));

            result.Add(new StringInput("DateFormat", "Date Format", SystemSettings?.DateFormat, "Formats", true));
            result.Add(new NumericInput<int>("TimeOffset", "Time Offset", SystemSettings?.TimeOffset, "Formats", true));

            result.Add(new StringInput("WebsiteUrl", "Website Base Url", SystemSettings?.WebsiteBaseUrl, "Website", true));
            result.Add(new LabelInput("64Bit", "Is 64 bit", Environment.Is64BitOperatingSystem.ToString(), "Website"));
            result.Add(new LabelInput("Time", "System Time", DateTime.Now.ToString("HH:mm:ss")));
            using (var session = DataService.OpenSession())
            {
                var additionalSettings = AppSettings.GetAdditionalSystemSettings(session);
                foreach (var setting in additionalSettings)
                {
                    var dbSetting = session.QueryOver<SystemSettingValue>().Where(s => s.KeyName == setting.Key).SingleOrDefault();
                    //if (SystemSettingValues.ContainsKey(setting.Key))
                    //{
                      object defaultValue = dbSetting?.Value;
                        if (defaultValue == null)
                        {
                            defaultValue = setting.DefaultValue;
                        }
                        var input = InputFieldFactory.CreateInputField(setting, defaultValue);
                        result.Add(input);
                    //}
                }
            }

            return result;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            //SystemSettingValues.Clear();
            using (var session = DataService.OpenSession())
            {
                SystemSettings = session.QueryOver<Models.SystemSettings>().List<Models.SystemSettings>().FirstOrDefault();

                //var additionalSettings = AppSettings.GetAdditionalSystemSettings(session);
                //foreach (var setting in additionalSettings)
                //{
                //    var dbSetting = session.QueryOver<SystemSettingValue>().Where(s => s.KeyName == setting.Key).SingleOrDefault();
                //    SystemSettingValues.Add(setting.Key, dbSetting?.Value);
                //}
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
                var timeOffset = GetValue<int>("TimeOffset");

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
                    if (!String.IsNullOrWhiteSpace(password))
                    {
                        systemSettings.EmailPassword = Encryption.Encrypt(password, AppSettings.ApplicationPassPhrase);
                    }
                    systemSettings.EmailPort = port;
                    systemSettings.EmailEnableSsl = enableSsl;

                    systemSettings.DateFormat = dateFormat;
                    systemSettings.TimeOffset = timeOffset;

                    systemSettings.WebsiteBaseUrl = websiteBaseUrl;

                    DataService.SaveOrUpdate(session, systemSettings);

                    var additionalSettings = AppSettings.GetAdditionalSystemSettings(session);
                    foreach (var setting in additionalSettings)
                    {
                        if (InputData.ContainsKey(setting.Key))
                        {
                            var value = InputData[setting.Key]?.ToString();
                            var dbSetting = session.QueryOver<SystemSettingValue>().Where(s => s.KeyName == setting.Key).SingleOrDefault();
                            if (dbSetting == null)
                            {
                                dbSetting = new SystemSettingValue()
                                {
                                    KeyName = setting.Key
                                };
                            }
                            dbSetting.Value = value;
                            DataService.SaveOrUpdate(session, dbSetting);
                        }
                    }

                    session.Flush();
                }

                WebsiteUtils.DateFormat = dateFormat;

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