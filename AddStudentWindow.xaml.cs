using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class AddStudentWindow : Window
{
    public AddStudentWindow()
    {
        InitializeComponent();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(StudentIDTextBox.Text.Trim(), out int newId))
        {
            MessageBox.Show("Invalid Student ID.");
            return;
        }

        if (MainWindow.students.Any(s => s.StudentID == newId))
        {
            MessageBox.Show("Error: Student ID already exists.");
            return;
        }

        string name = NameTextBox.Text.Trim();
        string password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a name.");
            return;
        }

        MainWindow.students = MainWindow.students
            .Append(new Student(newId, name, password))
            .ToArray();

        MainWindow.SaveMemory();
        MessageBox.Show("Student added!");
        Close();
    }
}