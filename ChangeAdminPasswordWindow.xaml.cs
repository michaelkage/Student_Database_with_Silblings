using System.Windows;

namespace StudentManagementApp;

public partial class ChangeAdminPasswordWindow : Window
{
    public ChangeAdminPasswordWindow() => InitializeComponent();

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPasswordBox.Password == MainWindow.AdminPassword)
        {
            string newPass = NewPasswordBox.Password;
            if (!string.IsNullOrWhiteSpace(newPass))
            {
                MainWindow.AdminPassword = newPass;
                MainWindow.SaveMemory();
                MessageBox.Show("Password saved permanently!");
                Close();
            }
        }
        else
        {
            MessageBox.Show("Access Denied.");
        }
    }
}