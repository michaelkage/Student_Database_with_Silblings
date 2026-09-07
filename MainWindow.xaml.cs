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

        public Student(int id, string name, string password)
        {
            StudentID = id;
            Name = name;
            StudentPassword = password;
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
        // These arrays are operation-scoped working data, not the database itself.
        public static Student[] students = Array.Empty<Student>();
        public static Subject[] subjects = Array.Empty<Subject>();
        public static Score[] scores = Array.Empty<Score>();

        private const string StudentFile = "Student.txt";
        private const string SubjectFile = "Subject.txt";
        private const string ScoresFile = "Scores.txt";
        private const string PasswordFile = "AdminPassword.txt";
        private const string NextStudentIdFile = "NextStudentID.txt";
        private const string NextSubjectIdFile = "NextSubjectID.txt";

        private static readonly object DataLock = new();

        public static string AdminPassword { get; private set; } = "Messi";
        public static Student? CurrentLoggedInStudent { get; set; }
        public static bool IsAdminSessionActive { get; private set; }
        public bool isAdminSession { get; }

        public MainWindow(bool isAdmin)
        {
            InitializeComponent();
            isAdminSession = isAdmin;
            IsAdminSessionActive = isAdmin;
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

        // -----------------------------
        // Targeted reads
        // -----------------------------

        public static void LoadAdminPassword()
        {
            lock (DataLock)
            {
                if (File.Exists(PasswordFile))
                {
                    string value = File.ReadAllText(PasswordFile).Trim();
                    if (!string.IsNullOrEmpty(value))
                        AdminPassword = value;
                }
            }
        }

        public static void LoadStudents()
        {
            lock (DataLock)
            {
                if (!File.Exists(StudentFile))
                {
                    students = Array.Empty<Student>();
                    return;
                }

                var loadedStudents = new List<Student>();
                foreach (string line in File.ReadAllLines(StudentFile))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length < 3 || !int.TryParse(parts[0], out int id))
                        continue;

                    var student = new Student(id, parts[1], parts[2]);

                    // Current file format is ID,Name,Password,Count,SubjectID,...
                    // Only valid integer subject IDs are accepted. Malformed data is ignored.
                    if (parts.Length > 4)
                    {
                        student.OfferedSubjectIDs = parts
                            .Skip(4)
                            .Select(value => int.TryParse(value, out int subjectId) ? (int?)subjectId : null)
                            .Where(subjectId => subjectId.HasValue)
                            .Select(subjectId => subjectId!.Value)
                            .Distinct()
                            .ToList();
                    }

                    loadedStudents.Add(student);
                }

                students = loadedStudents.ToArray();
            }
        }

        public static Student? LoadStudent(int studentId)
        {
            LoadStudents();
            return students.FirstOrDefault(student => student.StudentID == studentId);
        }

        public static void LoadSubjects()
        {
            lock (DataLock)
            {
                if (!File.Exists(SubjectFile))
                {
                    subjects = Array.Empty<Subject>();
                    return;
                }

                var loadedSubjects = new List<Subject>();
                foreach (string line in File.ReadAllLines(SubjectFile))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int id))
                        loadedSubjects.Add(new Subject(id, parts[1]));
                }

                subjects = loadedSubjects.ToArray();
            }
        }

        public static void LoadScores()
        {
            lock (DataLock)
            {
                if (!File.Exists(ScoresFile))
                {
                    scores = Array.Empty<Score>();
                    return;
                }

                var loadedScores = new List<Score>();
                foreach (string line in File.ReadAllLines(ScoresFile))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 3 &&
                        int.TryParse(parts[0], out int studentId) &&
                        int.TryParse(parts[1], out int subjectId) &&
                        int.TryParse(parts[2], out int grade))
                    {
                        loadedScores.Add(new Score(studentId, subjectId, grade));
                    }
                }

                scores = loadedScores
                    .GroupBy(score => new { score.StudentID, score.SubjectID })
                    .Select(group => group.First())
                    .ToArray();
            }
        }

        // Compatibility method for older code. New operations should use targeted reads.
        public static void LoadMemory()
        {
            LoadAdminPassword();
            LoadSubjects();
            LoadStudents();
            LoadScores();
        }

        // -----------------------------
        // Persistent ID counters
        // -----------------------------

        public static int GetNextStudentID()
        {
            LoadStudents();
            return GetNextId(students.Select(student => student.StudentID), NextStudentIdFile);
        }

        public static int GetNextSubjectID()
        {
            LoadSubjects();
            return GetNextId(subjects.Select(subject => subject.SubjectID), NextSubjectIdFile);
        }

        private static int GetNextId(IEnumerable<int> existingIds, string counterFile)
        {
            int minimumNext = existingIds.Any() ? existingIds.Max() + 1 : 1;

            if (File.Exists(counterFile) &&
                int.TryParse(File.ReadAllText(counterFile).Trim(), out int storedNext))
            {
                return Math.Max(storedNext, minimumNext);
            }

            return minimumNext;
        }

        // -----------------------------
        // Targeted writes
        // -----------------------------

        public static void SaveStudents()
        {
            lock (DataLock)
            {
                WriteAllLinesAtomically(StudentFile, students.Select(student =>
                {
                    var offeredSubjectIds = student.OfferedSubjectIDs ?? new List<int>();
                    string subjectsString = offeredSubjectIds.Count > 0
                        ? string.Join(",", offeredSubjectIds)
                        : "";
                    return $"{student.StudentID},{student.Name},{student.StudentPassword},{offeredSubjectIds.Count},{subjectsString}".TrimEnd(',');
                }));

                WriteAllTextAtomically(NextStudentIdFile, GetNextId(students.Select(s => s.StudentID), NextStudentIdFile).ToString());
            }
        }

        public static void SaveSubjects()
        {
            lock (DataLock)
            {
                WriteAllLinesAtomically(SubjectFile, subjects.Select(subject => $"{subject.SubjectID},{subject.SubjectName}"));
                WriteAllTextAtomically(NextSubjectIdFile, GetNextId(subjects.Select(s => s.SubjectID), NextSubjectIdFile).ToString());
            }
        }

        public static void SaveScores()
        {
            lock (DataLock)
            {
                var validScores = scores.Where(score =>
                    score.Grade.HasValue &&
                    students.Any(student => student.StudentID == score.StudentID &&
                        (student.OfferedSubjectIDs ?? new List<int>()).Contains(score.SubjectID)) &&
                    subjects.Any(subject => subject.SubjectID == score.SubjectID));

                WriteAllLinesAtomically(
                    ScoresFile,
                    validScores.Select(score => $"{score.StudentID},{score.SubjectID},{score.Grade!.Value}"));
            }
        }

        public static void SaveAdminPassword()
        {
            lock (DataLock)
            {
                WriteAllTextAtomically(PasswordFile, AdminPassword);
            }
        }

        // Compatibility method for older code. New mutations should save only their affected data.
        public static void SaveMemory()
        {
            SaveStudents();
            SaveSubjects();
            SaveScores();
            SaveAdminPassword();
        }

        public static void SetAdminPassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password cannot be empty.", nameof(newPassword));

            AdminPassword = newPassword;
            SaveAdminPassword();
        }

        private static void WriteAllLinesAtomically(string path, IEnumerable<string> lines)
        {
            string tempPath = path + ".tmp";
            File.WriteAllLines(tempPath, lines);
            File.Move(tempPath, path, true);
        }

        private static void WriteAllTextAtomically(string path, string content)
        {
            string tempPath = path + ".tmp";
            File.WriteAllText(tempPath, content);
            File.Move(tempPath, path, true);
        }

        // -----------------------------
        // Session/UI control
        // -----------------------------

        private void UnlockDashboard(bool isAdmin)
        {
            AdminPanel.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            StudentPanel.Visibility = isAdmin ? Visibility.Collapsed : Visibility.Visible;

            if (isAdmin)
            {
                AdminMenu.Visibility = Visibility.Visible;
                StudentMenu.Visibility = Visibility.Collapsed;
                TxtWelcomeHeadline.Text = "Welcome to the Admin Control Panel";
            }
            else
            {
                AdminMenu.Visibility = Visibility.Collapsed;
                StudentMenu.Visibility = Visibility.Visible;
                TxtWelcomeHeadline.Text = CurrentLoggedInStudent == null
                    ? "Welcome"
                    : $"Welcome Back, {CurrentLoggedInStudent.Name} [ID: {CurrentLoggedInStudent.StudentID}]";
            }
        }

        private bool RequireAdminSession()
        {
            if (isAdminSession && IsAdminSessionActive)
                return true;

            MessageBox.Show("This operation is available to administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private bool RequireStudentSession()
        {
            if (!isAdminSession && !IsAdminSessionActive && CurrentLoggedInStudent != null)
                return true;

            MessageBox.Show("A student session is required for this operation.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private void MenuViewAllResults_Click(object sender, RoutedEventArgs e)
        {
            if (!RequireAdminSession()) return;
            new ViewStudentsWindow { Owner = this }.ShowDialog();
        }

        private void MenuStudentManagement_Click(object sender, RoutedEventArgs e)
        {
            if (!RequireAdminSession()) return;
            new StudentManagementWindow { Owner = this }.ShowDialog();
        }

        private void MenuAddNewSubject_Click(object sender, RoutedEventArgs e)
        {
            if (!RequireAdminSession()) return;
            new AddSubjectWindow { Owner = this }.ShowDialog();
        }

        private void MenuChangeAdminPassword_Click(object sender, RoutedEventArgs e)
        {
            if (!RequireAdminSession()) return;
            new ChangeAdminPasswordWindow { Owner = this }.ShowDialog();
        }

        private void MenuViewMyGrades_Click(object sender, RoutedEventArgs e)
        {
            if (!RequireStudentSession()) return;

            Student? student = LoadStudent(CurrentLoggedInStudent!.StudentID);
            if (student == null)
            {
                MessageBox.Show("Your student account could not be found.", "Account Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CurrentLoggedInStudent = student;
            new ViewGradesWindow(student) { Owner = this }.ShowDialog();
        }

        private void MenuManageSubjects_Click(object sender, RoutedEventArgs e)
        {
            if (!RequireStudentSession()) return;

            Student? student = LoadStudent(CurrentLoggedInStudent!.StudentID);
            if (student == null)
            {
                MessageBox.Show("Your student account could not be found.", "Account Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CurrentLoggedInStudent = student;
            new AssignSubjectsWindow(student, false) { Owner = this }.ShowDialog();
            RefreshCurrentStudentReference();
        }

        private void MenuStudentPassword_Click(object sender, RoutedEventArgs e)
        {
            if (!RequireStudentSession()) return;

            Student? student = LoadStudent(CurrentLoggedInStudent!.StudentID);
            if (student == null)
            {
                MessageBox.Show("Your student account could not be found.", "Account Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CurrentLoggedInStudent = student;
            new StudentPasswordWindow(student) { Owner = this }.ShowDialog();
            RefreshCurrentStudentReference();
        }

        private void MenuEditMyDetails_Click(object sender, RoutedEventArgs e)
        {
            if (!RequireStudentSession()) return;

            Student? student = LoadStudent(CurrentLoggedInStudent!.StudentID);
            if (student == null)
            {
                MessageBox.Show("Your student account could not be found.", "Account Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CurrentLoggedInStudent = student;
            new EditStudentDetailsWindow(student) { Owner = this }.ShowDialog();
            RefreshCurrentStudentReference();
        }

        private static void RefreshCurrentStudentReference()
        {
            if (CurrentLoggedInStudent == null) return;
            CurrentLoggedInStudent = LoadStudent(CurrentLoggedInStudent.StudentID);
        }

        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentLoggedInStudent = null;
            IsAdminSessionActive = false;
            new LoginWindow().Show();
            Close();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}