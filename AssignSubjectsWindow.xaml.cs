using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class AssignSubjectsWindow : Window
{
    private readonly Student student;

    public AssignSubjectsWindow(Student student)
    {
        InitializeComponent();
        this.student = student;
        TitleTextBlock.Text = $"Managing Subjects for {student.Name}";
    }

    private void Offer_Click(object sender, RoutedEventArgs e)
    {
        var available = MainWindow.subjects
            .Where(s => !student.OfferedSubjectIDs.Contains(s.SubjectID))
            .ToArray();

        if (available.Length == 0)
        {
            MessageBox.Show("No new subjects available to offer.");
            return;
        }

        var window = new SubjectChoiceWindow("Available subjects to offer", available) { Owner = this };
        if (window.ShowDialog() == true && window.SelectedSubject != null)
        {
            student.OfferedSubjectIDs.Add(window.SelectedSubject.SubjectID);
            MainWindow.SaveMemory();
            MessageBox.Show("Subject added to offerings successfully!");
        }
    }

    private void Drop_Click(object sender, RoutedEventArgs e)
    {
        if (student.OfferedSubjectIDs.Count == 0)
        {
            MessageBox.Show("This student isn't offering any subjects to drop.");
            return;
        }

        var offered = student.OfferedSubjectIDs
            .Select(id => MainWindow.subjects.FirstOrDefault(s => s.SubjectID == id))
            .Where(s => s != null)
            .Cast<Subject>()
            .ToArray();

        var window = new SubjectChoiceWindow("Currently offered subjects", offered) { Owner = this };
        if (window.ShowDialog() == true && window.SelectedSubject != null)
        {
            int subjectId = window.SelectedSubject.SubjectID;
            student.OfferedSubjectIDs.Remove(subjectId);
            MainWindow.scores = MainWindow.scores
                .Where(s => !(s.StudentID == student.StudentID && s.SubjectID == subjectId))
                .ToArray();
            MainWindow.SaveMemory();
            MessageBox.Show("Subject dropped successfully!");
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Close();
}