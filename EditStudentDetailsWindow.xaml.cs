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
        string newName = NameTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(newName))
            student.Name = newName;

        MainWindow.SaveMemory();
        MessageBox.Show("Student details updated successfully!");
        Close();
    }
}