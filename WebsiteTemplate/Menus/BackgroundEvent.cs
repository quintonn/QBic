﻿using System;
using System.Threading;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services.Background;
using WebsiteTemplate.Menus.BaseItems;
using Microsoft.Extensions.DependencyInjection;

namespace WebsiteTemplate.Menus
{
    public abstract class BackgroundEvent : Event
    {
        private BackgroundService BackgroundService { get; set; }

        //TODO: I don't like having to pass container around.
        //      But I can't pass BackgroundService because background service's constructor calls UserManager, which is not yet defined 
        //      when background events are obtained.
        //      A fix could be to ignore background event types when loading events.
        public BackgroundEvent(IServiceProvider container)
        {
            BackgroundService = container.GetService<BackgroundService>();
        }
        public override EventType ActionType
        {
            get
            {
                return EventType.BackgroundEvent;
            }
        }

        public override bool RequiresAuthorization
        {
            get
            {
                return false;
            }
        }

        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public abstract Task DoWork(CancellationToken token);

        /// <summary>
        /// Set this to true if the next run time should only be determined after successfully running doing the work.
        /// Else, set it to false to calculate next run time immediately after starting work.
        /// </summary>
        public virtual bool RunSynchronously
        {
            get
            {
                return false;
            }
        }

        public abstract DateTime CalculateNextRunTime(DateTime? lastRunTime);

        public abstract bool RunImmediatelyFirstTime { get; }

        public void AddBackgroundInfo(string info)
        {
            BackgroundService.AddBackgroundInformation(this.Description, info);
        }

        public void AddBackgroundError(string error)
        {
            BackgroundService.AddBackgroundError(Description, new Exception(error));
        }
    }
}