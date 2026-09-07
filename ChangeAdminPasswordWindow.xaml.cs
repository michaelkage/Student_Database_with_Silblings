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

        MainWindow.LoadAdminPassword();

        if (CurrentPasswordBox.Password != MainWindow.AdminPassword)
        {
            MessageBox.Show("Access Denied.");
            return;
        }

        string newPass = NewPasswordBox.Password;
        if (string.IsNullOrWhiteSpace(newPass))
        {
            MessageBox.Show("Please enter a new password.");
            return;
        }

        MainWindow.SetAdminPassword(newPass);
        MessageBox.Show("Password saved permanently!");
        Close();
    }
}