using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class AddSubjectWindow : Window
{
    public AddSubjectWindow()
    {
        InitializeComponent();
        SubjectIDTextBox.IsReadOnly = true;
        SubjectIDTextBox.Text = MainWindow.GetNextSubjectID().ToString();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string name = SubjectNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a subject name.");
            return;
        }

        int newId = MainWindow.GetNextSubjectID();

        MainWindow.subjects = MainWindow.subjects
            .Append(new Subject(newId, name))
            .ToArray();

        MainWindow.SaveMemory();
        MessageBox.Show($"Subject added! Subject ID: {newId}");
        Close();
    }
}