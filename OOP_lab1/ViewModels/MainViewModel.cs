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
        private int _activeTableNumber = 1;
        private readonly Stack<(Action Undo, Action Redo)> _undoHistory = new();
        private readonly Stack<(Action Undo, Action Redo)> _redoHistory = new();

        private int _demoCounter = 1;
        private string _bindingModeText = Loc.Tr("Demo_InitialText");
        private string _triggerDemoText = Loc.Tr("Demo_TriggerText");

        private string _inputLastName = string.Empty;
        private string _inputGroup = string.Empty;
        private string _inputGrade = "5";
        private string _statusMessage = Loc.Tr("Status_Ready");

        public MainViewModel(Func<bool>? commitTableEdit = null)
        {
            Students = new ObservableCollection<Student>
            {
                new Student { LastName = "Иванов", Group = "ПИН.ИИ", Grade = 5 },
                new Student { LastName = "Петров", Group = "ПИН.РИС", Grade = 4 },
                new Student { LastName = "Новиков", Group = "ПИЭ", Grade = 3 }
            };

            SecondStudents = new ObservableCollection<Student>();
            foreach (Student student in Students)
                student.EditCommitted += Student_EditCommitted;

            AddStudentCommand = CreateTableCommand(_ => AddStudent(), _ => CanAddStudent());
            EditStudentCommand = CreateTableCommand(_ => EditStudent(), _ => SelectedStudent != null && CanAddStudent());
            DeleteStudentsCommand = CreateTableCommand(rows => DeleteStudents((IList)rows!), HasSelectedRows);
            CopyStudentsCommand = CreateTableCommand(rows => CopyStudents((IList)rows!, cut: false), HasSelectedRows);
            CutStudentsCommand = CreateTableCommand(rows => CopyStudents((IList)rows!, cut: true), HasSelectedRows);
            PasteStudentsCommand = CreateTableCommand(target => PasteStudents((int)target!),
                target => target is 1 or 2 && StudentClipboard.HasText());
            UndoStudentsCommand = CreateTableCommand(_ => MoveHistory(undo: true));
            RedoStudentsCommand = CreateTableCommand(_ => MoveHistory(undo: false));

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

            ICommand CreateTableCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
            {
                return new RelayCommand(parameter =>
                {
                    if ((commitTableEdit?.Invoke() ?? true) && (canExecute?.Invoke(parameter) ?? true))
                        execute(parameter);
                }, canExecute);
            }
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

        public int ActiveTableNumber
        {
            get => _activeTableNumber;
            internal set => SetProperty(ref _activeTableNumber, value);
        }

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
                        LoadStudentInputs(value);
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
        public ICommand UndoStudentsCommand { get; }
        public ICommand RedoStudentsCommand { get; }
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
            AddStudents(Students, [student]);
            SelectedStudent = student;
            SetStatus("Status_StudentAdded", student.LastName);
        }

        private void EditStudent()
        {
            if (SelectedStudent == null) return;
            int.TryParse(InputGrade, out int grade);

            Student student = SelectedStudent;
            Student before = student.Clone();
            student.LastName = InputLastName.Trim();
            student.Group = InputGroup.Trim();
            student.Grade = grade;
            RecordStudentEdit(student, before);
        }

        private void Student_EditCommitted(object? sender, Student before)
        {
            var student = (Student)sender!;
            if (student == SelectedStudent && InputLastName == before.LastName &&
                InputGroup == before.Group && InputGrade == before.Grade.ToString())
            {
                LoadStudentInputs(student);
            }

            RecordStudentEdit(student, before);
        }

        private void RecordStudentEdit(Student student, Student before)
        {
            if (student.LastName == before.LastName && student.Group == before.Group && student.Grade == before.Grade)
                return;

            Student after = student.Clone();
            RecordChange(() => ApplyStudentValues(before), () => ApplyStudentValues(after));
            SetStatus("Status_StudentEdited", student.LastName);

            void ApplyStudentValues(Student values)
            {
                student.CopyFrom(values);
                if (student == SelectedStudent)
                    LoadStudentInputs(student);
            }
        }

        private void LoadStudentInputs(Student student)
        {
            InputLastName = student.LastName;
            InputGroup = student.Group;
            InputGrade = student.Grade.ToString();
        }

        private static bool HasSelectedRows(object? parameter) => parameter is IList { Count: > 0 };

        private void DeleteStudents(IList selection)
        {
            Student[] rows = selection.Cast<Student>().ToArray();
            RemoveStudents(rows);
            SetStatus("Status_StudentsDeleted", rows.Length);
        }

        private void AddStudents(ObservableCollection<Student> table, IReadOnlyList<Student> students)
        {
            int startIndex = table.Count;
            var rows = students.Select((student, offset) => (Student: student, Index: startIndex + offset)).ToArray();
            ChangeRows(table, rows, remove: false);
        }

        private void RemoveStudents(IReadOnlyList<Student> students)
        {
            var table = Students.Contains(students[0]) ? Students : SecondStudents;
            var rows = students.Select(student => (Student: student, Index: table.IndexOf(student)))
                .OrderBy(row => row.Index)
                .ToArray();
            ChangeRows(table, rows, remove: true);
        }

        private void ChangeRows(ObservableCollection<Student> table, (Student Student, int Index)[] rows, bool remove)
        {
            void Insert()
            {
                foreach (var row in rows)
                {
                    if (table == Students)
                        row.Student.EditCommitted += Student_EditCommitted;
                    table.Insert(row.Index, row.Student);
                }
            }

            void Remove()
            {
                foreach (var row in rows)
                {
                    if (table == Students)
                        row.Student.EditCommitted -= Student_EditCommitted;
                    table.Remove(row.Student);
                }
            }

            Action undo = remove ? Insert : Remove;
            Action redo = remove ? Remove : Insert;
            redo();
            RecordChange(undo, redo);
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

        private void PasteStudents(int tableNumber)
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

            var table = tableNumber == 1 ? Students : SecondStudents;
            AddStudents(table, rows);
            SetStatus("Status_StudentsPasted", rows.Count, tableNumber);
        }

        private void RecordChange(Action undo, Action redo)
        {
            _undoHistory.Push((undo, redo));
            _redoHistory.Clear();
        }

        private void MoveHistory(bool undo)
        {
            var source = undo ? _undoHistory : _redoHistory;
            var destination = undo ? _redoHistory : _undoHistory;
            if (!source.TryPeek(out var change))
                return;

            (undo ? change.Undo : change.Redo)();
            source.Pop();
            destination.Push(change);
            SetStatus(undo ? "Status_StudentsUndo" : "Status_StudentsRedo");
        }

        private void SetStatus(string textId, params object[] values)
        {
            StatusMessage = string.Format(Loc.Tr(textId), values);
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
