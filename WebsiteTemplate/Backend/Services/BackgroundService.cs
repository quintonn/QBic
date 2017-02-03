﻿using BasicAuthentication.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class BackgroundService : IDisposable
    {
        private DataService DataService { get; set; }

        internal static List<BackgroundInformation> Errors { get; set; }
        internal static List<BackgroundInformation> StatusInfo { get; set; }

        private static object Locker = new object();

        static BackgroundService()
        {
            StatusInfo = new List<BackgroundInformation>();
            StatusInfo.Add(new BackgroundInformation("Initialization", "Background Service Static Constructor"));
            Setup();
            Errors = new List<BackgroundInformation>();
            
            BackgroundThreads = new List<Thread>();
            StatusInfo.Add(new BackgroundInformation("Initialization", "Background Service Static Constructor end"));
        }

        public BackgroundService(DataService dataService)
        {
            DataService = dataService;
            StatusInfo.Add(new BackgroundInformation("Initialization", "Background Service Public Constructor"));
        }

        private static async void Setup()
        {
            StatusInfo.Add(new BackgroundInformation("Setup", "Background Service Setup"));
            SystemUser = await CoreAuthenticationEngine.UserManager.FindByNameAsync("System") as User;
            StatusInfo.Add(new BackgroundInformation("Setup", "Background Service Setup End"));
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
            AddToStatusInfo("Background jobs", "Initialize background jobs 1.");
            BackgroundJobs = EventService.BackgroundEventList.Select(b => new BackgroundJob()
            {
                Event = b.Value,
            }).ToList();
            AddToStatusInfo("Background jobs", "Initialize background jobs 2.");
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
                        AddError("error in initializing background jobs: "+job.Event.Description, e);
                    }
                }
            }
            AddToStatusInfo("Background jobs", "Initialize background jobs 10.");
        }

        private void BackgroundWork(object jobObject)
        {
            AddToStatusInfo("Background jobs", "Background work 1");
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
                        AddToStatusInfo(job.Event.Description, String.Format("Background process {0} is going to sleep for {1} days, {2} hours, {3} minutes and {4} seconds", job.Event.Description, sleepTime.Days, sleepTime.Hours, sleepTime.Minutes, sleepTime.Seconds));
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
                        AddError(job.Event.Description, e);
                        //TODO: Log this in file and in a way to display on screen. Maybe i can  do both in 1
                    }
                    AddToStatusInfo(job.Event.Description, String.Format("Ran background process {0} : {1} -> {2}", job.Event.Description, result.Status, result.ExecutionInformation));
                    
                    SaveBackgroundJobStatus(result);
                }
            }
            catch (Exception error)
            {
                BackgroundService.AddError("Doing BackgroundWork", error);
            }
        }

        //public static Task Delay(double milliseconds)
        //{
        //    var tcs = new TaskCompletionSource<bool>();
        //    System.Timers.Timer timer = new System.Timers.Timer();
        //    timer.Elapsed += (obj, args) =>
        //    {
        //        tcs.TrySetResult(true);
        //    };
        //    timer.Interval = milliseconds;
        //    timer.AutoReset = false;
        //    timer.Start();
        //    return tcs.Task;
        //}

        public async void StartBackgroundJobs()
        {
            AddToStatusInfo("Background jobs", "Starting background jobs 1");
            if (BackgroundJobs == null)
            {
                try
                {
                    InitializeBackgroundJobs();
                }
                catch (Exception e)
                {
                    AddError("Starting error", e);
                }
            }
            AddToStatusInfo("Background jobs", "Starting background jobs 2");
            foreach (var backgroundJob in BackgroundJobs)
            {
                var thread = new Thread(new ParameterizedThreadStart(BackgroundWork));
                BackgroundThreads.Add(thread);
                thread.Start(backgroundJob);
            }
            AddToStatusInfo("Background jobs", "Starting background jobs 10");
            //BackgroundThread = new Thread(new ThreadStart(BackgroundWork));
            //BackgroundThread.Start();
        }

        internal static void AddError(string action, Exception error)
        {
            Errors.Add(new BackgroundInformation(action, String.Format("Error:\n{0}\n{1}", error.Message, error.StackTrace)));
        }

        private static void AddToStatusInfo(string task, string statusInfo)
        {
            lock(Locker)
            {
                try
                {
                    //var date = DateTime.Now;
                    //var timePart = date.ToShortDateString() + "  " + date.ToLongTimeString();
                    StatusInfo.Add(new BackgroundInformation(task, statusInfo));
                    //StatusInfo.Add(timePart + "\t" + statusInfo);
                }
                catch (Exception e)
                {
                    StatusInfo.Add(new BackgroundInformation(task, "Error:\n" + e.Message));
                }
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