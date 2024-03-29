﻿using System;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace QBic.Core.Utilities
{
    public class SystemLogger
    {
        private static ILoggerFactory LogFactory { get; set; }
        public static ILogger GetLogger(Type type)
        {
            //var typeString = type.ToString().Split(".".ToCharArray()).Last();
            //var result = LogManager.GetLogger(typeString);

            //return result;
            return LogFactory.CreateLogger(type);
        }
        public static ILogger GetLogger<T>()
        {
            //var typeString = typeof(T).ToString().Split(".".ToCharArray()).Last();
            //var result = LogManager.GetLogger(typeString);

            //return result;
            return GetLogger(typeof(T));
        }

        public static string GetMessageStack(Exception exception)
        {
            var result = String.Empty;
            while (exception != null)
            {
                result = result  + exception.Message + "\n";

                exception = exception.InnerException;
            }

            return result.Trim();
        }

        public static void LogError<T>(string message, Exception error)
        {
            LogError(message, typeof(T), error);
        }

        public static void LogError(string message, Type callingType, Exception error)
        {
            var typeString = callingType.ToString().Split(".".ToCharArray()).Last();
            var logger = LogFactory.CreateLogger(typeString);

            logger.LogError("An error ocurred: " + message);
            while (error != null)
            {
                logger.LogError("\t" + error.Message);
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

        public static void Setup(ILoggerFactory loggerFactory)
        {
            LogFactory = loggerFactory;
            //var hierarchy = (Hierarchy)LogManager.GetRepository();

            //SetLevel("NHibernate", Level.Error); // Set NHibernate to only log errors

            //var patternLayout = new PatternLayout();
            //patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            //patternLayout.ActivateOptions();

            //var roller = new RollingFileAppender();
            //roller.AppendToFile = true;
            //roller.File = @"Logs\log.txt";
            //roller.Layout = patternLayout;
            //roller.MaxSizeRollBackups = 10;
            ////roller.MaximumFileSize = "1GB"; default is 10MB
            //roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            //roller.StaticLogFileName = false;
            //roller.LockingModel = new FileAppender.MinimalLock();
            //roller.ActivateOptions();
            //hierarchy.Root.AddAppender(roller);

            ////MemoryAppender memory = new MemoryAppender();
            ////memory.ActivateOptions();
            ////hierarchy.Root.AddAppender(memory);

            //hierarchy.Root.Level = logLevel;
            //hierarchy.Configured = true;
        }

        //public static void SetLevel(string loggerName, Level level)
        //{
        //    var log = LogManager.GetLogger(loggerName);
        //    var l = (log4net.Repository.Hierarchy.Logger)log.Logger;

        //    l.Level = level;
        //}
    }
}