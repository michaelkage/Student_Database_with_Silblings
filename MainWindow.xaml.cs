using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace StudentManagementApp
{
    public class Score
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public int? Grade { get; set; }

        public Score(int studentId, int subjectId, int? grade)
        {
            StudentID = studentId;
            SubjectID = subjectId;
            Grade = grade;
        }
    }

    public class Student
    {
        public int StudentID { get; set; }
        public string StudentPassword { get; set; }
        public string Name { get; set; }
        public List<int> OfferedSubjectIDs { get; set; } = new List<int>();

        public Student(int id, string name, string Password)
        {
            StudentID = id;
            Name = name;
            StudentPassword = Password;
        }
    }

    public class Subject
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }

        public Subject(int id, string name)
        {
            SubjectID = id;
            SubjectName = name;
        }
    }

    public partial class MainWindow : Window
    {
        public static Student[] students = new Student[0];
        public static Subject[] subjects = new Subject[0];
        public static Score[] scores = new Score[0];

        private const string StudentFile = "Student.txt";
        private const string SubjectFile = "Subject.txt";
        private const string ScoresFile = "Scores.txt";
        private const string PasswordFile = "AdminPassword.txt";
        private const string NextStudentIdFile = "NextStudentID.txt";
        private const string NextSubjectIdFile = "NextSubjectID.txt";

        public static string AdminPassword { get; set; } = "Messi";
        public static Student CurrentLoggedInStudent { get; set; }
        public bool isAdminSession = false;

        public MainWindow(bool isAdmin)
        {
            InitializeComponent();
            isAdminSession = isAdmin;
            UnlockDashboard(isAdmin);
        }

        public static string GetLetterGrade(int numericGrade)
        {
            if (numericGrade >= 80) return "A";
            if (numericGrade >= 70) return "B";
            if (numericGrade >= 60) return "C";
            if (numericGrade >= 50) return "P";
            return "F";
        }

        public static int GetNextStudentID()
        {
            int minimumNext = students.Length == 0 ? 1 : students.Max(s => s.StudentID) + 1;
            if (File.Exists(NextStudentIdFile) && int.TryParse(File.ReadAllText(NextStudentIdFile).Trim(), out int storedNext))
                return Math.Max(storedNext, minimumNext);
            return minimumNext;
        }

        public static int GetNextSubjectID()
        {
            int minimumNext = subjects.Length == 0 ? 1 : subjects.Max(s => s.SubjectID) + 1;
            if (File.Exists(NextSubjectIdFile) && int.TryParse(File.ReadAllText(NextSubjectIdFile).Trim(), out int storedNext))
                return Math.Max(storedNext, minimumNext);
            return minimumNext;
        }

        public static void LoadMemory()
        {
            if (File.Exists(PasswordFile))
                AdminPassword = File.ReadAllText(PasswordFile).Trim();

            // Load subjects before students so stale offered-subject references can be removed safely.
            if (File.Exists(SubjectFile))
            {
                var loadedSubjects = new List<Subject>();
                foreach (var line in File.ReadAllLines(SubjectFile))
                {
                    var parts = line.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int id))
                        loadedSubjects.Add(new Subject(id, parts[1]));
                }
                subjects = loadedSubjects.ToArray();
            }
            else
            {
                subjects = new Subject[0];
            }

            if (File.Exists(StudentFile))
            {
                var loadedStudents = new List<Student>();
                foreach (var line in File.ReadAllLines(StudentFile))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 3 && int.TryParse(parts[0], out int id))
                    {
                        string name = parts[1];
                        string password = parts[2];
                        var student = new Student(id, name, password);

                        if (parts.Length > 4)
                        {
                            student.OfferedSubjectIDs = parts
                                .Skip(4)
                                .Select(value => int.TryParse(value, out int subjectId) ? (int?)subjectId : null)
                                .Where(subjectId => subjectId.HasValue)
                                .Select(subjectId => subjectId!.Value)
                                .Where(subjectId => subjects.Any(subject => subject.SubjectID == subjectId))
                                .ToList();
                        }

                        loadedStudents.Add(student);
                    }
                }
                students = loadedStudents.ToArray();
            }
            else
            {
                students = new Student[0];
            }

            if (File.Exists(ScoresFile))
            {
                var loadedScores = new List<Score>();
                foreach (var line in File.ReadAllLines(ScoresFile))
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3 &&
                        int.TryParse(parts[0], out int sId) &&
                        int.TryParse(parts[1], out int subId) &&
                        int.TryParse(parts[2], out int grade))
                    {
                        loadedScores.Add(new Score(sId, subId, grade));
                    }
                }

                // Keep one score per StudentID + SubjectID. The first record is retained,
                // matching the existing FirstOrDefault() behavior used throughout the app.
                scores = loadedScores
                    .GroupBy(score => new { score.StudentID, score.SubjectID })
                    .Select(group => group.First())
                    .ToArray();
            }
            else
            {
                scores = new Score[0];
            }
        }

        public static void SaveMemory()
        {
            var studentLines = students.Select(s =>
            {
                var offeredSubjectIDs = s.OfferedSubjectIDs ?? new List<int>();
                string subjectsString = offeredSubjectIDs.Count > 0
                    ? string.Join(",", offeredSubjectIDs) : "";
                return $"{s.StudentID},{s.Name},{s.StudentPassword},{offeredSubjectIDs.Count},{subjectsString}".TrimEnd(',');
            });

            File.WriteAllLines(StudentFile, studentLines);
            File.WriteAllLines(SubjectFile, subjects.Select(s => $"{s.SubjectID},{s.SubjectName}"));

            // A missing Scores.txt record represents an offered subject with no score yet.
            // A stored 0 remains a real, explicitly entered zero.
            // Only save scores that point to an existing student, existing subject,
            // and a subject actually offered by that student.
            var validScores = scores.Where(score =>
                score.Grade.HasValue &&
                students.Any(student => student.StudentID == score.StudentID &&
                                        (student.OfferedSubjectIDs ?? new List<int>()).Contains(score.SubjectID)) &&
                subjects.Any(subject => subject.SubjectID == score.SubjectID));

            File.WriteAllLines(ScoresFile,
                validScores.Select(s => $"{s.StudentID},{s.SubjectID},{s.Grade!.Value}"));

            File.WriteAllText(PasswordFile, AdminPassword);

            // Persist the next unused IDs so deleting the highest ID never causes it to be reused.
            File.WriteAllText(NextStudentIdFile, GetNextStudentID().ToString());
            File.WriteAllText(NextSubjectIdFile, GetNextSubjectID().ToString());
        }

        private void UnlockDashboard(bool isAdmin)
        {
            if (isAdmin)
            {
                AdminPanel.Visibility = Visibility.Visible;
                StudentPanel.Visibility = Visibility.Collapsed;
                TxtWelcomeHeadline.Text = "Welcome to the Admin Control Panel";
            }
            else
            {
                AdminPanel.Visibility = Visibility.Collapsed;
                StudentPanel.Visibility = Visibility.Visible;
                TxtWelcomeHeadline.Text = CurrentLoggedInStudent == null
                    ? "Welcome"
                    : $"Welcome Back, {CurrentLoggedInStudent.Name} [ID: {CurrentLoggedInStudent.StudentID}]";
            }
        }

        private void MenuViewAllResults_Click(object sender, RoutedEventArgs e)
        {
            var window = new ViewStudentsWindow { Owner = this };
            window.ShowDialog();
        }

        private void MenuStudentManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new StudentManagementWindow { Owner = this };
            window.ShowDialog();
        }

        private void MenuAddNewSubject_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddSubjectWindow { Owner = this };
            window.ShowDialog();
        }

        private void MenuChangeAdminPassword_Click(object sender, RoutedEventArgs e)
        {
            var window = new ChangeAdminPasswordWindow { Owner = this };
            window.ShowDialog();
        }

        private void MenuViewMyGrades_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLoggedInStudent == null) return;
            var window = new ViewGradesWindow(CurrentLoggedInStudent) { Owner = this };
            window.ShowDialog();
        }

        private void MenuManageSubjects_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLoggedInStudent == null) return;
            var window = new AssignSubjectsWindow(CurrentLoggedInStudent) { Owner = this };
            window.ShowDialog();
            RefreshCurrentStudentReference();
        }

        private void MenuStudentPassword_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLoggedInStudent == null) return;
            var window = new StudentPasswordWindow(CurrentLoggedInStudent) { Owner = this };
            window.ShowDialog();
            RefreshCurrentStudentReference();
        }

        private void MenuEditMyDetails_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLoggedInStudent == null) return;
            int studentId = CurrentLoggedInStudent.StudentID;
            var window = new EditStudentDetailsWindow(CurrentLoggedInStudent) { Owner = this };
            window.ShowDialog();
            CurrentLoggedInStudent = students.FirstOrDefault(s => s.StudentID == studentId);
            TxtWelcomeHeadline.Text = CurrentLoggedInStudent == null
                ? "Welcome"
                : $"Welcome Back, {CurrentLoggedInStudent.Name} [ID: {CurrentLoggedInStudent.StudentID}]";
        }

        private static void RefreshCurrentStudentReference()
        {
            if (CurrentLoggedInStudent == null) return;
            int studentId = CurrentLoggedInStudent.StudentID;
            CurrentLoggedInStudent = students.FirstOrDefault(s => s.StudentID == studentId);
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentLoggedInStudent = null;
            var login = new LoginWindow();
            login.Show();
            Close();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}