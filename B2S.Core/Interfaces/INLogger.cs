using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2S.Core.Interfaces
{
    public interface INLogger
    {
        void LogInfo(string message);
        void LogTrace(string message);
        void LogError(Exception ex);
    }
}
