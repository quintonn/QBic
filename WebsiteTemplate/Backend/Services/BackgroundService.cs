using BasicAuthentication.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class BackgroundService : IDisposable
    {
        private DataService DataService { get; set; }

        internal static List<string> Errors { get; set; }
        internal static List<string> StatusInfo { get; set; }

        private static object Locker = new object();

        internal static void AddError(string action, Exception error)
        {
            Errors.Add(string.Format("Error while performing {0}\n{1}\n{2}", action, error.Message, error.StackTrace));
        }

        static BackgroundService()
        {
            Setup();
            Errors = new List<string>();
            StatusInfo = new List<string>();
            BackgroundThreads = new List<Thread>();
        }

        public BackgroundService(DataService dataService)
        {
            DataService = dataService;
        }

        private static async void Setup()
        {
            SystemUser = await CoreAuthenticationEngine.UserManager.FindByNameAsync("System") as User;
        }

        private static List<Thread> BackgroundThreads { get; set; }
        private static List<BackgroundJob> BackgroundJobs { get; set; }
        private static User SystemUser { get; set; }
        private void SaveBackgroundJobStatus(BackgroundJobResult jobResult)
        {
            try
            {
                using (var session = DataService.OpenSession())
                {
                    DataService.SaveOrUpdate(session, jobResult, SystemUser);
                    session.Flush();
                }
            }
            catch (Exception e)
            {
                BackgroundService.AddError("Saving background job status", e);
            }
        }

        private void InitializeBackgroundJobs()
        {
            BackgroundJobs = EventService.BackgroundEventList.Select(b => new BackgroundJob()
            {
                Event = b.Value,
            }).ToList();
            using (var session = DataService.OpenSession())
            {
                foreach (var job in BackgroundJobs)
                {
                    var lastJob = session.QueryOver<BackgroundJobResult>().Where(b => b.EventNumber == job.Event.GetEventId()).OrderBy(e => e.DateTimeRunUTC).Desc.Take(1).SingleOrDefault();
                    if (lastJob != null)
                    {
                        job.LastRunTime = lastJob.DateTimeRunUTC?.ToLocalTime();
                    }
                }
            }
        }

        private void BackgroundWork(object jobObject)
        {
            var job = (BackgroundJob)jobObject;
            var firstTime = true;
            try
            {
                while (true)
                {
                    if (firstTime && job.Event.RunImmediatelyFirstTime)
                    {
                        firstTime = false;
                    }
                    else
                    {
                        /* First calculate the amount of time to wait before doing work */
                        job.NextRunTime = job.Event.CalculateNextRunTime(job.LastRunTime);
                        var sleepTime = job.NextRunTime.Subtract(DateTime.Now);
                        AddToStatusInfo(String.Format("Background process {0} is going to sleep for {1} days, {2} hours, {3} minutes and {4} seconds", job.Event.Description, sleepTime.Days, sleepTime.Hours, sleepTime.Minutes, sleepTime.Seconds), job);
                        Thread.Sleep(sleepTime);
                    }

                    var result = new BackgroundJobResult()
                    {
                        EventNumber = job.Event.GetEventId(),
                        DateTimeRunUTC = DateTime.UtcNow
                    };
                    job.LastRunTime = DateTime.Now;
                    try
                    {
                        job.Event.DoWork();
                        result.Status = "Success";
                    }
                    catch (Exception e)
                    {
                        result.Status = "Error";
                        result.ExecutionInformation = e.Message + "\n" + e.StackTrace;
                        //TODO: Log this in file and in a way to display on screen. Maybe i can  do both in 1
                    }
                    AddToStatusInfo(String.Format("Ran background process {0} : {1} -> {2}", job.Event.Description, result.Status, result.ExecutionInformation), job);
                    
                    SaveBackgroundJobStatus(result);
                }
            }
            catch (Exception error)
            {
                BackgroundService.AddError("Doing BackgroundWork", error);
            }
        }
        public async void StartBackgroundJobs()
        {
            if (BackgroundJobs == null)
            {
                InitializeBackgroundJobs();
            }
            foreach (var backgroundJob in BackgroundJobs)
            {
                var thread = new Thread(new ParameterizedThreadStart(BackgroundWork));
                BackgroundThreads.Add(thread);
                thread.Start(backgroundJob);
            }
            //BackgroundThread = new Thread(new ThreadStart(BackgroundWork));
            //BackgroundThread.Start();
        }

        private static void AddToStatusInfo(string statusInfo, BackgroundJob job)
        {
            lock(Locker)
            {
                var date = ((DateTime)job.LastRunTime);
                var timePart = date.ToShortDateString() + "  " + date.ToLongTimeString();
                StatusInfo.Add(timePart + "\t" + statusInfo);
            }
        }

        public void Dispose()
        {
            try
            {
                if (BackgroundThreads != null)
                {
                    foreach (var t in BackgroundThreads)
                    {
                        if (t.IsAlive)
                        {
                            t.Abort();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                BackgroundService.AddError("dispose of background service", e);
            }
        }
    }
}