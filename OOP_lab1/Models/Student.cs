using OOP_lab1.Infrastructure;

using System.ComponentModel;

namespace OOP_lab1.Models
{
    public class Student : BaseViewModel, IEditableObject
    {
        private string _lastName = string.Empty;
        private string _group = string.Empty;
        private int _grade;
        private Student? _beforeEdit;

        internal event EventHandler<Student>? EditCommitted;

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string Group
        {
            get => _group;
            set => SetProperty(ref _group, value);
        }

        public int Grade
        {
            get => _grade;
            set => SetProperty(ref _grade, value);
        }

        public Student Clone()
        {
            var clone = new Student();
            clone.CopyFrom(this);
            return clone;
        }

        internal void CopyFrom(Student source)
        {
            LastName = source.LastName;
            Group = source.Group;
            Grade = source.Grade;
        }

        void IEditableObject.BeginEdit() => _beforeEdit ??= Clone();

        void IEditableObject.EndEdit()
        {
            var before = _beforeEdit;
            _beforeEdit = null;
            if (before != null)
                EditCommitted?.Invoke(this, before);
        }

        void IEditableObject.CancelEdit()
        {
            var before = _beforeEdit;
            _beforeEdit = null;
            if (before != null)
                CopyFrom(before);
        }
    }
}
