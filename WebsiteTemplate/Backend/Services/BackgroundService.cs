using BasicAuthentication.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class BackgroundService : IDisposable
    {
        private DataService DataService { get; set; }

        static BackgroundService()
        {
            Setup();
        }

        public BackgroundService(DataService dataService)
        {
            DataService = dataService;
        }

        private static async void Setup()
        {
            SystemUser = await CoreAuthenticationEngine.UserManager.FindByNameAsync("System") as User;
        }

        private static Thread BackgroundThread { get; set; }
        private static List<BackgroundJob> BackgroundJobs { get; set; }
        private static User SystemUser { get; set; }

        private List<BackgroundJobResult> RunBackgroundTasks()
        {
            var list = new List<BackgroundJobResult>();
            foreach (var job in BackgroundJobs.Where(b => b.WillRunNext == true))
            {
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
                }
                list.Add(result);
            }
            return list;
        }

        private void SaveBackgroundJobStatus(List<BackgroundJobResult> list)
        {
            using (var session = DataService.OpenSession())
            {
                foreach (var item in list)
                {
                    DataService.SaveOrUpdate(session, item, SystemUser);
                }
                session.Flush();
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
            /*Parallel.ForEach(BackgroundJobs, (job) =>*/
            foreach (var job in BackgroundJobs)
            {
                job.NextRunTime = job.Event.CalculateNextRunTime(job.LastRunTime);
            }
        }

        private void BackgroundWork()
        {
            if (BackgroundJobs == null)
            {
                InitializeBackgroundJobs();
            }

            while (true)
            {
                var list = RunBackgroundTasks();

                SaveBackgroundJobStatus(list);

                var currentTime = DateTime.Now;

                // Calculate next time to run for completed or past jobs.
                foreach (var job in BackgroundJobs.Where(b => b.WillRunNext == true || b.NextRunTime < currentTime))
                {
                    job.WillRunNext = false;
                    job.NextRunTime = job.Event.CalculateNextRunTime(job.LastRunTime);
                }

                // Find soonest next job to run
                var nextTime = currentTime.AddDays(1000);
                foreach (var job in BackgroundJobs)
                {
                    if (job.NextRunTime < nextTime && job.NextRunTime > currentTime)
                    {
                        nextTime = job.NextRunTime;
                    }
                }
                foreach (var job in BackgroundJobs.Where(j => j.NextRunTime == nextTime))
                {
                    job.WillRunNext = true;
                }

                // Sleep until next soonest job is scheduled to run.
                var sleepTime = nextTime.Subtract(DateTime.Now);

                Thread.Sleep(sleepTime);
            }
        }
        public async void StartBackgroundJobs()
        {
            BackgroundThread = new Thread(new ThreadStart(BackgroundWork));
            BackgroundThread.Start();
        }

        public void Dispose()
        {
            if (BackgroundThread != null && BackgroundThread.IsAlive)
            {
                BackgroundThread.Abort();
            }
        }
    }
}