using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class ViewStudentsWindow : Window
{
    private class ResultRow
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
        MainWindow.LoadStudents();
        MainWindow.LoadSubjects();
        MainWindow.LoadScores();

        if (MainWindow.students.Length == 0)
        {
            MessageBox.Show("No students registered.");
            ResultsDataGrid.ItemsSource = new List<ResultRow>();
            return;
        }

        var rows = new List<ResultRow>();
        foreach (var student in MainWindow.students)
        {
            var offeredSubjectIds = student.OfferedSubjectIDs ?? new List<int>();
            var offeredSubjects = MainWindow.subjects
                .Where(subject => offeredSubjectIds.Contains(subject.SubjectID))
                .ToArray();

            foreach (var subject in offeredSubjects)
            {
                var match = MainWindow.scores.FirstOrDefault(score =>
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
            }
        }

        if (rows.Count == 0)
            MessageBox.Show("No students are currently offering any subjects.");

        ResultsDataGrid.ItemsSource = rows;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}