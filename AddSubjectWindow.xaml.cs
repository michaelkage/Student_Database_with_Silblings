using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class AddSubjectWindow : Window
{
    public AddSubjectWindow()
    {
        InitializeComponent();
        SubjectIDTextBox.IsReadOnly = true;
        SubjectIDTextBox.Text = GetNextSubjectId().ToString();
    }

    private static int GetNextSubjectId()
    {
        return MainWindow.subjects.Length == 0
            ? 1
            : MainWindow.subjects.Max(s => s.SubjectID) + 1;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string name = SubjectNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a subject name.");
            return;
        }

        int newId = GetNextSubjectId();

        MainWindow.subjects = MainWindow.subjects
            .Append(new Subject(newId, name))
            .ToArray();

        MainWindow.SaveMemory();
        MessageBox.Show($"Subject added! Subject ID: {newId}");
        Close();
    }
}