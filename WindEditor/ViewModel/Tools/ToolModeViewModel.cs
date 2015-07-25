
namespace WindEditor.UI
{
    public class ToolModeViewModel
    {
        public ObjectPlaceToolViewModel ObjectPlaceView { get; private set;}


        private readonly MainWindowViewModel m_mainWindow;

        public ToolModeViewModel(MainWindowViewModel mainWindow)
        {
            m_mainWindow = mainWindow;
            ObjectPlaceView = new ObjectPlaceToolViewModel(mainWindow);
        }
    }
}
