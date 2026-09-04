using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace StudentManagementApp
{
    // ==========================================
    // CORE PERSISTENT DATA MODELS (ORIGINAL SPEC)
    // ==========================================

    public class Score
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public int Grade { get; set; } // Your Dad's original property

        public Score(int studentId, int subjectId, int grade)
        {
            StudentID = studentId;
            SubjectID = subjectId;
            Grade = grade;
        }
    }

    // THE COMPATIBILITY BRIDGE: Maps sibling window property calls directly back to your Dad's Grade
    public class Result : Score
    {
        public int Score
        {
            get => Grade;
            set => Grade = value;
        }

        public Result(int studentId, int subjectId, int grade) : base(studentId, subjectId, grade)
        {
        }
    }

    public class Student
    {
        public int StudentID { get; set; }
        public string StudentPassword { get; set; }
        public string Name { get; set; } // Your Dad's original property
        public List<int> OfferedSubjectIDs { get; set; } = new List<int>();

        // SIBLING COMPATIBILITY MIRRORS: Automatically maps FirstName and Surname to Name split patterns
        public string FirstName
        {
            get => string.IsNullOrWhiteSpace(Name) ? "" : Name.Split(' ')[0];
            set => Name = $"{value} {Surname}".Trim();
        }

        public string Surname
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return "";
                var parts = Name.Split(' ');
                return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
            }
            set => Name = $"{FirstName} {value}".Trim();
        }

        // Base matching constructor
        public Student(int id, string name, string password)
        {
            StudentID = id;
            Name = name;
            StudentPassword = password;
        }

        // Sibling constructor override (Fixes AddStudentWindow error)
        public Student(int id, string firstName, string surname, string password)
        {
            StudentID = id;
            Name = $"{firstName} {surname}".Trim();
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

    // ==========================================
    // MAIN APPLICATION WINDOW INTERACTION ENGINE
    // ==========================================

    public partial class MainWindow : Window
    {
        // Central database memory arrays preserved intact
        public static Student[] students = new Student[0];
        public static Subject[] subjects = new Subject[0];
        public static Score[] scores = new Score[0];

        private const string StudentFile = "Student.txt";
        private const string SubjectFile = "Subject.txt";
        private const string ScoresFile = "Scores.txt";
        private const string PasswordFile = "AdminPassword.txt";

        public static string AdminPassword { get; set; } = "Messi";
        public static Student CurrentLoggedInStudent { get; set; }
        public bool isAdminSession = false;

        public MainWindow(bool isAdmin)
        {
            InitializeComponent();
            LoadMemory(); // Instantly map local file records into memory on window construction
            this.isAdminSession = isAdmin;
            UnlockDashboard(isAdmin);
        }

        // --- GATEWAY PORTAL LOGIN HANDLER ---
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usernameInput = TxtUsername.Text.Trim();
            string passwordInput = TxtPassword.Password;

            if (RadioAdmin.IsChecked == true)
            {
                if (passwordInput == AdminPassword)
                {
                    isAdminSession = true;
                    CurrentLoggedInStudent = null;
                    UnlockDashboard(true);
                }
                else
                {
                    MessageBox.Show("Wrong Admin Password!", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                if (int.TryParse(usernameInput, out int studentId))
                {
                    Student student = students.FirstOrDefault(s => s.StudentID == studentId);
                    if (student != null && student.StudentPassword == passwordInput)
                    {
                        isAdminSession = false;
                        CurrentLoggedInStudent = student;
                        UnlockDashboard(false);
                    }
                    else
                    {
                        MessageBox.Show("Invalid Student ID or password.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please type a valid numeric Student ID.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void UnlockDashboard(bool isAdmin)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            AppMenuBar.Visibility = Visibility.Visible;
            WorkspacePanel.Visibility = Visibility.Visible;

            if (isAdmin)
            {
                MenuAdminOptions.Visibility = Visibility.Visible;
                MenuStudentOptions.Visibility = Visibility.Collapsed;
                TxtWelcomeHeadline.Text = "Welcome to the Admin Control Panel";
            }
            else
            {
                MenuAdminOptions.Visibility = Visibility.Collapsed;
                MenuStudentOptions.Visibility = Visibility.Visible;
                if (CurrentLoggedInStudent != null)
                {
                    TxtWelcomeHeadline.Text = $"Welcome Back, {CurrentLoggedInStudent.Name} [ID: {CurrentLoggedInStudent.StudentID}]";
                }
            }
        }

        // --- ORIGINAL PRESERVED LOCAL STORAGE UTILITY OPERATIONS ---
        public void LoadMemory()
        {
            if (File.Exists(PasswordFile))
                AdminPassword = File.ReadAllText(PasswordFile).Trim();

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
                        if (parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]))
                        {
                            student.OfferedSubjectIDs = parts.Skip(4).Select(int.Parse).ToList();
                        }
                        loadedStudents.Add(student);
                    }
                }
                students = loadedStudents.ToArray();
            }

            if (File.Exists(SubjectFile))
            {
                subjects = File.ReadAllLines(SubjectFile)
                    .Select(line => line.Split(','))
                    .Where(parts => parts.Length == 2 && int.TryParse(parts[0], out _))
                    .Select(parts => new Subject(int.Parse(parts[0]), parts[1])).ToArray();
            }

            if (File.Exists(ScoresFile))
            {
                var loadedScores = new List<Score>();
                foreach (var line in File.ReadAllLines(ScoresFile))
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3 && int.TryParse(parts[0], out int sId) && int.TryParse(parts[1], out int subId) && int.TryParse(parts[2], out int grade))
                    {
                        loadedScores.Add(new Result(sId, subId, grade)); // Re-mapped to result subclass mirror
                    }
                }
                scores = loadedScores.ToArray();
            }
        }

        public void SaveMemory()
        {
            var studentLines = students.Select(s =>
            {
                string subjectsString = s.OfferedSubjectIDs != null && s.OfferedSubjectIDs.Count > 0
                    ? string.Join(",", s.OfferedSubjectIDs) : "";
                return $"{s.StudentID},{s.Name},{s.StudentPassword},{s.OfferedSubjectIDs.Count},{subjectsString}".TrimEnd(',');
            });
            File.WriteAllLines(StudentFile, studentLines);
            File.WriteAllLines(SubjectFile, subjects.Select(s => $"{s.SubjectID},{s.SubjectName}"));
            File.WriteAllLines(ScoresFile, scores.Select(s => $"{s.StudentID},{s.SubjectID},{s.Grade}"));
            File.WriteAllText(PasswordFile, AdminPassword);
        }

        // --- DROPDOWN ACTION TRIGGERS ---
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
            AppMenuBar.Visibility = Visibility.Collapsed;
            WorkspacePanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            TxtUsername.Clear();
            TxtPassword.Clear();
            CurrentLoggedInStudent = null;
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuAddNewStudent_Click(object sender, RoutedEventArgs e) { int autoGeneratedId = students.Length > 0 ? students.Max(s => s.StudentID) + 1 : 1; string name = Microsoft.VisualBasic.Interaction.InputBox("Enter Student Full Name:", "Add Student Registry", ""); string pass = Microsoft.VisualBasic.Interaction.InputBox("Enter Student Password:", "Add Student Registry", "1234"); if (!string.IsNullOrWhiteSpace(name)) { students = students.Append(new Student(autoGeneratedId, name, pass)).ToArray(); SaveMemory(); MessageBox.Show($"Student successfully created with Auto-ID: {autoGeneratedId}", "Registry Action Completed"); } }
        private void MenuAddNewSubject_Click(object sender, RoutedEventArgs e) { int autoGeneratedSubId = subjects.Length > 0 ? subjects.Max(s => s.SubjectID) + 1 : 1; string name = Microsoft.VisualBasic.Interaction.InputBox("Enter New Subject Name:", "Add Subject Registry", ""); if (!string.IsNullOrWhiteSpace(name)) { subjects = subjects.Append(new Subject(autoGeneratedSubId, name)).ToArray(); SaveMemory(); MessageBox.Show($"Subject successfully created with Auto-ID: {autoGeneratedSubId}", "Registry Action Completed"); } }
        private void MenuViewAllResults_Click(object sender, RoutedEventArgs e) { ViewStudentsWindow viewStudentsWin = new ViewStudentsWindow(); viewStudentsWin.Owner = this; viewStudentsWin.ShowDialog(); }
        private void MenuModifyStudent_Click(object sender, RoutedEventArgs e) { EditResultWindow editWin = new EditResultWindow(); editWin.Owner = this; editWin.ShowDialog(); }
        private void MenuViewMyGrades_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Grades sheet visual layout loading next!", "Student Record Profile"); }
        private void MenuEditMyDetails_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Self detail edit utility module loading next!", "Student Record Profile"); }
    }
}