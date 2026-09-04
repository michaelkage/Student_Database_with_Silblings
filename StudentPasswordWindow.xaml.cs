using System.Windows;

namespace StudentManagementApp;

public partial class StudentPasswordWindow : Window
{
    private readonly Student student;

    public StudentPasswordWindow(Student student)
    {
        InitializeComponent();
        this.student = student;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        string password = CurrentPasswordBox.Password;
        if (password == student.StudentPassword)
        {
            string newPass = NewPasswordBox.Password;
            if (string.IsNullOrWhiteSpace(newPass))
            {
                MessageBox.Show("Please enter a new password.");
                return;
            }

            student.StudentPassword = newPass;
            MainWindow.SaveMemory();
            MessageBox.Show("Success!!!");
            Close();
        }
        else
        {
            MessageBox.Show("Fail :(");
        }
    }
}