using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using log4net;

namespace Magpie.Logging.Logger
{
    public class Log : Magpie.Logging.ILog
    {

        private BlockingCollection<LogEntry> LogQueue = new BlockingCollection<LogEntry>();
        private bool disposed = false;
        private Task logQueueProcessorTask;
        private CancellationTokenSource logQueueProcessorTaskCancellationTokenSource = new CancellationTokenSource();

        private int InstitutionID;

        private readonly log4net.ILog log = null;
        
        public string SystemUser { get; set; }

        public Log(Type loggerType)
        {
            log = LogManager.GetLogger(loggerType);
            logQueueProcessorTask = Task.Factory.StartNew(() =>
            {                
                ProcessLogQueue();
            },
            logQueueProcessorTaskCancellationTokenSource.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        }

        private void ProcessLogQueue()
        {
            
            try
            {
                foreach (var LogEntry in LogQueue.GetConsumingEnumerable(logQueueProcessorTaskCancellationTokenSource.Token))
                {
                    //// Adds custom properties to log4Net.
                    ThreadContext.Properties["TennantId"] = LogEntry.TennantId;

                    int logLevel = (int)LogEntry.LoggingLevel;
                    ThreadContext.Properties["LoggingLevel"] = logLevel;

                    ThreadContext.Properties["UserId"] = LogEntry.UserId;  

                    ThreadContext.Properties["TimeStamp"] = DateTime.Now;

                    switch (LogEntry.LoggingLevel)
                    {
                        case LoggingLevels.Debug:
                            log.Debug(LogEntry.Message);
                            break;

                        case LoggingLevels.Error:
                            log.Error(LogEntry.Message);
                            break;

                        case LoggingLevels.Fatal:
                            log.Fatal(LogEntry.Message);
                            break;

                        case LoggingLevels.Info:
                            log.Info(LogEntry.Message);
                            break;

                        case LoggingLevels.Warning:
                            log.Warn(LogEntry.Message);
                            break;
                    }
                }
            }
            catch (System.OperationCanceledException ex)
            {
                // swallow if cacelled...
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (LogQueue != null)
                    LogQueue.Dispose();

                LogQueue = null;
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Debug(string message, string userId, Exception exception = null, int? InstitutionID = null)
        {

            LogEntry e = new LogEntry(LoggingLevels.Debug, message, userId, exception, InstitutionID);            
            LogQueue.Add(e);
            
        }

        public void Error(string message, string userId, Exception exception = null, int? InstitutionID = null)
        {

            LogEntry e = new LogEntry(LoggingLevels.Error, message, userId, exception, InstitutionID);
            LogQueue.Add(e);
            
        }

        public void Fatal(string message, string userId, Exception exception = null, int? InstitutionID = null)
        {

            LogEntry e = new LogEntry(LoggingLevels.Fatal, message, userId, exception, InstitutionID);
            LogQueue.Add(e);
            
        }

        public void Info(string message, string userId, Exception exception = null, int? InstitutionID = null)
        {

            LogEntry e = new LogEntry(LoggingLevels.Info, message, userId, exception, InstitutionID);
            LogQueue.Add(e);
            
        }

        public void Warn(string message, string userId, Exception exception = null, int? InstitutionID = null)
        {

            LogEntry e = new LogEntry(LoggingLevels.Warning, message, userId, exception, InstitutionID);
            LogQueue.Add(e);
            
        }


    }
}
