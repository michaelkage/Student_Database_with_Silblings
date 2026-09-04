namespace StudentManagementApp;

public class Result
{
    public int StudentID { get; set; }
    public int SubjectID { get; set; }
    public int? Score { get; set; }

    public Result(int studentID, int subjectID, int? score)
    {
        StudentID = studentID;
        SubjectID = subjectID;
        Score = score;
    }
}