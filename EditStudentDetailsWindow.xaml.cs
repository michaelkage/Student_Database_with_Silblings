using System.Windows;

namespace StudentManagementApp;

public partial class EditStudentDetailsWindow : Window
{
    private Student? student;

    public EditStudentDetailsWindow(Student student)
    {
        InitializeComponent();

        if (MainWindow.IsAdminSessionActive ||
            MainWindow.CurrentLoggedInStudent?.StudentID != student.StudentID)
        {
            MessageBox.Show("This operation is available to the logged-in student only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        this.student = student;
        PromptTextBlock.Text = $"Enter new name (leave blank to keep '{student.Name}'):";
        NameTextBox.Text = "";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (student == null || MainWindow.IsAdminSessionActive ||
            MainWindow.CurrentLoggedInStudent?.StudentID != student.StudentID)
        {
            MessageBox.Show("This operation is available to the logged-in student only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Reload immediately before mutation so this window cannot overwrite newer account data.
        Student? current = MainWindow.LoadStudent(student.StudentID);
        if (current == null)
        {
            MessageBox.Show("Your student account could not be found.", "Account Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string enteredName = NameTextBox.Text;
        string newName = enteredName.Trim();

        if (enteredName.Length > 0 && string.IsNullOrWhiteSpace(enteredName))
        {
            MessageBox.Show("Name cannot contain only spaces.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(newName))
            current.Name = newName;

        MainWindow.SaveStudents();
        MainWindow.CurrentLoggedInStudent = current;
        MessageBox.Show("Student details updated successfully!");
        Close();
    }
}