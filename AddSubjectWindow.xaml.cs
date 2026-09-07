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
        SubjectIDTextBox.Text = "Assigned on save";
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

        try
        {
            Subject created = DataStore.CreateSubject(name);
            SubjectIDTextBox.Text = created.SubjectID.ToString();
            MessageBox.Show($"Subject added! Subject ID: {created.SubjectID}");
            created = null!;
            Close();
        }
        catch (IOException ex)
        {
            MessageBox.Show($"The subject could not be saved.\n\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}