﻿using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Diagnostics;
using System.Linq;

namespace WebsiteTemplate.Utilities
{
    public class SystemLogger
    {
        private ApplicationSettingsCore AppSettings { get; set; }

        public static ILog GetLogger<T>()
        {
            var typeString = typeof(T).ToString().Split(".".ToCharArray()).Last();
            var result = LogManager.GetLogger(typeString);

            return result;
        }

        public static void LogError<T>(string message, Exception error)
        {
            LogError(message, typeof(T), error);
        }

        public static void LogError(string message, Type callingType, Exception error)
        {
            var typeString = callingType.ToString().Split(".".ToCharArray()).Last();
            var logger = LogManager.GetLogger(typeString);

            logger.Error("An error ocurred: " + message);
            while (error != null)
            {
                logger.Error("\t" + error.Message);
                error = error.InnerException;
            }
        }

        //public static void LogMessage(string message,
        //[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        //[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        //[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        //{
        //    var logger = LogManager.GetLogger(typeof(SystemLogger));

        //    logger.Info("message: " + message);
        //    logger.Info("member name: " + memberName);
        //    logger.Info("source file path: " + sourceFilePath);
        //    logger.Info("source line number: " + sourceLineNumber);
        //}

        public SystemLogger(ApplicationSettingsCore appSettings)
        {
            AppSettings = appSettings;
        }
        public void Setup()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            SetLevel("NHibernate", Level.Error); // Set NHibernate to only log errors

            var patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender();
            roller.AppendToFile = true;
            roller.File = @"Logs\log.txt";
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 10;
            //roller.MaximumFileSize = "1GB"; default is 10MB
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.LockingModel = new FileAppender.MinimalLock();
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            //MemoryAppender memory = new MemoryAppender();
            //memory.ActivateOptions();
            //hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = AppSettings.LogLevel;
            hierarchy.Configured = true;
        }

        public static void SetLevel(string loggerName, Level level)
        {
            var log = LogManager.GetLogger(loggerName);
            var l = (log4net.Repository.Hierarchy.Logger)log.Logger;

            l.Level = level;
        }
    }
}