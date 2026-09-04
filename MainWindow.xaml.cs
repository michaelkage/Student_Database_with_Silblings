using System.Windows;

namespace StudentManagementApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void AddNewStudent_Click(object sender, RoutedEventArgs e)
    {
        AddStudentWindow window = new AddStudentWindow();

        window.Owner = this;

        window.ShowDialog();
    }

    private void ViewStudents_Click(object sender, RoutedEventArgs e)
    {
        ViewStudentsWindow window = new ViewStudentsWindow();

        window.Owner = this;

        window.ShowDialog();
    }

    private void AddNewSubject_Click(object sender, RoutedEventArgs e)
    {
        AddSubjectWindow window = new AddSubjectWindow();

        window.Owner = this;

        window.ShowDialog();
    }

    private void ViewSubjects_Click(object sender, RoutedEventArgs e)
    {
        ViewSubjectsWindow window = new ViewSubjectsWindow();

        window.Owner = this;

        window.ShowDialog();
    }

    private void EnterEditResult_Click(object sender, RoutedEventArgs e)
    {
        EditResultWindow window = new EditResultWindow();

        window.Owner = this;

        window.ShowDialog();
    }
}