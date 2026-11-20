using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            SampleGrid.ItemsSource = new[]
            {
                new { Фамилия = "Иванов", Группа = "ПИН.ИИ", Оценка = 5 },
                new { Фамилия = "Петров", Группа = "ПИН.РИС", Оценка = 4 },
                new { Фамилия = "Новиков", Группа = "ПИЭ", Оценка = 3 }
            };
        }
    }
}