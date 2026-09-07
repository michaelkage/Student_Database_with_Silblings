using System.Windows;

namespace StudentManagementApp;

public partial class StudentManagementWindow : Window
{
    private Student[]? loadedStudents;

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
        loadedStudents = DataStore.LoadStudents();
        StudentComboBox.ItemsSource = null;
        StudentComboBox.ItemsSource = loadedStudents;

        if (loadedStudents.Length > 0)
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

        Student? selected = SelectedStudent;
        if (selected == null)
        {
            MessageBox.Show("Please select a student.");
            return;
        }

        new EditResultWindow(selected) { Owner = this }.ShowDialog();
        selected = null;
        LoadStudents();
    }

    private void ManageSubjects_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Student? selected = SelectedStudent;
        if (selected == null)
        {
            MessageBox.Show("Please select a student.");
            return;
        }

        new AssignSubjectsWindow(selected, true) { Owner = this }.ShowDialog();
        selected = null;
        LoadStudents();
    }

    private void RemoveStudent_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Student? selected = SelectedStudent;
        if (selected == null)
        {
            MessageBox.Show("Please select a student.");
            return;
        }

        var confirmation = MessageBox.Show(
            $"Are you sure you want to delete {selected.Name}?",
            "Confirm deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
        {
            MessageBox.Show("Deletion cancelled.");
            return;
        }

        int studentId = selected.StudentID;
        selected = null;

        if (DataStore.DeleteStudent(studentId))
            MessageBox.Show("Student and all associated records deleted successfully!");
        else
            MessageBox.Show("Student account could not be found.", "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Warning);

        LoadStudents();
    }

    private void AddStudent_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        new AddStudentWindow { Owner = this }.ShowDialog();
        LoadStudents();
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        StudentComboBox.ItemsSource = null;
        loadedStudents = null;
        base.OnClosed(e);
    }
}