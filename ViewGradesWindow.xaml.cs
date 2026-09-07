using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StudentManagementApp;

public partial class ViewGradesWindow : Window
{
    private Student? student;

    private class GradeRow
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
            this.student = student;
        }
        else
        {
            MessageBox.Show("Students may only view their own grades.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        LoadGrades();
    }

    private void LoadGrades()
    {
        if (student == null) return;

        MainWindow.LoadStudents();
        MainWindow.LoadSubjects();
        MainWindow.LoadScores();
        student = MainWindow.students.FirstOrDefault(s => s.StudentID == student.StudentID);

        if (student == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        NameTextBlock.Text = $"Name: {student.Name}";

        var offeredSubjects = MainWindow.subjects
            .Where(subject => (student.OfferedSubjectIDs ?? new List<int>()).Contains(subject.SubjectID))
            .ToArray();

        if (offeredSubjects.Length == 0)
        {
            MessageBox.Show("You are not offering any subjects currently.");
            GradesGrid.ItemsSource = new List<GradeRow>();
            return;
        }

        var studentScores = MainWindow.scores
            .Where(score => score.StudentID == student.StudentID)
            .ToArray();

        var rows = offeredSubjects.Select(subject =>
        {
            var match = studentScores.FirstOrDefault(score => score.SubjectID == subject.SubjectID);
            return new GradeRow
            {
                SubjectName = subject.SubjectName,
                Score = match?.Grade?.ToString() ?? "—",
                LetterGrade = match?.Grade.HasValue == true
                    ? MainWindow.GetLetterGrade(match.Grade.Value)
                    : "—"
            };
        }).ToList();

        GradesGrid.ItemsSource = rows;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}