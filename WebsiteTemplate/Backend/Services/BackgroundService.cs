using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class BackgroundService : IDisposable
    {
        private DataService DataService { get; set; }
        private UserContext UserContext { get; set; }

        private static object Locker = new object();
        public static bool Started { get; set; }

        static BackgroundService()
        {
            BackgroundThreads = new List<Thread>();
            Started = false;
        }

        public BackgroundService(DataService dataService, UserContext userContext)
        {
            DataService = dataService;
            UserContext = userContext;

            if (Started == false)
            {
                Setup();
            }
        }

        private async void Setup()
        {
            SystemUser = await UserContext.FindUserByNameAsync("System");
            Started = true;
        }

        private static List<Thread> BackgroundThreads { get; set; }
        private static List<BackgroundJob> BackgroundJobs { get; set; }
        private static User SystemUser { get; set; }
        private void SaveBackgroundJobResult(BackgroundJobResult jobResult)
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

        private void InitializeBackgroundJobs()
        {
            AddBackgroundInformation("Background jobs", "Initialize background jobs 1.");
            BackgroundJobs = EventService.BackgroundEventList.Select(b => new BackgroundJob()
            {
                Event = b.Value,
            }).ToList();
            AddBackgroundInformation("Background jobs", "Initialize background jobs 2.");
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
                        AddBackgroundError("error in initializing background jobs: "+job.Event.Description, e);
                    }
                }
            }
            AddBackgroundInformation("Background jobs", "Initialize background jobs 10.");
        }

        private void BackgroundWork(object jobObject)
        {
            AddBackgroundInformation("Background jobs", "Background work 1");
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
                        AddBackgroundInformation(job.Event.Description, String.Format("Background process {0} is going to sleep for {1} days, {2} hours, {3} minutes and {4} seconds", job.Event.Description, sleepTime.Days, sleepTime.Hours, sleepTime.Minutes, sleepTime.Seconds));
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
                        if (BackupService.BusyWithBackups == true)
                        {
                            continue;
                        }
                        job.Event.DoWork();
                        result.Status = "Success";
                    }
                    catch (Exception e)
                    {
                        //result.Status = "Error";
                        //result.ExecutionInformation = e.Message + "\n" + e.StackTrace;
                        //AddBackgroundError(job.Event.Description, e);
                        throw;
                        //TODO: Log this in file and in a way to display on screen. Maybe i can  do both in 1
                    }
                    AddBackgroundInformation(job.Event.Description, String.Format("Ran background process {0} : {1} -> {2}", job.Event.Description, result.Status, result.ExecutionInformation));
                    
                    SaveBackgroundJobResult(result);
                }
            }
            catch (Exception error)
            {
                AddBackgroundError("Doing BackgroundWork", error);

                // Try fix background stopping
                Started = false;

                ///if (error is ThreadAbortException)
                {
                    //Thread.CurrentThread.Abort();
                    var index = BackgroundThreads.IndexOf(Thread.CurrentThread);
                    if (index > -1)
                    {
                        var backgroundJob = BackgroundJobs[index];
                        var thread = new Thread(new ParameterizedThreadStart(BackgroundWork));
                        BackgroundThreads.Add(thread);
                        thread.Start(backgroundJob);

                        BackgroundThreads[index] = thread;
                    }
                }
                
                //Thread.Sleep(TimeSpan.FromSeconds(1));
                //BackgroundWork(jobObject);
            }
        }

        public async void StartBackgroundJobs()
        {
            AddBackgroundInformation("Background jobs", "Starting background jobs 1");
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
            AddBackgroundInformation("Background jobs", "Starting background jobs 2");
            foreach (var backgroundJob in BackgroundJobs)
            {
                var thread = new Thread(new ParameterizedThreadStart(BackgroundWork));
                BackgroundThreads.Add(thread);
                thread.Start(backgroundJob);
            }
            AddBackgroundInformation("Background jobs", "Starting background jobs 10");
        }

        internal void AddBackgroundError(string action, Exception error, bool logInDatabase = true)
        {
            if (BackupService.BusyWithBackups == true)
            {
                return;
            }

            //Need to do something about thread being aborted exception

            var stackTrace = new System.Diagnostics.StackTrace();
            var stack = stackTrace.GetFrame(1).GetMethod().Name;
            var item = new BackgroundInformation(action, String.Format("Error:\n{0}\n{1}\n{2}\n{3}", error.Message, error.StackTrace, stack, stackTrace));
            var currentDirectory = HttpRuntime.AppDomainAppPath;
            var logs = currentDirectory + "\\Logs\\";
            if (!Directory.Exists(logs))
            {
                Directory.CreateDirectory(logs);
            }
            var path = logs + action + "_" + Guid.NewGuid().ToString();
            File.WriteAllText(path, item.Information + "\n" + error.StackTrace);

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