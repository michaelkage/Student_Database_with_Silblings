using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class SubjectChoiceWindow : Window
{
    public Subject? SelectedSubject { get; private set; }

    private class ChoiceItem
    {
        public Subject Subject { get; }
        public string DisplayName => $"ID: {Subject.SubjectID} | Name: {Subject.SubjectName}";
        public ChoiceItem(Subject subject) => Subject = subject;
    }

    public SubjectChoiceWindow(string heading, IEnumerable<Subject> subjects)
    {
        InitializeComponent();
        HeadingTextBlock.Text = heading;
        SubjectsListBox.ItemsSource = subjects.Select(s => new ChoiceItem(s)).ToList();
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        if (SubjectsListBox.SelectedItem is not ChoiceItem item)
        {
            MessageBox.Show("Invalid Subject ID.");
            return;
        }
        SelectedSubject = item.Subject;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}