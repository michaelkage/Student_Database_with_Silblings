using System;
using System.Windows;
using System.Linq;

namespace StudentManagementApp
{
    public partial class ViewStudentsWindow : Window
    {
        public ViewStudentsWindow()
        {
            InitializeComponent();
            LoadGridData();
        }

        private void LoadGridData()
        {
            if (MainWindow.students != null && MainWindow.students.Length > 0)
            {
                dgStudents.ItemsSource = MainWindow.students.ToList();
            }
            else
            {
                System.Windows.MessageBox.Show("No active student records discovered in local database arrays.", "Database Registry Status");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}