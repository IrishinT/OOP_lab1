using OOP_lab1.Infrastructure;

namespace OOP_lab1.Models
{
    public class Student : BaseViewModel
    {
        private string _lastName = string.Empty;
        private string _group = string.Empty;
        private int _grade;

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
            return new Student
            {
                LastName = this.LastName,
                Group = this.Group,
                Grade = this.Grade
            };
        }
    }
}