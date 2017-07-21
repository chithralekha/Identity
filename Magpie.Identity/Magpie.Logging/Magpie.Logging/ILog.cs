using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Logging
{
    public enum LoggingLevels { Debug, Error, Fatal, Info, Warning }
    
    public interface ILog
    {
        void Debug(string Message, string UserId,  Exception Exception = null, int? TennantId = null);
        void Error(string Message, string UserId, Exception Exception = null, int? TennantId = null);
        void Fatal(string Message, string UserId, Exception Exception = null, int? TennantId = null);
        void Info(string Message, string UserId, Exception Exception = null, int? TennantId = null);
        void Warn(string Message, string UserId, Exception Exception = null, int? TennantId = null);
    }
}
