using System.Windows;
using System.IO;

namespace StudentManagementApp;

public partial class AddStudentWindow : Window
{
    public AddStudentWindow()
    {
        InitializeComponent();

        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        StudentIDTextBox.IsReadOnly = true;
        StudentIDTextBox.Text = "Assigned on save";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string name = NameTextBox.Text.Trim();
        string password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a name.");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Please enter a password.");
            return;
        }

        try
        {
            Student created = DataStore.CreateStudent(name, password);
            StudentIDTextBox.Text = created.StudentID.ToString();
            MessageBox.Show($"Student added! Student ID: {created.StudentID}");
            created = null!;
            Close();
        }
        catch (IOException ex)
        {
            MessageBox.Show($"The student could not be saved.\n\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}