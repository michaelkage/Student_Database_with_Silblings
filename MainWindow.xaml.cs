using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace StudentManagementApp
{
    // Made public so sibling windows can see it
    public class Result
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }
        public int Score { get; set; }

        public Result(int studentId, int subjectId, int score)
        {
            StudentID = studentId;
            SubjectID = subjectId;
            Score = score;
        }
    }

    // Made public so sibling windows can see it
    public class Student
    {
        public int StudentID { get; set; }
        public string StudentPassword { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public List<int> OfferedSubjectIDs { get; set; } = new List<int>();

        public Student(int id, string firstName, string surname, string password)
        {
            StudentID = id;
            FirstName = firstName;
            Surname = surname;
            StudentPassword = password;
        }
    }

    // Made public so sibling windows can see it
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
        // Keeping central backend arrays active 
        public static Student[] students = new Student[0];
        public static Subject[] subjects = new Subject[0];
        public static Result[] scores = new Result[0]; // Renamed Score to Result array match

        private const string StudentFile = "Student.txt";
        private const string SubjectFile = "Subject.txt";
        private const string ScoresFile = "Scores.txt";
        private const string PasswordFile = "AdminPassword.txt";
        public static string AdminPassword { get; set; } = "Messi";

        public static Student CurrentLoggedInStudent { get; set; }
        public bool isAdminSession = false;

        // 3. FIXED CONSTRUCTOR SIGNATURE TO MATCH LOGINWINDOW EXPECTATIONS
        public MainWindow(bool isAdmin)
        {
            InitializeComponent();
            LoadMemory();
            this.isAdminSession = isAdmin;
            UnlockDashboard(isAdmin);
        }

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
                    TxtWelcomeHeadline.Text = $"Welcome Back, {CurrentLoggedInStudent.FirstName} {CurrentLoggedInStudent.Surname} [ID: {CurrentLoggedInStudent.StudentID}]";
                }
            }
        }

        private void LoadMemory()
        {
            if (File.Exists(PasswordFile))
                AdminPassword = File.ReadAllText(PasswordFile).Trim();

            if (File.Exists(StudentFile))
            {
                var loadedStudents = new List<Student>();
                foreach (var line in File.ReadAllLines(StudentFile))
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 4 && int.TryParse(parts[0], out int id))
                    {
                        string fName = parts[1];
                        string sName = parts[2];
                        string password = parts[3];
                        var student = new Student(id, fName, sName, password);
                        if (parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]))
                        {
                            student.OfferedSubjectIDs = parts
                                .Skip(5)
                                .Select(int.Parse)
                                .ToList();
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
                var loadedScores = new List<Result>();
                foreach (var line in File.ReadAllLines(ScoresFile))
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3 && int.TryParse(parts[0], out int sId) && int.TryParse(parts[1], out int subId) && int.TryParse(parts[2], out int scoreVal))
                    {
                        loadedScores.Add(new Result(sId, subId, scoreVal));
                    }
                }
                scores = loadedScores.ToArray();
            }
        }

        private void SaveMemory()
        {
            var studentLines = students.Select(s =>
            {
                string subjectsString = s.OfferedSubjectIDs != null && s.OfferedSubjectIDs.Count > 0
                    ? string.Join(",", s.OfferedSubjectIDs) : "";
                return $"{s.StudentID},{s.FirstName},{s.Surname},{s.StudentPassword},{s.OfferedSubjectIDs.Count},{subjectsString}".TrimEnd(',');
            });
            File.WriteAllLines(StudentFile, studentLines);
            File.WriteAllLines(SubjectFile, subjects.Select(s => $"{s.SubjectID},{s.SubjectName}"));
            File.WriteAllLines(ScoresFile, scores.Select(s => $"{s.StudentID},{s.SubjectID},{s.Score}"));
            File.WriteAllText(PasswordFile, AdminPassword);
        }

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

        private void MenuAddNewStudent_Click(object sender, RoutedEventArgs e)
        {
            int autoGeneratedId = students.Length > 0 ? students.Max(s => s.StudentID) + 1 : 1;

            string fName = Microsoft.VisualBasic.Interaction.InputBox("Enter Student First Name:", "Add Student Control", "");
            string sName = Microsoft.VisualBasic.Interaction.InputBox("Enter Student Surname:", "Add Student Control", "");
            string pass = Microsoft.VisualBasic.Interaction.InputBox("Enter Student Password:", "Add Student Control", "1234");

            if (!string.IsNullOrWhiteSpace(fName) && !string.IsNullOrWhiteSpace(sName))
            {
                // 1. Add to the data array
                students = students.Append(new Student(autoGeneratedId, fName, sName, pass)).ToArray();
                SaveMemory();

                // 2. Force the UI Windows to reload the fresh data array
                MessageBox.Show($"Student successfully created with Auto-ID: {autoGeneratedId}", "Success");
            }
        }

        private void MenuAddNewSubject_Click(object sender, RoutedEventArgs e)
        {
            int autoGeneratedSubId = subjects.Length > 0 ? subjects.Max(s => s.SubjectID) + 1 : 1;

            string name = Microsoft.VisualBasic.Interaction.InputBox("Enter New Subject Name:", "Add Subject Control", "");
            if (!string.IsNullOrWhiteSpace(name))
            {
                subjects = subjects.Append(new Subject(autoGeneratedSubId, name)).ToArray();
                SaveMemory();
                MessageBox.Show($"Subject successfully created with Auto-ID: {autoGeneratedSubId}", "Success");
            }
        }

        private void MenuViewAllResults_Click(object sender, RoutedEventArgs e)
        {
            ViewStudentsWindow viewStudentsWin = new ViewStudentsWindow();
            viewStudentsWin.Owner = this;
            viewStudentsWin.ShowDialog();
        }

        private void MenuModifyStudent_Click(object sender, RoutedEventArgs e)
        {
            EditResultWindow editWin = new EditResultWindow();
            editWin.Owner = this;
            editWin.ShowDialog();
        }

        private void MenuViewMyGrades_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Grades statement module loading next!");
        }

        private void MenuEditMyDetails_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Self detail edit utility module loading next!");
        }
    }
}