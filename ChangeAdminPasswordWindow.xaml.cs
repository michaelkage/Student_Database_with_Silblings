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
        // 1. Session verification check
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string currentPassInput = CurrentPasswordBox.Password;
        string newPassInput = NewPasswordBox.Password;

        // 2. Structural data checks
        if (string.IsNullOrWhiteSpace(newPassInput))
        {
            MessageBox.Show("Please enter a new password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // 3. Send both to the engine. It verifies the old password AND saves the new one in one single transaction!
        if (!DataStore.ChangeAdminPassword(newPassInput, currentPassInput))
        {
            MessageBox.Show("Access Denied or password could not be updated. Please verify your current password.", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        MessageBox.Show("Password saved permanently!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }
}