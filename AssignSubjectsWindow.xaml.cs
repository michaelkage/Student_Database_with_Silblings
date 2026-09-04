using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace StudentManagementApp;

public partial class AssignSubjectsWindow : Window
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

    public AssignSubjectsWindow()
    {
        InitializeComponent();

        Directory.CreateDirectory(AppDataFolder);

        LoadData();
        PopulateStudents();
        PopulateSubjects();
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

        StudentComboBox.DisplayMemberPath = "FullName";

        if (students.Count > 0)
        {
            StudentComboBox.SelectedIndex = 0;
        }
    }

    private void PopulateSubjects()
    {
        SubjectsPanel.Children.Clear();

        foreach (Subject subject in subjects)
        {
            CheckBox checkBox = new CheckBox
            {
                Content = $"{subject.SubjectID} - {subject.SubjectName}",
                Tag = subject.SubjectID,
                Margin = new Thickness(5)
            };

            SubjectsPanel.Children.Add(checkBox);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (StudentComboBox.SelectedItem is not Student selectedStudent)
        {
            MessageBox.Show("Please select a student.");
            return;
        }

        int addedCount = 0;

        foreach (CheckBox checkBox in
                 SubjectsPanel.Children.OfType<CheckBox>())
        {
            if (checkBox.IsChecked != true)
            {
                continue;
            }

            int subjectID = (int)checkBox.Tag;

            bool alreadyAssigned = results.Any(r =>
                r.StudentID == selectedStudent.StudentID &&
                r.SubjectID == subjectID);

            if (alreadyAssigned)
            {
                continue;
            }

            Result newResult = new Result(
                selectedStudent.StudentID,
                subjectID,
                null);

            results.Add(newResult);

            addedCount++;
        }

        SaveResults();

        MessageBox.Show(
            $"{addedCount} subject(s) assigned to " +
            $"{selectedStudent.FirstName} {selectedStudent.Surname}.");

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