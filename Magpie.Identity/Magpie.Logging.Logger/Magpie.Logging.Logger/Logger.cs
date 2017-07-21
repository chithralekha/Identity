using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Log4Net.config", Watch = true)]

namespace Magpie.Logging.Logger
{
    using System;
    using System.Threading.Tasks;
    using log4net;



    public class Logger
    {


        public enum LogType
        {
            Debug = 0,
            Error,
            Fatal,
            Info,
            Warn
        }

        private static Logger instance = null;

        private readonly ILog log = null;

        private Logger()
        {
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger
        //    (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }

                return instance;
            }
        }

        /// <summary>
        /// Log a message and/or exception asynchronously.
        /// This simply uses a "fire-and-forget" async call to
        /// call the LogMessage method.
        /// <remarks>
        /// See the Log4Net.config file for details about
        /// the logging.
        /// </remarks>
        /// <param name="logType">Type of log message (Debug, Error, Fatal, Info, Warn).</param>
        /// <param name="tennantId">The InstitutionID.</param>        
        /// <param name="message">Message to log.</param>
        /// <param name="exception">Exception to log.</param>
        public void LogMessageAsync(LogType logType, int tennantId, string message, Exception exception = null)
        {
            try
            {
                Task.Run(() =>
                {
                    //// Adds custom properties to log4Net.
                    ThreadContext.Properties["TennantId"] = tennantId;

                    switch (logType)
                    {
                        case LogType.Debug:                            
                            log.Debug(message, exception);
                            break;

                        case LogType.Error:                            
                            log.Error(message, exception);
                            break;

                        case LogType.Fatal:                            
                            log.Fatal(message, exception);
                            break;

                        case LogType.Info:
                            log.Info(message, exception);
                            break;

                        case LogType.Warn:
                            log.Warn(message, exception);
                            break;
                    }
                });
            }
            catch (Exception e)
            {
                string strMessage = e.Message;
            }
        }
    }
}
