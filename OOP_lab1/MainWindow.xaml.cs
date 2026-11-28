using OOP_lab1.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OOP_lab1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void ExplicitUpdate_Click(object sender, RoutedEventArgs e)
        {
            ExplicitBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        private void StudentsGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is TextBoxBase || sender is not DataGrid grid ||
                DataContext is not MainViewModel viewModel)
                return;

            ICommand? command = (Keyboard.Modifiers, e.Key) switch
            {
                (ModifierKeys.Control, Key.C) => viewModel.CopyStudentsCommand,
                (ModifierKeys.Control, Key.X) => viewModel.CutStudentsCommand,
                (ModifierKeys.Control, Key.V) => viewModel.PasteStudentsCommand,
                (ModifierKeys.None, Key.Delete) => viewModel.DeleteStudentsCommand,
                _ => null
            };

            if (command == null)
                return;

            e.Handled = true;
            if (command.CanExecute(grid.SelectedItems))
                command.Execute(grid.SelectedItems);
        }

    }
}
