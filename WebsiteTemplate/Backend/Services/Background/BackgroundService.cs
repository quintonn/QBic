using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Services.Background
{
    public class BackgroundService : IDisposable
    {
        private DataService DataService { get; set; }
        private UserContext UserContext { get; set; }
        private BackgroundManager BackgroundManager { get; set; }

        private static object Locker = new object();
        public static bool Started { get; set; }

        protected static readonly ILog Logger = SystemLogger.GetLogger<BackgroundService>();

        static BackgroundService()
        {
            BackgroundThreads = new List<Thread>();
            Started = false;
        }

        public BackgroundService(DataService dataService, UserContext userContext, BackgroundManager manager)
        {
            DataService = dataService;
            UserContext = userContext;
            BackgroundManager = manager;

            if (Started == false)
            {
                Setup();
            }
        }

        private async void Setup()
        {
            Logger.Debug("Starting background service");
            SystemUser = await UserContext.FindUserByNameAsync("System");
            Started = true;
        }

        private static List<Thread> BackgroundThreads { get; set; }
        private static List<BackgroundJob> BackgroundJobs { get; set; }
        private static User SystemUser { get; set; }
        internal void SaveBackgroundJobResult(BackgroundJobResult jobResult)
        {
            lock (Locker)
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
                    AddBackgroundError("Saving background job status", e);
                }
            }
        }

        private void InitializeBackgroundJobs()
        {
            AddBackgroundInformation("Background jobs", "Initializing background jobs");
            BackgroundJobs = EventService.BackgroundEventList.Select(b => new BackgroundJob()
            {
                Event = b.Value,
            }).ToList();
            //AddBackgroundInformation("Background jobs", "Initialize background jobs 2.");
            using (var session = DataService.OpenSession())
            {
                foreach (var job in BackgroundJobs)
                {
                    try
                    {
                        var lastJob = session.QueryOver<BackgroundJobResult>().Where(b => b.EventNumber == job.Event.GetEventId()).OrderBy(e => e.DateTimeRunUTC).Desc.Take(1).SingleOrDefault();
                        if (lastJob != null && lastJob.DateTimeRunUTC != null)
                        {
                            job.LastRunTime = lastJob.DateTimeRunUTC?.ToLocalTime();
                        }
                    }
                    catch (Exception e)
                    {
                        AddBackgroundError("Error in initializing background jobs: "+job.Event.Description, e);
                    }
                }
            }
            AddBackgroundInformation("Background jobs", "Background jobs initialized successfully");
        }

        private void BackgroundWork(object jobObject)
        {
            //AddBackgroundInformation("Background jobs", "Background work 1");
            var job = (BackgroundJob)jobObject;
            job.EventNumber = job.Event.GetEventId();
            var firstTime = true;

            BackgroundWorker worker = null;
            if (job.Event.RunSynchronously == true)
            {
                worker = new BackgroundWorker(this);
            }

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

                    //TODO: This might not be what we want.
                    //      The next run time might depend on the result of previous run
                    
                    AddBackgroundInformation(job.Event.Description, String.Format("Background process {0} is going to sleep until {1}", job.Event.Description, job.NextRunTime));
                    var sleepTime = job.NextRunTime.Subtract(DateTime.Now); /* Do this after adding background info because it takes time too */
                    if (sleepTime < TimeSpan.FromSeconds(0))
                    {
                        sleepTime = TimeSpan.FromMinutes(1);
                    }

                    Thread.Sleep(sleepTime);
                }


                job.LastRunTime = DateTime.Now;
                if (BackupService.BusyWithBackups == true)
                {
                    continue;
                }

                if (job.Event.RunSynchronously == true)
                {
                    worker.DoWork(job);
                }
                else
                {
                    BackgroundManager.AddJobToQueue(job); 
                }
            }
        }

        public async void StartBackgroundJobs()
        {
            AddBackgroundInformation("Background jobs", "Starting background jobs");
            if (BackgroundJobs == null)
            {
                try
                {
                    InitializeBackgroundJobs();
                }
                catch (Exception e)
                {
                    AddBackgroundError("Starting error", e);
                }
            }
            //AddBackgroundInformation("Background jobs", "Starting background jobs 2");
            foreach (var backgroundJob in BackgroundJobs)
            {
                var thread = new Thread(new ParameterizedThreadStart(BackgroundWork));
                BackgroundThreads.Add(thread);
                thread.Start(backgroundJob);
            }
            BackgroundManager.StartWorkers();
            AddBackgroundInformation("Background jobs", "Background jobs started");
        }

        internal void AddBackgroundError(string action, Exception error, bool logInDatabase = true)
        {
            SystemLogger.LogError<BackgroundService>(action, error);
            if (BackupService.BusyWithBackups == true)
            {
                return;
            }

            //Need to do something about thread being aborted exception

            var stackTrace = new System.Diagnostics.StackTrace();
            var stack = stackTrace.GetFrame(1).GetMethod().Name;
            var item = new BackgroundInformation(action, String.Format("Error:\n{0}\n{1}\n{2}\n{3}", error.Message, error.StackTrace, stack, stackTrace));
            //var currentDirectory = HttpRuntime.AppDomainAppPath;
            //var logs = currentDirectory + "\\Logs\\";
            //if (!Directory.Exists(logs))
            //{
            //    Directory.CreateDirectory(logs);
            //}
            //var path = logs + action + "_" + Guid.NewGuid().ToString();
            //File.WriteAllText(path, item.Information + "\n" + error.StackTrace);

            if (error is ThreadAbortException)
            {
                return;
            }

            if (logInDatabase == true)
            {
                using (var session = DataService.OpenSession())
                {
                    session.Save(item);
                    session.Flush();
                }
            }
        }

        internal void AddBackgroundInformation(string task, string statusInfo)
        {
            Logger.Debug(task + "\n" + statusInfo);
            if (BackupService.BusyWithBackups == true)
            {
                return;
            }
            lock (Locker)
            {
                var item = new BackgroundInformation(task, statusInfo);
                using (var session = DataService.OpenSession())
                {
                    session.Save(item);
                    session.Flush();
                }
            }
        }

        public void Dispose()
        {
            try
            {
                Started = false;
                BackgroundManager.StopWorkers();
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
                AddBackgroundError("dispose of background service", e);
            }
        }
    }
}