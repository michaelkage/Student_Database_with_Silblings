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

        int studentId = student.StudentID;
        Student? current = DataStore.LoadStudent(studentId);
        if (current == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (CurrentPasswordBox.Password != current.StudentPassword)
        {
            MessageBox.Show("Fail :(");
            current = null;
            return;
        }

        string newPass = NewPasswordBox.Password;
        if (string.IsNullOrWhiteSpace(newPass))
        {
            MessageBox.Show("Please enter a new password.");
            current = null;
            return;
        }

        if (!DataStore.ChangeStudentPassword(studentId, newPass))
        {
            MessageBox.Show("The student account could not be updated.", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            current = null;
            return;
        }

        MainWindow.CurrentLoggedInStudent = DataStore.LoadStudent(studentId);
        current = null;
        MessageBox.Show("Success!!!");
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        student = null;
        base.OnClosed(e);
    }
}