using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace StudentManagementApp;

public partial class EditResultWindow : Window
{
    private static readonly string AppDataFolder =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "StudentManagementApp");

    private static readonly string StudentFile =
        Path.Combine(AppDataFolder, "students.json");

    private static readonly string SubjectFile =
        Path.Combine(AppDataFolder, "subjects.json");

    private static readonly string ResultFile =
        Path.Combine(AppDataFolder, "results.json");

    private List<Student> students = new();
    private List<Subject> subjects = new();
    private List<Result> results = new();

    public EditResultWindow()
    {
        InitializeComponent();

        Directory.CreateDirectory(AppDataFolder);

        LoadData();
        PopulateStudents();
    }

    private void LoadData()
    {
        if (File.Exists(StudentFile))
        {
            string json = File.ReadAllText(StudentFile);

            students =
                JsonSerializer.Deserialize<List<Student>>(json)
                ?? new List<Student>();
        }

        if (File.Exists(SubjectFile))
        {
            string json = File.ReadAllText(SubjectFile);

            subjects =
                JsonSerializer.Deserialize<List<Subject>>(json)
                ?? new List<Subject>();
        }

        if (File.Exists(ResultFile))
        {
            string json = File.ReadAllText(ResultFile);

            results =
                JsonSerializer.Deserialize<List<Result>>(json)
                ?? new List<Result>();
        }
    }

    private void PopulateStudents()
    {
        StudentComboBox.ItemsSource = students;

        StudentComboBox.SelectedIndex =
            students.Count > 0 ? 0 : -1;
    }

    private void StudentComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (StudentComboBox.SelectedItem is not Student selectedStudent)
        {
            SubjectComboBox.ItemsSource = null;
            ScoreTextBox.Text = "";
            return;
        }

        // Find subjects assigned to this student.
        List<int> assignedSubjectIDs = results
            .Where(r => r.StudentID == selectedStudent.StudentID)
            .Select(r => r.SubjectID)
            .ToList();

        List<Subject> assignedSubjects = subjects
            .Where(s => assignedSubjectIDs.Contains(s.SubjectID))
            .ToList();

        SubjectComboBox.ItemsSource = assignedSubjects;

        if (assignedSubjects.Count > 0)
        {
            SubjectComboBox.SelectedIndex = 0;
        }
        else
        {
            SubjectComboBox.SelectedIndex = -1;
        }
    }

    private void SubjectComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (StudentComboBox.SelectedItem is not Student selectedStudent)
        {
            return;
        }

        if (SubjectComboBox.SelectedItem is not Subject selectedSubject)
        {
            ScoreTextBox.Text = "";
            return;
        }

        Result? result = results.FirstOrDefault(r =>
            r.StudentID == selectedStudent.StudentID &&
            r.SubjectID == selectedSubject.SubjectID);

        if (result == null || result.Score == null)
        {
            ScoreTextBox.Text = "";
        }
        else
        {
            ScoreTextBox.Text = result.Score.Value.ToString();
        }
    }

    private void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (StudentComboBox.SelectedItem is not Student selectedStudent)
        {
            MessageBox.Show("Please select a student.");
            return;
        }

        if (SubjectComboBox.SelectedItem is not Subject selectedSubject)
        {
            MessageBox.Show("Please select a subject.");
            return;
        }

        if (!int.TryParse(ScoreTextBox.Text.Trim(), out int score))
        {
            MessageBox.Show("Please enter a valid numerical score.");
            return;
        }

        if (score < 0 || score > 100)
        {
            MessageBox.Show("Score must be between 0 and 100.");
            return;
        }

        Result? existingResult = results.FirstOrDefault(r =>
            r.StudentID == selectedStudent.StudentID &&
            r.SubjectID == selectedSubject.SubjectID);

        if (existingResult == null)
        {
            MessageBox.Show(
                "This subject has not been assigned to this student.");

            return;
        }

        existingResult.Score = score;

        SaveResults();

        MessageBox.Show(
            $"Result saved successfully!\n\n" +
            $"{selectedStudent.FirstName} {selectedStudent.Surname}\n" +
            $"{selectedSubject.SubjectName}: {score}");

        Close();
    }

    private void SaveResults()
    {
        string json = JsonSerializer.Serialize(
            results,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(ResultFile, json);
    }
}