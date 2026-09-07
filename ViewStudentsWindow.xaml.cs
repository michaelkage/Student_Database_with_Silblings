using System.Windows;

namespace StudentManagementApp;

public partial class ViewStudentsWindow : Window
{
    private Student[]? students;
    private Subject[]? subjects;
    private Score[]? scores;

    private sealed class ResultRow
    {
        public int StudentID { get; set; }
        public string Name { get; set; } = "";
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = "";
        public string Score { get; set; } = "—";
        public string LetterGrade { get; set; } = "—";
    }

    public ViewStudentsWindow()
    {
        InitializeComponent();

        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("All-student results are available to administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        LoadResults();
    }

    private void LoadResults()
    {
        students = DataStore.LoadStudents();
        subjects = DataStore.LoadSubjects();
        scores = DataStore.LoadAllScores();

        if (students.Length == 0)
        {
            MessageBox.Show("No students registered.");
            ResultsDataGrid.ItemsSource = new List<ResultRow>();
            return;
        }

        var rows = new List<ResultRow>();
        foreach (Student student in students)
        {
            List<int> offeredSubjectIds = student.OfferedSubjectIDs ?? new List<int>();
            foreach (Subject subject in subjects.Where(subject => offeredSubjectIds.Contains(subject.SubjectID)))
            {
                Score? match = scores.FirstOrDefault(score =>
                    score.StudentID == student.StudentID && score.SubjectID == subject.SubjectID);

                rows.Add(new ResultRow
                {
                    StudentID = student.StudentID,
                    Name = student.Name,
                    SubjectID = subject.SubjectID,
                    SubjectName = subject.SubjectName,
                    Score = match?.Grade?.ToString() ?? "—",
                    LetterGrade = match?.Grade.HasValue == true
                        ? MainWindow.GetLetterGrade(match.Grade.Value)
                        : "—"
                });
                match = null;
            }
        }

        if (rows.Count == 0)
            MessageBox.Show("No students are currently offering any subjects.");

        ResultsDataGrid.ItemsSource = rows;
        rows = null!;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        ResultsDataGrid.ItemsSource = null;
        students = null;
        subjects = null;
        scores = null;
        base.OnClosed(e);
    }
}