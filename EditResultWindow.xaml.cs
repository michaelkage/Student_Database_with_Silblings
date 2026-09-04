using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StudentManagementApp;

public partial class EditResultWindow : Window
{
    private readonly Student student;

    public EditResultWindow(Student student)
    {
        InitializeComponent();
        this.student = student;
        StudentTextBlock.Text = $"{student.StudentID} - {student.Name}";

        // Only subjects actually offered by this student can receive a score.
        SubjectComboBox.ItemsSource = MainWindow.subjects
            .Where(s => student.OfferedSubjectIDs.Contains(s.SubjectID))
            .ToArray();

        if (SubjectComboBox.Items.Count > 0)
            SubjectComboBox.SelectedIndex = 0;
    }

    private void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SubjectComboBox.SelectedItem is not Subject selectedSubject)
            return;

        var existing = MainWindow.scores.FirstOrDefault(s =>
            s.StudentID == student.StudentID && s.SubjectID == selectedSubject.SubjectID);

        ScoreTextBox.Text = existing?.Grade?.ToString() ?? "";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (SubjectComboBox.SelectedItem is not Subject selectedSubject)
        {
            MessageBox.Show("Please select a subject the student is offering.");
            return;
        }

        // This is enforced here as well as by the UI so an invalid Score can never be created.
        if (!student.OfferedSubjectIDs.Contains(selectedSubject.SubjectID))
        {
            MessageBox.Show("Cannot enter a score: this student has not offered this subject.", "Invalid Subject", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string text = ScoreTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            // Blank means no score has been entered. Remove any existing score record.
            MainWindow.scores = MainWindow.scores
                .Where(s => !(s.StudentID == student.StudentID && s.SubjectID == selectedSubject.SubjectID))
                .ToArray();
            MainWindow.SaveMemory();
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
        var existing = scoreList.FirstOrDefault(s =>
            s.StudentID == student.StudentID && s.SubjectID == selectedSubject.SubjectID);

        if (existing != null)
            existing.Grade = grade;
        else
            scoreList.Add(new Score(student.StudentID, selectedSubject.SubjectID, grade));

        MainWindow.scores = scoreList.ToArray();
        MainWindow.SaveMemory();
        MessageBox.Show("Grade updated successfully!");
        Close();
    }
}