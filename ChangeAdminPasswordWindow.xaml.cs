using System.Windows;

namespace StudentManagementApp;

public partial class ChangeAdminPasswordWindow : Window
{
    public ChangeAdminPasswordWindow()
    {
        InitializeComponent();

        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string currentPassword = DataStore.LoadAdminPassword();
        if (CurrentPasswordBox.Password != currentPassword)
        {
            currentPassword = string.Empty;
            MessageBox.Show("Access Denied.");
            return;
        }

        string newPass = NewPasswordBox.Password;
        if (string.IsNullOrWhiteSpace(newPass))
        {
            currentPassword = string.Empty;
            MessageBox.Show("Please enter a new password.");
            return;
        }

        if (!DataStore.ChangeAdminPassword(newPass, currentPassword))
        {
            currentPassword = string.Empty;
            MessageBox.Show("The password could not be updated.", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        currentPassword = string.Empty;
        MainWindow.LoadAdminPassword();
        MessageBox.Show("Password saved permanently!");
        Close();
    }
}