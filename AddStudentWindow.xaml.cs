using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace StudentManagementApp;

public partial class AddStudentWindow : Window
{
    private static readonly string AppDataFolder =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "StudentManagementApp");

    private static readonly string StudentFile =
        Path.Combine(AppDataFolder, "students.json");

    public AddStudentWindow()
    {
        InitializeComponent();

        Directory.CreateDirectory(AppDataFolder);

        int nextID = GetNextStudentID();

        StudentIDTextBlock.Text = nextID.ToString();
    }

    private int GetNextStudentID()
    {
        if (!File.Exists(StudentFile))
        {
            return 1;
        }

        string json = File.ReadAllText(StudentFile);

        List<Student>? students =
            JsonSerializer.Deserialize<List<Student>>(json);

        if (students == null || students.Count == 0)
        {
            return 1;
        }

        return students.Max(s => s.StudentID) + 1;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string firstName = FirstNameTextBox.Text.Trim();
        string surname = SurnameTextBox.Text.Trim();
        string password = PasswordBox.Password.Trim();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            MessageBox.Show("Please enter the student's first name.");
            return;
        }

        if (string.IsNullOrWhiteSpace(surname))
        {
            MessageBox.Show("Please enter the student's surname.");
            return;
        }

        // If no password is entered, use the surname.
        if (string.IsNullOrWhiteSpace(password))
        {
            password = surname;
        }

        firstName =
            System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(firstName.ToLower());

        surname =
            System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(surname.ToLower());

        List<Student> students = new();

        if (File.Exists(StudentFile))
        {
            string json = File.ReadAllText(StudentFile);

            students =
                JsonSerializer.Deserialize<List<Student>>(json)
                ?? new List<Student>();
        }

        int studentID = int.Parse(StudentIDTextBlock.Text);

        Student newStudent =
            new Student(
                studentID,
                firstName,
                surname,
                password);

        students.Add(newStudent);

        string newJson = JsonSerializer.Serialize(
            students,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(StudentFile, newJson);

        MessageBox.Show(
            $"Student added successfully!\n\nStudent ID: {studentID}");

        Close();
    }
}