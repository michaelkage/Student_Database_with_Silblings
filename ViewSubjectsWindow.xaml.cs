using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace StudentManagementApp;

public partial class ViewSubjectsWindow : Window
{
    private static readonly string AppDataFolder =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "StudentManagementApp");

    private static readonly string SubjectFile =
        Path.Combine(AppDataFolder, "subjects.json");

    public ViewSubjectsWindow()
    {
        InitializeComponent();

        LoadSubjects();
    }

    private void LoadSubjects()
    {
        if (!File.Exists(SubjectFile))
        {
            SubjectsDataGrid.ItemsSource = new List<Subject>();
            return;
        }

        string json = File.ReadAllText(SubjectFile);

        List<Subject> subjects =
            JsonSerializer.Deserialize<List<Subject>>(json)
            ?? new List<Subject>();

        SubjectsDataGrid.ItemsSource = subjects;
    }
}