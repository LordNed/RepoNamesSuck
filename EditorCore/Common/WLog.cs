using System.Collections.Generic;
using System.Diagnostics;

namespace WEditor
{
    public enum LogCategory
    {
        // Generic Messages, will show up in all filters.
        None,
        // Rendering System
        Rendering,
        // Shader Compiling (not shader generator)
        ShaderCompiler,
        // Automatic Shader Generation
        TEVShaderGenerator,
        // Model Loading
        ModelLoading,
        // Entity Loading
        EntityLoading,
        // Editor Messages
        EditorCore,
        // Texture Decoding/Encoding
        Textures,
        // Archive Loader
        ArchiveLoading,
        // UI
        UI
    }

    public enum LogSeverity
    {
        // Application is just telling you what it's doing.
        Info,
        // Something unexpected or wrong happened, but should recover gracefully
        Warning,
        // Something unexpected happened, probably can't recover gracefully
        Error
    }

    public static class WLog
    {
        public delegate void RecieveLogMessage(Entry message);

        public class Entry
        {
            public LogCategory Category { get; private set; }
            public LogSeverity Severity { get; private set; }
            public string Message { get; private set; }
            public object Context { get; private set; }
            public StackTrace StackTrace { get; private set; }

            public Entry(LogCategory cat, LogSeverity severity, string message, object context, StackTrace stackTrace)
            {
                Category = cat;
                Severity = severity;
                Message = message;
                Context = context;
                StackTrace = stackTrace;
            }

            public override string ToString()
            {
                return Message;
            }
        }

        private sealed class MessageSubscriber
        {
            public LogCategory Category { get; private set;}
            public RecieveLogMessage Callback { get; private set; }
            public object ContextFilter { get; private set; }

            public MessageSubscriber(LogCategory category, RecieveLogMessage callback, object contextFilter)
            {
                Category = category;
                Callback = callback;
                ContextFilter = contextFilter;
            }
        }

        private static List<MessageSubscriber> m_subscribers = new List<MessageSubscriber>();

        public static void Subscribe(LogCategory category, RecieveLogMessage callback, object contextFilter)
        {
            if(m_subscribers.Find(x => x.Category == category && x.Callback == callback && x.ContextFilter.Equals(contextFilter)) != null)
            {
                WLog.Warning(LogCategory.None, null, "Subscriber list already contains callback: {0}!", callback);
            }

            MessageSubscriber subscriber = new MessageSubscriber(category, callback, contextFilter);
            m_subscribers.Add(subscriber);
        }

        public static void Info(LogCategory category, object context, string format, params object[] args)
        {
            LogMessage(category, LogSeverity.Info, context, format, args);
        }

        public static void Warning(LogCategory category, object context, string format, params object[] args)
        {
            LogMessage(category, LogSeverity.Warning, context, format, args);
        }

        public static void Error(LogCategory category, object context, string format, params object[] args)
        {
            LogMessage(category, LogSeverity.Error, context, format, args);
        }

        private static void LogMessage(LogCategory category, LogSeverity severity, object context, string format, params object[] args)
        {
            // Get the Stack Trace for this message to give a better idea of where it's coming from. Chop off
            // the first call from the stack trace which is this internal LogMessage so it only shows from the public function up.
            StackTrace stackTrace = new StackTrace(1, true);

            string finalMessage = string.Format(format, args);
            Entry entry = new Entry(category, severity, finalMessage, context, stackTrace);

            for(int i = 0; i < m_subscribers.Count; i++)
            {
                MessageSubscriber subscriber = m_subscribers[i];

                // If the subscriber subs to that category (or None which is 'all categories') and
                // the context object of the subscriber is null (or matches our context) we send
                // along the message.
                if((subscriber.Category == category || subscriber.Category == LogCategory.None) && (subscriber.ContextFilter == null || subscriber.ContextFilter.Equals(context)))
                {
                    subscriber.Callback(entry);
                }
            }
        }
    }
}
