using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class AssignSubjectsWindow : Window
{
    private Student? student;

    public AssignSubjectsWindow(Student student, bool adminMode = false)
    {
        InitializeComponent();

        if (MainWindow.IsAdminSessionActive)
        {
            if (!adminMode)
            {
                MessageBox.Show("This window must be opened through the administrator session.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }
        }
        else if (MainWindow.CurrentLoggedInStudent == null ||
                 MainWindow.CurrentLoggedInStudent.StudentID != student.StudentID)
        {
            MessageBox.Show("Students may only manage their own offered subjects.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        MainWindow.LoadStudents();
        MainWindow.LoadSubjects();
        this.student = MainWindow.students.FirstOrDefault(s => s.StudentID == student.StudentID);

        if (this.student == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        TitleTextBlock.Text = $"Managing Subjects for {this.student.Name}";
    }

    private void Offer_Click(object sender, RoutedEventArgs e)
    {
        if (student == null) return;
        MainWindow.LoadSubjects();

        var offeredIds = student.OfferedSubjectIDs ?? new System.Collections.Generic.List<int>();
        var available = MainWindow.subjects
            .Where(s => !offeredIds.Contains(s.SubjectID))
            .ToArray();

        if (available.Length == 0)
        {
            MessageBox.Show("No new subjects available to offer.");
            return;
        }

        var window = new SubjectChoiceWindow("Available subjects to offer", available) { Owner = this };
        if (window.ShowDialog() == true && window.SelectedSubject != null)
        {
            offeredIds.Add(window.SelectedSubject.SubjectID);
            student.OfferedSubjectIDs = offeredIds;
            MainWindow.SaveStudents();
            MessageBox.Show("Subject added to offerings successfully!");
        }
    }

    private void Drop_Click(object sender, RoutedEventArgs e)
    {
        if (student == null) return;
        MainWindow.LoadSubjects();
        MainWindow.LoadScores();

        var offeredIds = student.OfferedSubjectIDs ?? new System.Collections.Generic.List<int>();
        if (offeredIds.Count == 0)
        {
            MessageBox.Show("This student isn't offering any subjects to drop.");
            return;
        }

        var offered = offeredIds
            .Select(id => MainWindow.subjects.FirstOrDefault(s => s.SubjectID == id))
            .Where(s => s != null)
            .Cast<Subject>()
            .ToArray();

        var window = new SubjectChoiceWindow("Currently offered subjects", offered) { Owner = this };
        if (window.ShowDialog() == true && window.SelectedSubject != null)
        {
            int subjectId = window.SelectedSubject.SubjectID;
            offeredIds.Remove(subjectId);
            student.OfferedSubjectIDs = offeredIds;

            MainWindow.scores = MainWindow.scores
                .Where(s => !(s.StudentID == student.StudentID && s.SubjectID == subjectId))
                .ToArray();

            MainWindow.SaveStudents();
            MainWindow.SaveScores();
            MessageBox.Show("Subject dropped successfully!");
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Close();
}