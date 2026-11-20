using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using OOP_lab1.Infrastructure;
using OOP_lab1.Models;
using OOP_lab1.Services;

namespace OOP_lab1.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _selectedTheme = "Светлая (Light)";
        private string _selectedLanguage = "Русский";
        private string _selectedFontFamily = "Segoe UI";
        private double _selectedFontSize = 14;

        private Student? _selectedStudent;
        private Student? _selectedSecondStudent;
        private Student? _clipboardStudent;

        private int _demoCounter = 1;
        private string _bindingModeText = "Начальный текст";
        private string _triggerDemoText = "Текст для проверки";

        private string _inputLastName = string.Empty;
        private string _inputGroup = string.Empty;
        private string _inputGrade = "5";
        private string _statusMessage = "Готово";

        public MainViewModel()
        {
            Students = new ObservableCollection<Student>
            {
                new Student { LastName = "Иванов", Group = "ПИН.ИИ", Grade = 5 },
                new Student { LastName = "Петров", Group = "ПИН.РИС", Grade = 4 },
                new Student { LastName = "Новиков", Group = "ПИЭ", Grade = 3 }
            };

            SecondStudents = new ObservableCollection<Student>();

            AddStudentCommand = new RelayCommand(_ => AddStudent(), _ => CanAddStudent());
            EditStudentCommand = new RelayCommand(_ => EditStudent(), _ => SelectedStudent != null && CanAddStudent());
            DeleteStudentCommand = new RelayCommand(_ => DeleteStudent(), _ => SelectedStudent != null);

            CopyStudentCommand = new RelayCommand(_ => CopyStudent(), _ => SelectedStudent != null);
            PasteStudentCommand = new RelayCommand(_ => PasteStudent(), _ => _clipboardStudent != null);

            ResetSettingsCommand = new RelayCommand(_ => ResetSettings());
            ShowActionMessageCommand = new RelayCommand(param =>
            {
                StatusMessage = $"Выполнена команда: {param}";
            });
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            ChangeBindingTextCommand = new RelayCommand(_ =>
            {
                BindingModeText = $"Изменено из VM #{_demoCounter++}";
            });
        }

        public ObservableCollection<string> Themes { get; } = new()
        {
            "Светлая (Light)",
            "Темная (Dark)",
            "Системная (System)"
        };

        public ObservableCollection<string> Languages { get; } = new()
        {
            "Русский",
            "English"
        };

        public ObservableCollection<string> FontFamilies { get; } = new()
        {
            "Segoe UI", "Consolas", "Tahoma", "Arial", "Calibri", "Times New Roman"
        };

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    ThemeService.ApplyTheme(value);
                    StatusMessage = $"Тема изменена: {value}";
                }
            }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value))
                {
                    ThemeService.ApplyLanguage(value);
                    StatusMessage = $"Язык интерфейса: {value}";
                }
            }
        }

        public string SelectedFontFamily
        {
            get => _selectedFontFamily;
            set => SetProperty(ref _selectedFontFamily, value);
        }

        public double SelectedFontSize
        {
            get => _selectedFontSize;
            set => SetProperty(ref _selectedFontSize, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<Student> Students { get; }
        public ObservableCollection<Student> SecondStudents { get; }

        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (SetProperty(ref _selectedStudent, value))
                {
                    if (value != null)
                    {
                        _selectedSecondStudent = null;
                        OnPropertyChanged(nameof(SelectedSecondStudent));
                        InputLastName = value.LastName;
                        InputGroup = value.Group;
                        InputGrade = value.Grade.ToString();
                    }
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Student? SelectedSecondStudent
        {
            get => _selectedSecondStudent;
            set
            {
                if (SetProperty(ref _selectedSecondStudent, value))
                {
                    if (value != null)
                    {
                        _selectedStudent = null;
                        OnPropertyChanged(nameof(SelectedStudent));
                    }
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string InputLastName
        {
            get => _inputLastName;
            set => SetProperty(ref _inputLastName, value);
        }

        public string InputGroup
        {
            get => _inputGroup;
            set => SetProperty(ref _inputGroup, value);
        }

        public string InputGrade
        {
            get => _inputGrade;
            set => SetProperty(ref _inputGrade, value);
        }

        public ICommand AddStudentCommand { get; }
        public ICommand EditStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand CopyStudentCommand { get; }
        public ICommand PasteStudentCommand { get; }
        public ICommand DeleteSecondStudentCommand { get; }
        public ICommand ResetSettingsCommand { get; }
        public ICommand ShowActionMessageCommand { get; }
        public ICommand ExitCommand { get; }

        public string BindingModeText
        {
            get => _bindingModeText;
            set => SetProperty(ref _bindingModeText, value);
        }

        public string TriggerDemoText
        {
            get => _triggerDemoText;
            set => SetProperty(ref _triggerDemoText, value);
        }

        public ICommand ChangeBindingTextCommand { get; }

        private bool CanAddStudent()
        {
            return !string.IsNullOrWhiteSpace(InputLastName)
                && !string.IsNullOrWhiteSpace(InputGroup)
                && int.TryParse(InputGrade, out int grade)
                && grade >= 1 && grade <= 10;
        }

        private void AddStudent()
        {
            int.TryParse(InputGrade, out int grade);
            var student = new Student
            {
                LastName = InputLastName.Trim(),
                Group = InputGroup.Trim(),
                Grade = grade
            };
            Students.Add(student);
            SelectedStudent = student;
            StatusMessage = $"Добавлен студент: {student.LastName}";
        }

        private void EditStudent()
        {
            if (SelectedStudent == null) return;
            int.TryParse(InputGrade, out int grade);

            SelectedStudent.LastName = InputLastName.Trim();
            SelectedStudent.Group = InputGroup.Trim();
            SelectedStudent.Grade = grade;
            StatusMessage = $"Изменена запись: {SelectedStudent.LastName}";
        }

        private void DeleteStudent()
        {
            if (SelectedStudent == null) return;
            string name = SelectedStudent.LastName;
            Students.Remove(SelectedStudent);
            SelectedStudent = null;
            StatusMessage = $"Удалён из Таблицы 1: {name}";
            CommandManager.InvalidateRequerySuggested();
        }

        private void CopyStudent()
        {
            if (SelectedStudent == null) return;
            _clipboardStudent = SelectedStudent.Clone();
            StatusMessage = $"Скопировано из Таблицы 1: {_clipboardStudent.LastName}";
            CommandManager.InvalidateRequerySuggested();
        }

        private void PasteStudent()
        {
            if (_clipboardStudent == null) return;
            var pasted = _clipboardStudent.Clone();
            SecondStudents.Add(pasted);
            StatusMessage = $"Вставлено в Таблицу 2: {pasted.LastName}";
            CommandManager.InvalidateRequerySuggested();
        }

        private void ResetSettings()
        {
            SelectedTheme = "Светлая (Light)";
            SelectedLanguage = "Русский";
            SelectedFontFamily = "Segoe UI";
            SelectedFontSize = 14;
            StatusMessage = "Настройки сброшены по умолчанию";
        }
    }
}