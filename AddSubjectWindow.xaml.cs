using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace StudentManagementApp;

public partial class AddSubjectWindow : Window
{
    private static readonly string AppDataFolder =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "StudentManagementApp");

    private static readonly string SubjectFile =
        Path.Combine(AppDataFolder, "subjects.json");

    public AddSubjectWindow()
    {
        InitializeComponent();

        Directory.CreateDirectory(AppDataFolder);

        int nextID = GetNextSubjectID();

        SubjectIDTextBlock.Text = nextID.ToString();
    }

    private int GetNextSubjectID()
    {
        if (!File.Exists(SubjectFile))
        {
            return 1;
        }

        string json = File.ReadAllText(SubjectFile);

        List<Subject>? subjects =
            JsonSerializer.Deserialize<List<Subject>>(json);

        if (subjects == null || subjects.Count == 0)
        {
            return 1;
        }

        return subjects.Max(s => s.SubjectID) + 1;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string subjectName = SubjectNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(subjectName))
        {
            MessageBox.Show("Please enter a subject name.");
            return;
        }

        // Format the subject name.
        subjectName =
            System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(subjectName.ToLower());

        List<Subject> subjects = new();

        if (File.Exists(SubjectFile))
        {
            string json = File.ReadAllText(SubjectFile);

            subjects =
                JsonSerializer.Deserialize<List<Subject>>(json)
                ?? new List<Subject>();
        }

        // Make sure the subject name is unique.
        if (subjects.Any(s =>
            s.SubjectName.Equals(
                subjectName,
                StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("That subject already exists.");
            return;
        }

        int subjectID = int.Parse(SubjectIDTextBlock.Text);

        Subject newSubject =
            new Subject(
                subjectID,
                subjectName);

        subjects.Add(newSubject);

        string newJson = JsonSerializer.Serialize(
            subjects,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(SubjectFile, newJson);

        MessageBox.Show(
            $"Subject added successfully!\n\nSubject ID: {subjectID}");

        Close();
    }
}