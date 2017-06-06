﻿using Microsoft.Practices.Unity;
using System;
using System.Linq;
using System.Net;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.KeepAlive
{
    public class KeepAliveBackgroundEvent //: BackgroundEvent
    {
        private DataService DataService { get; set; }

        public KeepAliveBackgroundEvent(DataService dataService, IUnityContainer container)
            //:base(container)
        {
            DataService = dataService;
        }

        public /*override*/ string Description
        {
            get
            {
                return "Keep alive background task";
            }
        }

        public /*override*/ DateTime CalculateNextRunTime(DateTime? lastRunTime)
        {
            //return DateTime.Now.AddMinutes(2);
            return DateTime.Now.AddMinutes(60);
        }

        public /*override*/ bool RunImmediatelyFirstTime
        {
            get
            {
                return false;
            }
        }

        public /*override*/ void DoWork()
        {
            var url = String.Empty;

            using (var session = DataService.OpenSession())
            {
                var systemSettings = session.QueryOver<Models.SystemSettings>().List<Models.SystemSettings>().FirstOrDefault();
                url = systemSettings?.WebsiteBaseUrl;
            }

            if (!String.IsNullOrWhiteSpace(url))
            {
                if (url.EndsWith("/"))
                {
                    url = url.Substring(0, url.Length - 1);
                }
                url = url + "/api/v1/systemPing";
                //TODO: remove later
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                using (var wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate, br";
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=UTF-8";

                    var internalSecret = "";

                    // The following code can be used to ignore invalid certificates, but it should not be used in production
                    //ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    //{
                    //    return true;
                    //};

                    var bytes = wc.UploadData(url, XXXUtils.GetBytes(internalSecret));
                    //bytes = CompressionHelper.InflateByte(bytes);
                    //var resp = XXXUtils.GetString(bytes);


                    //Console.WriteLine(resp);
                }
            }
        }

        public /*override*/ EventNumber GetId()
        {
            return EventNumber.KeepAliveBackgroundTask;
        }
    }
}