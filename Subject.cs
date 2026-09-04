namespace StudentManagementApp;

public class Subject
{
    public int SubjectID { get; set; }

    public string SubjectName { get; set; }

    public Subject(
        int subjectID,
        string subjectName)
    {
        SubjectID = subjectID;
        SubjectName = subjectName;
    }

    public override string ToString()
    {
        return $"{SubjectID} - {SubjectName}";
    }
}