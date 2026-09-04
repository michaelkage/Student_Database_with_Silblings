namespace StudentManagementApp;

public class Student
{
    public int StudentID { get; set; }

    public string FullName => $"{FirstName} {Surname}";

    public string FirstName { get; set; }

    public string Surname { get; set; }

    public string Password { get; set; }

    public Student(
        int studentID,
        string firstName,
        string surname,
        string password)
    {
        StudentID = studentID;
        FirstName = firstName;
        Surname = surname;
        Password = password;
    }
}