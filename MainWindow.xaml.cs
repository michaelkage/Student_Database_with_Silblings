using System.Windows;

namespace StudentManagementApp;

public partial class MainWindow : Window
{    
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
        if (!isAdminSession && IsAdminSessionActive == false && CurrentLoggedInStudent != null)
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

        int studentId = CurrentLoggedInStudent!.StudentID;
        Student? student = DataStore.LoadStudent(studentId);
        if (student == null)
        {
            MessageBox.Show("Your student account could not be found.", "Account Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        CurrentLoggedInStudent = student;
        new ViewGradesWindow(student) { Owner = this }.ShowDialog();
        student = null;
        CurrentLoggedInStudent = DataStore.LoadStudent(studentId);
    }

    private void MenuManageSubjects_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireStudentSession()) return;

        int studentId = CurrentLoggedInStudent!.StudentID;
        Student? student = DataStore.LoadStudent(studentId);
        if (student == null)
        {
            MessageBox.Show("Your student account could not be found.", "Account Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        CurrentLoggedInStudent = student;
        new AssignSubjectsWindow(student, false) { Owner = this }.ShowDialog();
        student = null;
        CurrentLoggedInStudent = DataStore.LoadStudent(studentId);
    }

    private void MenuStudentPassword_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireStudentSession()) return;

        int studentId = CurrentLoggedInStudent!.StudentID;
        Student? student = DataStore.LoadStudent(studentId);
        if (student == null)
        {
            MessageBox.Show("Your student account could not be found.", "Account Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        CurrentLoggedInStudent = student;
        new StudentPasswordWindow(student) { Owner = this }.ShowDialog();
        student = null;
        CurrentLoggedInStudent = DataStore.LoadStudent(studentId);
    }

    private void MenuEditMyDetails_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireStudentSession()) return;

        int studentId = CurrentLoggedInStudent!.StudentID;
        Student? student = DataStore.LoadStudent(studentId);
        if (student == null)
        {
            MessageBox.Show("Your student account could not be found.", "Account Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        CurrentLoggedInStudent = student;
        new EditStudentDetailsWindow(student) { Owner = this }.ShowDialog();
        student = null;
        CurrentLoggedInStudent = DataStore.LoadStudent(studentId);
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

    protected override void OnClosed(EventArgs e)
    {
        CurrentLoggedInStudent = null;
        IsAdminSessionActive = false;
        base.OnClosed(e);
    }
}