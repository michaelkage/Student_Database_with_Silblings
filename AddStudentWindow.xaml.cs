using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class AddStudentWindow : Window
{
    public AddStudentWindow()
    {
        InitializeComponent();
        StudentIDTextBox.IsReadOnly = true;
        StudentIDTextBox.Text = GetNextStudentId().ToString();
    }

    private static int GetNextStudentId()
    {
        return MainWindow.students.Length == 0
            ? 1
            : MainWindow.students.Max(s => s.StudentID) + 1;
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

        int newId = GetNextStudentId();

        MainWindow.students = MainWindow.students
            .Append(new Student(newId, name, password))
            .ToArray();

        MainWindow.SaveMemory();
        MessageBox.Show($"Student added! Student ID: {newId}");
        Close();
    }
}