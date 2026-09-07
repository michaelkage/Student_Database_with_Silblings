using System.Windows;

namespace StudentManagementApp;

public partial class ViewGradesWindow : Window
{
    private Student? student;
    private Subject[]? subjects;
    private Score[]? scores;

    private sealed class GradeRow
    {
        public string SubjectName { get; set; } = "";
        public string Score { get; set; } = "—";
        public string LetterGrade { get; set; } = "—";
    }

    public ViewGradesWindow(Student student)
    {
        InitializeComponent();

        if (MainWindow.IsAdminSessionActive ||
            MainWindow.CurrentLoggedInStudent?.StudentID == student.StudentID)
        {
            this.student = DataStore.LoadStudent(student.StudentID);
        }
        else
        {
            MessageBox.Show("Students may only view their own grades.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        if (this.student == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        LoadGrades();
    }

    private void LoadGrades()
    {
        if (student == null) return;

        int studentId = student.StudentID;
        subjects = DataStore.LoadSubjects();
        scores = DataStore.LoadScoresForStudent(studentId);

        NameTextBlock.Text = $"Name: {student.Name}";

        List<int> offeredIds = student.OfferedSubjectIDs ?? new List<int>();
        Subject[] offeredSubjects = subjects
            .Where(subject => offeredIds.Contains(subject.SubjectID))
            .ToArray();

        if (offeredSubjects.Length == 0)
        {
            MessageBox.Show("You are not offering any subjects currently.");
            GradesGrid.ItemsSource = new List<GradeRow>();
            offeredSubjects = null!;
            return;
        }

        var rows = new List<GradeRow>(offeredSubjects.Length);
        foreach (Subject subject in offeredSubjects)
        {
            Score? match = scores.FirstOrDefault(score => score.SubjectID == subject.SubjectID);
            rows.Add(new GradeRow
            {
                SubjectName = subject.SubjectName,
                Score = match?.Grade?.ToString() ?? "—",
                LetterGrade = match?.Grade.HasValue == true
                    ? MainWindow.GetLetterGrade(match.Grade.Value)
                    : "—"
            });
            match = null;
        }

        GradesGrid.ItemsSource = rows;
        offeredSubjects = null!;
        rows = null!;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        GradesGrid.ItemsSource = null;
        scores = null;
        subjects = null;
        student = null;
        base.OnClosed(e);
    }
}