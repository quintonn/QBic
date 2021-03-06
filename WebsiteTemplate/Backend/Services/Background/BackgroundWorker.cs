﻿using QBic.Core.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services.Background
{
    public class BackgroundWorker
    {
        private BackgroundService BackgroundService { get; set; }

        //protected static readonly ILog Logger = SystemLogger.GetLogger<BackgroundWorker>();

        public BackgroundWorker(BackgroundService backgroundService)
        {
            BackgroundService = backgroundService;
        }

        //public CancellationToken CancelToken { get; set; }

        public Task CreateNew(CancellationToken token)
        {
            //CancelToken = token;
            //var task = Task.Factory.StartNew(Run, token);
            var task = Task.Run(async () => await Run(token), token);

            return task;
        }

        async Task Run(CancellationToken token)
        {
            //Logger.Info("Running background worker " + Thread.CurrentThread.ManagedThreadId);
            while (true)
            {
                //Logger.Info("WaitOne in " + Thread.CurrentThread.ManagedThreadId);
                BackgroundManager.MainEvent.WaitOne();
                //Logger.Info("WaitOne passed in" + Thread.CurrentThread.ManagedThreadId);

                if (token.IsCancellationRequested)
                {
                    //Logger.Info("Cancellation token received for " + Thread.CurrentThread.ManagedThreadId);
                    token.ThrowIfCancellationRequested(); // sets task status to cancel
                    break; // or sets task as RanToCompletion (i like this option).
                }

                //object item;
                //Logger.Info("Dequeing background job " + Thread.CurrentThread.ManagedThreadId);
                var backgroundJob = BackgroundManager.Dequeue();
                if (backgroundJob == null)
                {
                    //Logger.Info("Background job is null for " + Thread.CurrentThread.ManagedThreadId);

                    break;
                }
                while (backgroundJob != null)
                {
                    //Thread.Sleep(1000);
                    //Console.WriteLine("Main  event doing work 1 time - " + Thread.CurrentThread.ManagedThreadId);

                    if (backgroundJob != null)
                    {
                        //Logger.Info("Doing work in " + Thread.CurrentThread.ManagedThreadId + " for " + backgroundJob.Event.Description);
                        await DoWork(backgroundJob, token);
                        backgroundJob = BackgroundManager.Dequeue();
                    }
                    else
                    {
                        //Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " got nothing");
                    }
                }
            }
        }

        public async Task DoWork(BackgroundJob job, CancellationToken token)
        {
            var result = new BackgroundJobResult()
            {
                EventNumber = job.EventNumber,
                DateTimeRunUTC = DateTime.UtcNow
            };

            try
            {
                await job.Event.DoWork(token);
                result.Status = "Success";

                BackgroundService.AddBackgroundInformation(job.Event.Description, String.Format("Ran background process {0} : {1} -> {2}", job.Event.Description, result.Status, result.ExecutionInformation));
            }
            catch (Exception error)
            {
                SystemLogger.LogError<BackgroundWorker>("Error doing background worker work", error);
                result.Status = "Error: " + job.Event.Description;
                result.ExecutionInformation = error.Message + "\n" + error.StackTrace;
                BackgroundService.AddBackgroundError(job.Event.Description, error);
            }

            //BackgroundService.AddBackgroundInformation(job.Event.Description, String.Format("Ran background process {0} : {1} -> {2}", job.Event.Description, result.Status, result.ExecutionInformation));

            //BackgroundService.SaveBackgroundJobResult(result); /* I don't have a view of this yet */
        }
    }
}