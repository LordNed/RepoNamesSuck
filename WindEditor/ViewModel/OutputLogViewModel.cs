using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using WEditor;

namespace WindEditor.UI
{
    public class OutputLogViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Messages
        {
            get { return m_messageLog.ToString(); }
        }

        private EditorCore m_editor;
        private StringBuilder m_messageLog;

        public OutputLogViewModel(EditorCore editor)
        {
            m_editor = editor;
            m_messageLog = new StringBuilder();
            WLog.Subscribe(LogCategory.None, OnRecieveMessage, null);
        }

        private void OnRecieveMessage(WLog.Entry message)
        {
            // Append Warning: or Error: to the messages if they have the right severity.
            string messagePrefix = "";
            if (message.Severity == LogSeverity.Warning)
                messagePrefix = "Warning: ";
            else if (message.Severity == LogSeverity.Error)
                messagePrefix = "Error: ";

            string categoryPrefix = "";
            if (message.Category != LogCategory.None)
                categoryPrefix = string.Format("[{0}] - ", message.Category);

            string finalMessage = string.Format("{0}{1}{2}", messagePrefix, categoryPrefix, message.Message);

            m_messageLog.AppendLine(finalMessage);
            OnPropertyChanged("Messages");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
