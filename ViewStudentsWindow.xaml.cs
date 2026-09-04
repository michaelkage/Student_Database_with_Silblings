using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace StudentManagementApp;

public partial class ViewStudentsWindow : Window
{
    private static readonly string AppDataFolder =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "StudentManagementApp");

    private static readonly string StudentFile =
        Path.Combine(AppDataFolder, "students.json");

    public ViewStudentsWindow()
    {
        InitializeComponent();

        LoadStudents();
    }

    private void LoadStudents()
    {
        if (!File.Exists(StudentFile))
        {
            StudentsDataGrid.ItemsSource = new List<Student>();
            return;
        }

        string json = File.ReadAllText(StudentFile);

        List<Student> students =
            JsonSerializer.Deserialize<List<Student>>(json)
            ?? new List<Student>();

        StudentsDataGrid.ItemsSource = students;
    }
}