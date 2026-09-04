using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class StudentManagementWindow : Window
{
    public StudentManagementWindow()
    {
        InitializeComponent();
        LoadStudents();
    }

    private void LoadStudents()
    {
        StudentComboBox.ItemsSource = null;
        StudentComboBox.ItemsSource = MainWindow.students;
        if (MainWindow.students.Length > 0)
            StudentComboBox.SelectedIndex = 0;
    }

    private Student? SelectedStudent => StudentComboBox.SelectedItem as Student;

    private void ModifyGrade_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedStudent == null)
        {
            MessageBox.Show("Please select a student.");
            return;
        }
        var window = new EditResultWindow(SelectedStudent) { Owner = this };
        window.ShowDialog();
    }

    private void ManageSubjects_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedStudent == null)
        {
            MessageBox.Show("Please select a student.");
            return;
        }
        var window = new AssignSubjectsWindow(SelectedStudent) { Owner = this };
        window.ShowDialog();
    }

    private void RemoveStudent_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedStudent == null)
        {
            MessageBox.Show("Please select a student.");
            return;
        }

        var confirmation = MessageBox.Show(
            $"Are you sure you want to delete {SelectedStudent.Name}?",
            "Confirm deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
        {
            MessageBox.Show("deletion Cancelled");
            return;
        }

        int id = SelectedStudent.StudentID;
        MainWindow.students = MainWindow.students.Where(s => s.StudentID != id).ToArray();
        MainWindow.scores = MainWindow.scores.Where(s => s.StudentID != id).ToArray();
        MainWindow.SaveMemory();
        MessageBox.Show("Student and all associated records deleted successfully!");
        LoadStudents();
    }

    private void AddStudent_Click(object sender, RoutedEventArgs e)
    {
        var window = new AddStudentWindow { Owner = this };
        window.ShowDialog();
        LoadStudents();
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Close();
}