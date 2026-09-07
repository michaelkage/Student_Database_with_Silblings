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

        int studentId = student.StudentID;
        Student? current = DataStore.LoadStudent(studentId);
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
            current = null;
            return;
        }

        if (!string.IsNullOrWhiteSpace(newName))
        {
            if (!DataStore.ChangeStudentName(studentId, newName))
            {
                MessageBox.Show("Your student account could not be updated.", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                current = null;
                return;
            }
        }

        MainWindow.CurrentLoggedInStudent = DataStore.LoadStudent(studentId);
        current = null;
        MessageBox.Show("Student details updated successfully!");
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        student = null;
        base.OnClosed(e);
    }
}