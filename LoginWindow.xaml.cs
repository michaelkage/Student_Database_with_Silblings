using System.Linq;
using System.Windows;

namespace StudentManagementApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            MainWindow.LoadMemory();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUserId.Text.Trim();
            string password = TxtPassword.Password;

            if (RadioAdmin.IsChecked == true)
            {
                if (password == MainWindow.AdminPassword)
                {
                    MainWindow.CurrentLoggedInStudent = null;
                    var main = new MainWindow(true);
                    main.Show();
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

            var student = MainWindow.students.FirstOrDefault(s => s.StudentID == studentId);
            if (student != null && student.StudentPassword == password)
            {
                MainWindow.CurrentLoggedInStudent = student;
                var main = new MainWindow(false);
                main.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Invalid Student ID or password. Access denied.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}