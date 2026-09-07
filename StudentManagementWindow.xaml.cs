using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class StudentManagementWindow : Window
{
    public StudentManagementWindow()
    {
        InitializeComponent();

        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Student management is available to administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        LoadStudents();
    }

    private void LoadStudents()
    {
        MainWindow.LoadStudents();
        StudentComboBox.ItemsSource = null;
        StudentComboBox.ItemsSource = MainWindow.students;

        if (MainWindow.students.Length > 0)
            StudentComboBox.SelectedIndex = 0;
        else
            MessageBox.Show("No students registered.");
    }

    private Student? SelectedStudent => StudentComboBox.SelectedItem as Student;

    private void ModifyGrade_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedStudent == null)
        {
            MessageBox.Show("Please select a student.");
            return;
        }

        var window = new EditResultWindow(SelectedStudent) { Owner = this };
        window.ShowDialog();
        LoadStudents();
    }

    private void ManageSubjects_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedStudent == null)
        {
            MessageBox.Show("Please select a student.");
            return;
        }

        var window = new AssignSubjectsWindow(SelectedStudent, true) { Owner = this };
        window.ShowDialog();
        LoadStudents();
    }

    private void RemoveStudent_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

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
            MessageBox.Show("Deletion cancelled.");
            return;
        }

        int id = SelectedStudent.StudentID;
        MainWindow.LoadStudents();
        MainWindow.LoadSubjects();
        MainWindow.LoadScores();
        MainWindow.students = MainWindow.students.Where(student => student.StudentID != id).ToArray();
        MainWindow.scores = MainWindow.scores.Where(score => score.StudentID != id).ToArray();
        MainWindow.SaveStudents();
        MainWindow.SaveScores();

        MessageBox.Show("Student and all associated records deleted successfully!");
        LoadStudents();
    }

    private void AddStudent_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var window = new AddStudentWindow { Owner = this };
        window.ShowDialog();
        LoadStudents();
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Close();
}