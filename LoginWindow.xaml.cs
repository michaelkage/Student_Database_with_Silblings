using System.Windows;
using System.Linq;

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
                if (password == MainWindow.AdminPassword)
                {
                    MainWindow main = new MainWindow(true);
                    main.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Wrong Admin Password!", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                if (int.TryParse(username, out int studentId))
                {
                    var student = MainWindow.students.FirstOrDefault(s => s.StudentID == studentId);
                    if (student != null && student.StudentPassword == password)
                    {
                        MainWindow.CurrentLoggedInStudent = student;
                        MainWindow main = new MainWindow(false);
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Invalid Student ID or password.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a numeric Student ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}