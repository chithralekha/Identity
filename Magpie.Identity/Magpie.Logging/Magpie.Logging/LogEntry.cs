using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magpie.Logging
{
    public struct LogEntry
    {
        private LoggingLevels loggingLevel;

        public LoggingLevels LoggingLevel { get { return loggingLevel; } }

        private string message;

        public string Message { get { return message; } }

        private int? tennantId;

        public int? TennantId { get { return tennantId; } }
        
        private string userId;

        public string UserId
        {
            get { return userId;  }
        }

        private Exception exception;

        public Exception Exception { get { return exception; } }

        public LogEntry(LoggingLevels LoggingLevel,  string Message, string UserId)
        {
            loggingLevel = LoggingLevel;
            message = Message;
            tennantId = null;
            exception = null;
            userId = UserId;
        }

        public LogEntry(LoggingLevels LoggingLevel, string Message, string UserId, int? TennantId)
        {
            loggingLevel = LoggingLevel;
            message = Message;
            tennantId = TennantId;
            exception = null;
            userId = UserId;
        }

        public LogEntry(LoggingLevels LoggingLevel, string Message, string UserId, Exception Exception)
        {
            loggingLevel = LoggingLevel;
            message = Message;
            tennantId = null;
            exception = Exception;
            userId = UserId;
        }

        public LogEntry(LoggingLevels LoggingLevel, string Message, string UserId, Exception Exception, int? TennantId)
        {
            loggingLevel = LoggingLevel;
            message = Message;
            tennantId = TennantId;
            exception = Exception;
            userId = UserId;
        }
    }
}
