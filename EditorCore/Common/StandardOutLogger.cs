using System;

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
