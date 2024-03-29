﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace WebsiteTemplate.Backend.Services.Background
{
    public class BackgroundManager
    {
        internal static ManualResetEvent MainEvent { get; set; }
        private static ConcurrentQueue<BackgroundJob> BackgroundWorkerQueue { get; set; }
        private static CancellationTokenSource CancelToken = new CancellationTokenSource();
        public static CancellationToken Token => CancelToken.Token;

        //protected static readonly ILog Logger = SystemLogger.GetLogger<BackgroundManager>();

        private static List<Task> Tasks { get; set; }

        //private BackgroundWorker BackgroundWorker { get; set; }
        private IServiceProvider Container { get; set; }

        static BackgroundManager()
        {
            MainEvent = new ManualResetEvent(false);
            BackgroundWorkerQueue = new ConcurrentQueue<BackgroundJob>();

            Tasks = new List<Task>();
        }

        public BackgroundManager(IServiceProvider container) /* Can't put background service here because it results in circult loop and stack overflow */
        {
            Container = container;
        }

        public void StartWorkers()
        {
            var backgroundWorker = Container.GetService<BackgroundWorker>();
            var numberOfWorkers = 5; // This could come from a setting
            var token = CancelToken.Token;

            for (var i = 0; i < numberOfWorkers; i++)
            {
                //var task = Task.Factory.StartNew(RunWorker, token);
                var task = backgroundWorker.CreateNew(token);
                Tasks.Add(task);
            }
        }

        public static void StopWorkers()
        {
            CancelToken.Cancel();
            MainEvent.Set();

            while (Tasks.Count(t => TaskHasEnded(t) == false) > 0)
            {
                Console.WriteLine("Waiting for tasks to complete");
                Thread.Sleep(1000);
            }
        }

        static bool TaskHasEnded(Task task)
        {
            return task.Status == TaskStatus.Canceled || task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted;
        }

        public static void AddJobToQueue(BackgroundJob job)
        {
            //Logger.Info("adding job " + job.Event.Description + " to backgorund queue");
            BackgroundWorkerQueue.Enqueue(job);
            MainEvent.Set();
            MainEvent.Reset(); // must call reset right after set, else the next call to WaitOne will not block
        }

        public static BackgroundJob Dequeue()
        {
            BackgroundJob result = null;
            BackgroundWorkerQueue.TryDequeue(out result);
            return result;
        }
    }
}