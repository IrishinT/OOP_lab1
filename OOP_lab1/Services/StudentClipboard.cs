using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using OOP_lab1.Models;

namespace OOP_lab1.Services
{
    internal static class StudentClipboard
    {
        public static bool HasText()
        {
            try
            {
                return Clipboard.ContainsText();
            }
            catch (ExternalException)
            {
                return false;
            }
        }

        public static bool TryCopy(IEnumerable<Student> students)
        {
            string text = string.Join(Environment.NewLine, students.Select(student => string.Join('\t',
                EscapeField(student.LastName), EscapeField(student.Group), student.Grade.ToString(CultureInfo.InvariantCulture))));

            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (ExternalException)
            {
                return false;
            }
        }

        public static IReadOnlyList<Student>? Read()
        {
            try
            {
                using var parser = new TextFieldParser(new StringReader(Clipboard.GetText()));
                parser.SetDelimiters("\t");
                parser.TrimWhiteSpace = false;

                var students = new List<Student>();
                while (!parser.EndOfData)
                {
                    string[]? fields = parser.ReadFields();
                    if (fields is not { Length: 3 } ||
                        !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int grade))
                        return [];

                    students.Add(new Student { LastName = fields[0], Group = fields[1], Grade = grade });
                }

                return students;
            }
            catch (MalformedLineException)
            {
                return [];
            }
            catch (ExternalException)
            {
                return null;
            }
        }

        private static string EscapeField(string value)
        {
            return value.IndexOfAny(['\t', '\r', '\n', '"']) < 0
                ? value
                : $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
