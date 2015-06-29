using System.Windows;
using System.Windows.Input;
using WindEditor.UI;

namespace WindEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel m_viewModel;

        public MainWindow()
        {
            InitializeComponent();
            m_viewModel = (MainWindowViewModel)DataContext;
        }

        private void OnExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Exit();
        }

        private void OnOpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Open();
        }

        private void OnSaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_viewModel.CanSave;
        }

        private void OnSaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Save();
        }

        private void OnCloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_viewModel.CanClose;
        }

        private void OnCloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Close();
        }

        private void OnUndoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_viewModel.CanUndo;
        }

        private void OnUndoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Undo();
        }

        private void OnRedoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m_viewModel.CanRedo;
        }

        private void OnRedoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            m_viewModel.Redo();
        }
    }
}
