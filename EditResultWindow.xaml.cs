using System.Windows;
using System.Windows.Controls;

namespace StudentManagementApp;

public partial class EditResultWindow : Window
{
    private Student? student;
    private Subject[]? offeredSubjects;

    public EditResultWindow(Student student)
    {
        InitializeComponent();

        if (!MainWindow.IsAdminSessionActive)
        {
            MessageBox.Show("Grade editing is available to administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            Close();
            return;
        }

        this.student = DataStore.LoadStudent(student.StudentID);
        Subject[] allSubjects = DataStore.LoadSubjects();

        if (this.student == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            allSubjects = null!;
            Close();
            return;
        }

        offeredSubjects = allSubjects
            .Where(subject => this.student.OfferedSubjectIDs.Contains(subject.SubjectID))
            .ToArray();

        StudentTextBlock.Text = $"{this.student.StudentID} - {this.student.Name}";
        SubjectComboBox.ItemsSource = offeredSubjects;

        if (SubjectComboBox.Items.Count > 0)
            SubjectComboBox.SelectedIndex = 0;

        allSubjects = null!;
    }

    private void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (student == null || SubjectComboBox.SelectedItem is not Subject selectedSubject)
            return;

        Score[] studentScores = DataStore.LoadScoresForStudent(student.StudentID);
        Score? existing = studentScores.FirstOrDefault(score => score.SubjectID == selectedSubject.SubjectID);
        ScoreTextBox.Text = existing?.Grade?.ToString() ?? "";
        existing = null;
        studentScores = null!;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!MainWindow.IsAdminSessionActive || student == null)
        {
            MessageBox.Show("Administrators only.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int studentId = student.StudentID;
        Student? currentStudent = DataStore.LoadStudent(studentId);
        if (currentStudent == null)
        {
            MessageBox.Show("Student account not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (SubjectComboBox.SelectedItem is not Subject selectedSubject)
        {
            MessageBox.Show("Please select a subject the student is offering.");
            currentStudent = null;
            return;
        }

        if (!currentStudent.OfferedSubjectIDs.Contains(selectedSubject.SubjectID))
        {
            MessageBox.Show("Cannot enter a score: this student has not offered this subject.", "Invalid Subject", MessageBoxButton.OK, MessageBoxImage.Warning);
            currentStudent = null;
            return;
        }

        string text = ScoreTextBox.Text.Trim();
        int subjectId = selectedSubject.SubjectID;

        if (string.IsNullOrWhiteSpace(text))
        {
            DataStore.SaveGrade(studentId, subjectId, null);
            MessageBox.Show("Score cleared. It will now display as —.");
            currentStudent = null;
            Close();
            return;
        }

        if (!int.TryParse(text, out int grade) || grade < 0 || grade > 100)
        {
            MessageBox.Show("Invalid grade scale. Enter a whole number from 0 to 100, or leave it blank.");
            currentStudent = null;
            return;
        }

        if (!DataStore.SaveGrade(studentId, subjectId, grade))
        {
            MessageBox.Show("The grade could not be saved because the student/subject relationship changed.", "Save Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            currentStudent = null;
            return;
        }

        MessageBox.Show("Grade updated successfully!");
        currentStudent = null;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        SubjectComboBox.ItemsSource = null;
        offeredSubjects = null;
        student = null;
        base.OnClosed(e);
    }
}