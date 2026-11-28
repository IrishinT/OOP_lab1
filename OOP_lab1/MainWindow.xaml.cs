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
            DataContext = new MainViewModel(() => StudentsGrid.CommitEdit(DataGridEditingUnit.Row, true));
        }

        private void ExplicitUpdate_Click(object sender, RoutedEventArgs e)
        {
            ExplicitBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

        private void StudentsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = (DataGrid)sender;
            ActivateTable(grid);
            if (!grid.IsKeyboardFocusWithin)
                grid.Focus();
        }

        private void StudentsGrid_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ActivateTable((DataGrid)sender);
        }

        private void ActivateTable(DataGrid grid)
        {
            if (DataContext is MainViewModel viewModel)
                viewModel.ActiveTableNumber = grid == SecondStudentsGrid ? 2 : 1;
        }

        private void StudentData_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control || e.OriginalSource is TextBoxBase ||
                DataContext is not MainViewModel viewModel)
                return;

            ICommand? command = e.Key switch
            {
                Key.Z => viewModel.UndoStudentsCommand,
                Key.Y => viewModel.RedoStudentsCommand,
                _ => null
            };

            if (command == null)
                return;

            e.Handled = true;
            command.Execute(null);
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
            ActivateTable(grid);
            object parameter = e.Key == Key.V ? viewModel.ActiveTableNumber : grid.SelectedItems;
            if (command.CanExecute(parameter))
                command.Execute(parameter);
        }

    }
}
