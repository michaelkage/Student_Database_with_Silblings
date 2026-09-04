using System;
using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class AddSubjectWindow : Window
{
    public AddSubjectWindow()
    {
        InitializeComponent();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(SubjectIDTextBox.Text.Trim(), out int newId))
        {
            MessageBox.Show("Invalid Subject ID.");
            return;
        }

        if (MainWindow.subjects.Any(s => s.SubjectID == newId))
        {
            MessageBox.Show("Error: Subject ID already exists.");
            return;
        }

        string name = SubjectNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a subject name.");
            return;
        }

        MainWindow.subjects = MainWindow.subjects
            .Append(new Subject(newId, name))
            .ToArray();

        MainWindow.SaveMemory();
        MessageBox.Show("Subject added!");
        Close();
    }
}