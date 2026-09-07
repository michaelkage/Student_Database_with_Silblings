using System.Windows;

namespace StudentManagementApp
{
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

            if (RadioAdmin.IsChecked == true)
            {
                MainWindow.LoadAdminPassword();

                if (password == MainWindow.AdminPassword)
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

            if (!int.TryParse(username, out int studentId))
            {
                MessageBox.Show("Invalid ID format.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Student? student = MainWindow.LoadStudent(studentId);
            if (student != null && student.StudentPassword == password)
            {
                MainWindow.CurrentLoggedInStudent = student;
                new MainWindow(false).Show();
                Close();
            }
            else
            {
                MessageBox.Show("Invalid Student ID or password. Access denied.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}