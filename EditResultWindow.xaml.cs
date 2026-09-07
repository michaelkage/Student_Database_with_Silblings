using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StudentManagementApp;

public partial class EditResultWindow : Window
{
    private Student? student;

    public EditResultWindow(Student student)
    {
        InitializeComponent();

        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Grade editing is available to administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        MainWindow.LoadStudents();
        MainWindow.LoadSubjects();
        MainWindow.LoadScores();
        this.student = MainWindow.students.FirstOrDefault(s => s.StudentID == student.StudentID);

        if (this.student == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
            return;
        }

        StudentTextBlock.Text = $"{this.student.StudentID} - {this.student.Name}";
        SubjectComboBox.ItemsSource = MainWindow.subjects
            .Where(subject => (this.student.OfferedSubjectIDs ?? new System.Collections.Generic.List<int>()).Contains(subject.SubjectID))
            .ToArray();

        if (SubjectComboBox.Items.Count > 0)
            SubjectComboBox.SelectedIndex = 0;
    }

    private void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (student == null || SubjectComboBox.SelectedItem is not Subject selectedSubject)
            return;

        MainWindow.LoadScores();
        var existing = MainWindow.scores.FirstOrDefault(score =>
            score.StudentID == student.StudentID && score.SubjectID == selectedSubject.SubjectID);

        ScoreTextBox.Text = existing?.Grade?.ToString() ?? "";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive || student == null)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Student? currentStudent = MainWindow.LoadStudent(student.StudentID);
        MainWindow.LoadSubjects();
        MainWindow.LoadScores();

        if (currentStudent == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (SubjectComboBox.SelectedItem is not Subject selectedSubject)
        {
            MessageBox.Show("Please select a subject the student is offering.");
            return;
        }

        if (!(currentStudent.OfferedSubjectIDs ?? new System.Collections.Generic.List<int>()).Contains(selectedSubject.SubjectID))
        {
            MessageBox.Show("Cannot enter a score: this student has not offered this subject.", "Invalid Subject", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string text = ScoreTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            MainWindow.scores = MainWindow.scores
                .Where(score => !(score.StudentID == currentStudent.StudentID && score.SubjectID == selectedSubject.SubjectID))
                .ToArray();
            MainWindow.SaveScores();
            MessageBox.Show("Score cleared. It will now display as —.");
            Close();
            return;
        }

        if (!int.TryParse(text, out int grade) || grade < 0 || grade > 100)
        {
            MessageBox.Show("Invalid grade scale. Enter a whole number from 0 to 100, or leave it blank.");
            return;
        }

        var scoreList = MainWindow.scores.ToList();
        var existing = scoreList.FirstOrDefault(score =>
            score.StudentID == currentStudent.StudentID && score.SubjectID == selectedSubject.SubjectID);

        if (existing != null)
            existing.Grade = grade;
        else
            scoreList.Add(new Score(currentStudent.StudentID, selectedSubject.SubjectID, grade));

        MainWindow.scores = scoreList.ToArray();
        MainWindow.SaveScores();
        MessageBox.Show("Grade updated successfully!");
        Close();
    }
}