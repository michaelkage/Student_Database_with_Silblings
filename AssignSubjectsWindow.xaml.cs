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
        Subject[] availableSubjects = DataStore.LoadSubjects();

        if (current == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            availableSubjects = null!;
            return;
        }

        List<int> offeredIds = current.OfferedSubjectIDs ?? new List<int>();
        Subject[] available = availableSubjects
            .Where(subject => !offeredIds.Contains(subject.SubjectID))
            .ToArray();

        if (available.Length == 0)
        {
            MessageBox.Show("No new subjects available to offer.");
            current = null;
            availableSubjects = null!;
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

        window.SelectedSubject = null;
        window = null!;
        current = null;
        availableSubjects = null!;
        available = null!;
    }

    private void Drop_Click(object sender, RoutedEventArgs e)
    {
        if (!HasValidSession() || student == null)
            return;

        int studentId = student.StudentID;
        Student? current = DataStore.LoadStudent(studentId);
        Subject[] allSubjects = DataStore.LoadSubjects();
        Score[] studentScores = DataStore.LoadScoresForStudent(studentId);

        if (current == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            allSubjects = null!;
            studentScores = null!;
            return;
        }

        List<int> offeredIds = current.OfferedSubjectIDs ?? new List<int>();
        if (offeredIds.Count == 0)
        {
            MessageBox.Show("This student isn't offering any subjects to drop.");
            current = null;
            allSubjects = null!;
            studentScores = null!;
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

            if (DataStore.UpdateStudentSubjects(studentId, offeredIds))
            {
                DataStore.SaveGrade(studentId, subjectId, null);
                MessageBox.Show("Subject dropped successfully!");
            }
        }

        window.SelectedSubject = null;
        window = null!;
        current = null;
        allSubjects = null!;
        studentScores = null!;
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