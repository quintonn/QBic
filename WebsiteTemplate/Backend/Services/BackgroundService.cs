﻿using BasicAuthentication.Users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        static BackgroundService()
        {
            BackgroundThreads = new List<Thread>();
        }

        public BackgroundService(DataService dataService, UserContext userContext)
        {
            DataService = dataService;
            UserContext = userContext;

            if (SystemUser == null)
            {
                Setup();
                //AddBackgroundError("Initialization", new Exception("Test"));
            }
            
            //AddBackgroundInformation("Initialization", "Background Service Public Constructor");
        }

        private async void Setup()
        {
            SystemUser = await UserContext.FindUserByNameAsync("System");
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
                        job.Event.DoWork();
                        result.Status = "Success";
                    }
                    catch (Exception e)
                    {
                        result.Status = "Error";
                        result.ExecutionInformation = e.Message + "\n" + e.StackTrace;
                        AddBackgroundError(job.Event.Description, e);
                        //TODO: Log this in file and in a way to display on screen. Maybe i can  do both in 1
                    }
                    AddBackgroundInformation(job.Event.Description, String.Format("Ran background process {0} : {1} -> {2}", job.Event.Description, result.Status, result.ExecutionInformation));
                    
                    SaveBackgroundJobResult(result);
                }
            }
            catch (Exception error)
            {
                if (error is ThreadAbortException)
                {
                    throw;
                }
                AddBackgroundError("Doing BackgroundWork", error);
                Thread.Sleep(TimeSpan.FromSeconds(1));
                BackgroundWork(jobObject);
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

        private void WrappedBackgroundWork(object parameter)
        {
            try
            {
                BackgroundWork(parameter);
            }
            catch (ThreadAbortException exception)
            {
                AddBackgroundError("Thread aborted", exception, false);
            }
        }

        internal void AddBackgroundError(string action, Exception error, bool logInDatabase = true)
        {
            var item = new BackgroundInformation(action, String.Format("Error:\n{0}\n{1}", error.Message, error.StackTrace));
            var currentDirectory = HttpRuntime.AppDomainAppPath;
            var logs = currentDirectory + "\\Logs\\";
            if (!Directory.Exists(logs))
            {
                Directory.CreateDirectory(logs);
            }
            var path = logs + action + "_" + Guid.NewGuid().ToString();
            File.WriteAllText(path, item.Information + "\n" + error.StackTrace);
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