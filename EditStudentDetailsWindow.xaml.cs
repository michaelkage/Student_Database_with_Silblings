using System.Windows;

namespace StudentManagementApp;

public partial class EditStudentDetailsWindow : Window
{
    private readonly Student student;

    public EditStudentDetailsWindow(Student student)
    {
        InitializeComponent();
        this.student = student;
        PromptTextBlock.Text = $"Enter new name (leave blank to keep '{student.Name}'):";
        NameTextBox.Text = "";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        string enteredName = NameTextBox.Text;
        string newName = enteredName.Trim();

        // Preserve the console behavior: an actually blank field keeps the current name.
        // Whitespace-only input, however, is an attempted replacement and is rejected.
        if (enteredName.Length > 0 && string.IsNullOrWhiteSpace(enteredName))
        {
            MessageBox.Show("Name cannot contain only spaces.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(newName))
            student.Name = newName;

        MainWindow.SaveMemory();
        MessageBox.Show("Student details updated successfully!");
        Close();
    }
}