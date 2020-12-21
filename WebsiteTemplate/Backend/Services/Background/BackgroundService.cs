using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QBic.Authentication;
using QBic.Core.Services;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services.Background
{
    public class BackgroundService : IDisposable
    {
        private DataService DataService { get; set; }
        private UserManager<IUser> UserContext { get; set; }
        private BackgroundManager BackgroundManager { get; set; }

        private static object Locker = new object();
        public static bool Started { get; set; }

        protected static readonly ILogger Logger = SystemLogger.GetLogger<BackgroundService>();
        private CancellationToken CancelToken { get; set; }

        static BackgroundService()
        {
            BackgroundThreads = new List<Task>();
            Started = false;
        }

        public BackgroundService(DataService dataService, UserManager<IUser> userContext, BackgroundManager manager)
        {
            DataService = dataService;
            UserContext = userContext;
            BackgroundManager = manager;
            CancelToken = BackgroundManager.Token;

            if (Started == false)
            {
                Setup();
            }
        }

        private async void Setup()
        {
            Logger.LogDebug("Starting background service");
            SystemUser = await UserContext.FindByNameAsync("System");
            Started = true;
        }

        private static List<Task> BackgroundThreads { get; set; }
        private static List<BackgroundJob> BackgroundJobs { get; set; }
        private static IUser SystemUser { get; set; }
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

        /// <summary>
        /// This methods runs for each registered background process and either runs it or adds it to the queue of jobs to run.
        /// This runs an infinite while loop for all background events and sleeps the amount of time determined by CalculateNextRunTime.
        /// Then it either executes the event or adds it to the queue of jobs to be done.
        /// The difference is that the first will wait for the result before sleeping the time allocated.
        /// Whereas the second will schedule the job as soon as it is done sleeping.
        /// </summary>
        /// <param name="jobObject"></param>
        private async Task RunBackgroundEventLoop(object jobObject)
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
                    await worker.DoWork(job, CancelToken);
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
                //var thread = new Thread(new ParameterizedThreadStart(RunBackgroundEventLoop));
                var task = Task.Run(async () => RunBackgroundEventLoop(backgroundJob));
                BackgroundThreads.Add(task);
                //thread.Start(backgroundJob);
            }
            BackgroundManager.StartWorkers(); // starts background timers that process jobs in the queue
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
            Logger.LogDebug(task + "\n" + statusInfo);
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
                BackgroundManager.StopWorkers(); // this call cancellation token . cancel
                
                //if (BackgroundThreads != null)
                //{
                //    foreach (var t in BackgroundThreads)
                //    {
                //        //if (t.IsAlive)
                //        //{
                //        //    t.Abort();
                //        //}
                //        if (t != null)
                //        {
                //            t.c
                //        }
                //    }
                //}
            }
            catch (Exception e)
            {
                AddBackgroundError("dispose of background service", e);
            }
        }
    }
}