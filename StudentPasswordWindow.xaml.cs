using System.Windows;

namespace StudentManagementApp;

public partial class StudentPasswordWindow : Window
{
    private Student? student;

    public StudentPasswordWindow(Student student)
    {
        InitializeComponent();

        if (MainWindow.CurrentLoggedInStudent?.StudentID != student.StudentID || MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Students may only change their own password.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        this.student = student;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (student == null || MainWindow.IsAdminSessionActive || MainWindow.CurrentLoggedInStudent?.StudentID != student.StudentID)
        {
            MessageBox.Show("Students may only change their own password.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Re-read the account immediately before authentication.
        Student? current = MainWindow.LoadStudent(student.StudentID);
        if (current == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (CurrentPasswordBox.Password != current.StudentPassword)
        {
            MessageBox.Show("Fail :(");
            return;
        }

        string newPass = NewPasswordBox.Password;
        if (string.IsNullOrWhiteSpace(newPass))
        {
            MessageBox.Show("Please enter a new password.");
            return;
        }

        current.StudentPassword = newPass;
        MainWindow.SaveStudents();
        MainWindow.CurrentLoggedInStudent = current;
        MessageBox.Show("Success!!!");
        Close();
    }
}