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
        public int Score { get; set; }
        public string LetterGrade { get; set; } = "";
    }

    public ViewStudentsWindow()
    {
        InitializeComponent();
        LoadResults();
    }

    private void LoadResults()
    {
        MainWindow.LoadMemory();

        if (MainWindow.students.Length == 0)
        {
            MessageBox.Show("No students registered.");
            ResultsDataGrid.ItemsSource = new List<ResultRow>();
            return;
        }

        var rows = new List<ResultRow>();
        foreach (var student in MainWindow.students)
        {
            var offeredSubjects = MainWindow.subjects
                .Where(sub => student.OfferedSubjectIDs.Contains(sub.SubjectID))
                .ToArray();

            foreach (var subject in offeredSubjects)
            {
                var match = MainWindow.scores.FirstOrDefault(s =>
                    s.StudentID == student.StudentID && s.SubjectID == subject.SubjectID);
                int grade = match != null ? match.Grade : 0;

                rows.Add(new ResultRow
                {
                    StudentID = student.StudentID,
                    Name = student.Name,
                    SubjectID = subject.SubjectID,
                    SubjectName = subject.SubjectName,
                    Score = grade,
                    LetterGrade = MainWindow.GetLetterGrade(grade)
                });
            }
        }

        ResultsDataGrid.ItemsSource = rows;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}