using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class AddSubjectWindow : Window
{
    public AddSubjectWindow()
    {
        InitializeComponent();

        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        SubjectIDTextBox.IsReadOnly = true;
        SubjectIDTextBox.Text = MainWindow.GetNextSubjectID().ToString();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string name = SubjectNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a subject name.");
            return;
        }

        MainWindow.LoadSubjects();
        int newId = MainWindow.GetNextSubjectID();

        MainWindow.subjects = MainWindow.subjects
            .Append(new Subject(newId, name))
            .ToArray();

        MainWindow.SaveSubjects();
        MessageBox.Show($"Subject added! Subject ID: {newId}");
        Close();
    }
}