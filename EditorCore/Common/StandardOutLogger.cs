using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEditor
{
    public class StandardOutLogger
    {
        public StandardOutLogger()
        {
            WLog.Subscribe(LogCategory.None, OnRecieveLoggerMessage, null);
        }

        private void OnRecieveLoggerMessage(WLog.Entry message)
        {
            string messagePrefix = "";
            if(message.Severity == LogSeverity.Warning)
                messagePrefix = "Warning: ";
            else if (message.Severity == LogSeverity.Error)
                messagePrefix = "Error: ";

            string categoryPrefix = "";
            if (message.Category != LogCategory.None)
                categoryPrefix = string.Format("[{0}] - ", message.Category);

            Console.WriteLine("{0}{1}{2}", messagePrefix, categoryPrefix, message.Message);
        }
    }
}
