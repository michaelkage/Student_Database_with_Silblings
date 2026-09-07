using System.Windows;

namespace StudentManagementApp;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        string username = TxtUserId.Text.Trim();
        string password = TxtPassword.Password;

        // --- ADMIN LOGIN PATH ---
        if (RadioAdmin.IsChecked == true)
        {
            // Direct boolean verification: The true admin password never enters UI RAM!
            if (DataStore.VerifyAdminPassword(password))
            {
                MainWindow.CurrentLoggedInStudent = null;
                new MainWindow(true).Show();
                Close();
            }
            else
            {
                MessageBox.Show("Wrong!!!!", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return;
        }

        // --- STUDENT LOGIN PATH ---
        if (!int.TryParse(username, out int studentId))
        {
            MessageBox.Show("Invalid ID format.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Transient student record check
        Student? student = DataStore.LoadStudent(studentId);
        bool authenticated = student != null && student.StudentPassword == password;

        if (authenticated && student != null)
        {
            MainWindow.CurrentLoggedInStudent = student;
            new MainWindow(false).Show();
            Close();
        }
        else
        {
            MessageBox.Show("Invalid Student ID or password. Access denied.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Immediate cleanup of the transient object reference for the Garbage Collector
        student = null;
    }
}