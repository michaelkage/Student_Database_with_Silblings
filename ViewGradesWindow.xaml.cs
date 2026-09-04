using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class ViewGradesWindow : Window
{
    private readonly Student student;

    private class GradeRow
    {
        public string SubjectName { get; set; } = "";
        public int Score { get; set; }
        public string LetterGrade { get; set; } = "";
    }

    public ViewGradesWindow(Student student)
    {
        InitializeComponent();
        this.student = student;
        LoadGrades();
    }

    private void LoadGrades()
    {
        NameTextBlock.Text = $"Name: {student.Name}";

        var offeredSubjects = MainWindow.subjects
            .Where(sub => student.OfferedSubjectIDs.Contains(sub.SubjectID))
            .ToArray();

        if (offeredSubjects.Length == 0)
        {
            MessageBox.Show("You are not offering any subjects currently.");
            GradesGrid.ItemsSource = new List<GradeRow>();
            return;
        }

        var studentScores = MainWindow.scores
            .Where(s => s.StudentID == student.StudentID)
            .ToArray();

        var rows = offeredSubjects.Select(subject =>
        {
            var match = studentScores.FirstOrDefault(s => s.SubjectID == subject.SubjectID);
            int currentGrade = match != null ? match.Grade : 0;
            return new GradeRow
            {
                SubjectName = subject.SubjectName,
                Score = currentGrade,
                LetterGrade = MainWindow.GetLetterGrade(currentGrade)
            };
        }).ToList();

        GradesGrid.ItemsSource = rows;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}