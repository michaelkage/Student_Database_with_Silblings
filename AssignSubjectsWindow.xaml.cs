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

        this.student = DataStore.LoadStudent(student.StudentID);
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
        if (!HasValidSession() || student == null)
            return;

        int studentId = student.StudentID;
        Student? current = DataStore.LoadStudent(studentId);
        Subject[] allSubjects = DataStore.LoadSubjects();

        if (current == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            allSubjects = null!;
            return;
        }

        List<int> offeredIds = current.OfferedSubjectIDs ?? new List<int>();
        Subject[] available = allSubjects
            .Where(subject => !offeredIds.Contains(subject.SubjectID))
            .ToArray();

        if (available.Length == 0)
        {
            MessageBox.Show("No new subjects available to offer.");
            current = null;
            allSubjects = null!;
            available = null!;
            return;
        }

        var window = new SubjectChoiceWindow("Available subjects to offer", available) { Owner = this };
        if (window.ShowDialog() == true && window.SelectedSubject != null)
        {
            int subjectId = window.SelectedSubject.SubjectID;
            offeredIds.Add(subjectId);
            if (DataStore.UpdateStudentSubjects(studentId, offeredIds))
                MessageBox.Show("Subject added to offerings successfully!");
        }

        window = null!;
        current = null;
        allSubjects = null!;
        available = null!;
    }

    private void Drop_Click(object sender, RoutedEventArgs e)
    {
        if (!HasValidSession() || student == null)
            return;

        int studentId = student.StudentID;
        Student? current = DataStore.LoadStudent(studentId);
        Subject[] allSubjects = DataStore.LoadSubjects();

        if (current == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            allSubjects = null!;
            return;
        }

        List<int> offeredIds = current.OfferedSubjectIDs ?? new List<int>();
        if (offeredIds.Count == 0)
        {
            MessageBox.Show("This student isn't offering any subjects to drop.");
            current = null;
            allSubjects = null!;
            return;
        }

        Subject[] offered = offeredIds
            .Select(id => allSubjects.FirstOrDefault(subject => subject.SubjectID == id))
            .Where(subject => subject != null)
            .Cast<Subject>()
            .ToArray();

        var window = new SubjectChoiceWindow("Currently offered subjects", offered) { Owner = this };
        if (window.ShowDialog() == true && window.SelectedSubject != null)
        {
            int subjectId = window.SelectedSubject.SubjectID;
            offeredIds.Remove(subjectId);

            // Remove the score while the subject is still offered, because SaveGrade validates
            // the relationship. Then persist the new offered-subject list.
            DataStore.SaveGrade(studentId, subjectId, null);
            if (DataStore.UpdateStudentSubjects(studentId, offeredIds))
                MessageBox.Show("Subject dropped successfully!");
        }

        window = null!;
        current = null;
        allSubjects = null!;
        offered = null!;
    }

    private bool HasValidSession()
    {
        if (MainWindow.IsAdminSessionActive)
            return true;

        if (student == null || MainWindow.CurrentLoggedInStudent == null ||
            MainWindow.CurrentLoggedInStudent.StudentID != student.StudentID)
        {
            MessageBox.Show("Students may only manage their own offered subjects.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        student = null;
        base.OnClosed(e);
    }
}