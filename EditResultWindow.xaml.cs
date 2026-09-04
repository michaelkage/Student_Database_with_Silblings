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

        MainWindow.LoadMemory();
        SubjectComboBox.ItemsSource = MainWindow.subjects;
        if (MainWindow.subjects.Length > 0)
            SubjectComboBox.SelectedIndex = 0;
    }

    private void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SubjectComboBox.SelectedItem is not Subject selectedSubject)
            return;

        var existing = MainWindow.scores.FirstOrDefault(s =>
            s.StudentID == student.StudentID && s.SubjectID == selectedSubject.SubjectID);

        ScoreTextBox.Text = existing != null ? existing.Grade.ToString() : "";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (SubjectComboBox.SelectedItem is not Subject selectedSubject)
        {
            MessageBox.Show("Invalid or non-existent Subject ID.");
            return;
        }

        if (!student.OfferedSubjectIDs.Contains(selectedSubject.SubjectID))
        {
            MessageBox.Show("Warning: This student hasn't offered this subject yet.");
        }

        if (int.TryParse(ScoreTextBox.Text.Trim(), out int grade) && grade >= 0 && grade <= 100)
        {
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
        else
        {
            MessageBox.Show("Invalid grade scale.");
        }
    }
}