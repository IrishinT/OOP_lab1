using System.Collections.ObjectModel;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using CodingSeb.Localization;
using OOP_lab1.Infrastructure;
using OOP_lab1.Models;
using OOP_lab1.Services;

namespace OOP_lab1.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private AppTheme _selectedTheme = AppTheme.Light;
        private string _selectedLanguage = App.DefaultLanguage;
        private string _selectedFontFamily = "Segoe UI";
        private double _selectedFontSize = 14;

        private Student? _selectedStudent;
        private Student? _selectedSecondStudent;

        private int _demoCounter = 1;
        private string _bindingModeText = Loc.Tr("Demo_InitialText");
        private string _triggerDemoText = Loc.Tr("Demo_TriggerText");

        private string _inputLastName = string.Empty;
        private string _inputGroup = string.Empty;
        private string _inputGrade = "5";
        private string _statusMessage = Loc.Tr("Status_Ready");

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
            DeleteStudentsCommand = new RelayCommand(rows => DeleteStudents((IList)rows!), HasSelectedRows);
            CopyStudentsCommand = new RelayCommand(rows => CopyStudents((IList)rows!, cut: false), HasSelectedRows);
            CutStudentsCommand = new RelayCommand(rows => CopyStudents((IList)rows!, cut: true), HasSelectedRows);
            PasteStudentsCommand = new RelayCommand(_ => PasteStudents(), _ => StudentClipboard.HasText());

            ResetSettingsCommand = new RelayCommand(_ => ResetSettings());
            ShowActionMessageCommand = new RelayCommand(param =>
            {
                SetStatus("Status_MenuAction", Loc.Tr((string)param!));
            });
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            ChangeBindingTextCommand = new RelayCommand(_ =>
            {
                BindingModeText = string.Format(Loc.Tr("Demo_ChangedText"), _demoCounter++);
            });
        }

        public IReadOnlyList<AppTheme> Themes { get; } = Enum.GetValues<AppTheme>();

        public IReadOnlyList<KeyValuePair<string, string>> Languages { get; } = Loc.Instance.AvailableLanguages
            .Select(code => KeyValuePair.Create(code, Loc.Tr("LanguageName", null, code)))
            .ToArray();

        public ObservableCollection<string> FontFamilies { get; } = new()
        {
            "Segoe UI", "Consolas", "Tahoma", "Arial", "Calibri", "Times New Roman"
        };

        public AppTheme SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    ThemeService.ApplyTheme(value);
                    SetStatus("Status_ThemeChanged", Loc.Tr($"Theme_{value}"));
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
                    Loc.Instance.CurrentLanguage = value;
                    SetStatus("Status_LanguageChanged", Loc.Tr("LanguageName"));
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
            private set => SetProperty(ref _statusMessage, value);
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
        public ICommand DeleteStudentsCommand { get; }
        public ICommand CopyStudentsCommand { get; }
        public ICommand CutStudentsCommand { get; }
        public ICommand PasteStudentsCommand { get; }
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
            SetStatus("Status_StudentAdded", student.LastName);
        }

        private void EditStudent()
        {
            if (SelectedStudent == null) return;
            int.TryParse(InputGrade, out int grade);

            SelectedStudent.LastName = InputLastName.Trim();
            SelectedStudent.Group = InputGroup.Trim();
            SelectedStudent.Grade = grade;
            SetStatus("Status_StudentEdited", SelectedStudent.LastName);
        }

        private static bool HasSelectedRows(object? parameter) => parameter is IList { Count: > 0 };

        private void DeleteStudents(IList selection)
        {
            Student[] rows = selection.Cast<Student>().ToArray();
            RemoveStudents(rows);
            SetStatus("Status_StudentsDeleted", rows.Length);
        }

        private void RemoveStudents(IEnumerable<Student> rows)
        {
            foreach (Student student in rows)
            {
                if (!Students.Remove(student))
                    SecondStudents.Remove(student);
            }
        }

        private void CopyStudents(IList selection, bool cut)
        {
            Student[] rows = selection.Cast<Student>().ToArray();
            if (!StudentClipboard.TryCopy(rows))
            {
                SetStatus("Status_ClipboardUnavailable");
                return;
            }

            if (cut)
                RemoveStudents(rows);

            SetStatus(cut ? "Status_StudentsCut" : "Status_StudentsCopied", rows.Length);
            CommandManager.InvalidateRequerySuggested();
        }

        private void PasteStudents()
        {
            var rows = StudentClipboard.Read();
            if (rows == null)
            {
                SetStatus("Status_ClipboardUnavailable");
                return;
            }

            if (rows.Count == 0)
            {
                SetStatus("Status_ClipboardInvalid");
                return;
            }

            foreach (Student row in rows)
                SecondStudents.Add(row);

            SetStatus("Status_StudentsPasted", rows.Count);
        }

        private void SetStatus(string textId, object? value = null)
        {
            string text = Loc.Tr(textId);
            StatusMessage = value == null ? text : string.Format(text, value);
        }

        private void ResetSettings()
        {
            SelectedTheme = AppTheme.Light;
            SelectedLanguage = App.DefaultLanguage;
            SelectedFontFamily = "Segoe UI";
            SelectedFontSize = 14;
            SetStatus("Status_SettingsReset");
        }
    }
}
