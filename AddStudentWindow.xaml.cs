using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class AddStudentWindow : Window
{
    public AddStudentWindow()
    {
        InitializeComponent();
        StudentIDTextBox.IsReadOnly = true;
        StudentIDTextBox.Text = MainWindow.GetNextStudentID().ToString();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
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

        int newId = MainWindow.GetNextStudentID();

        MainWindow.students = MainWindow.students
            .Append(new Student(newId, name, password))
            .ToArray();

        MainWindow.SaveMemory();
        MessageBox.Show($"Student added! Student ID: {newId}");
        Close();
    }
}