using System.Diagnostics;
using System.Text;
using System.IO; 

namespace StudentManagementApp;

public sealed class Score
{
    public int StudentID { get; set; }
    public int SubjectID { get; set; }
    public int? Grade { get; set; }

    public Score(int studentId, int subjectId, int? grade)
    {
        StudentID = studentId;
        SubjectID = subjectId;
        Grade = grade;
    }
}

public sealed class Student
{
    public int StudentID { get; set; }
    public string StudentPassword { get; set; }
    public string Name { get; set; }
    public List<int> OfferedSubjectIDs { get; set; }

    public Student(int id, string name, string password)
    {
        StudentID = id;
        Name = name;
        StudentPassword = password;
        OfferedSubjectIDs = new List<int>();
    }
}

public sealed class Subject
{
    public int SubjectID { get; set; }
    public string SubjectName { get; set; }

    public Subject(int id, string name)
    {
        SubjectID = id;
        SubjectName = name;
    }
}

/// <summary>
/// The only component that owns the persistent database mechanics.
/// It deliberately keeps no student, subject, score, or password collection in static memory.
/// Every public read creates operation-scoped objects and returns them to the caller.
/// Every public mutation reloads only the records it needs, writes atomically, and lets
/// its temporary objects fall out of scope before the method returns.
/// </summary>
public static class DataStore
{
    private const string StudentFile = "Student.txt";
    private const string SubjectFile = "Subject.txt";
    private const string ScoresFile = "Scores.txt";
    private const string PasswordFile = "AdminPassword.txt";
    private const string NextStudentIdFile = "NextStudentID.txt";
    private const string NextSubjectIdFile = "NextSubjectID.txt";

    // This lock protects operations within this process. The companion lock files below
    // also prevent a second application instance from entering the same transaction.
    private static readonly object DataLock = new();

    public static bool VerifyAdminPassword(string inputPassword)
    {
        // If the user didn't type anything, don't even open the file
        if (string.IsNullOrEmpty(inputPassword))
            return false;

        lock (DataLock)
        {
            // 1. New Strict Rule: If the file is missing, too bad! Access denied.
            if (!File.Exists(PasswordFile))
            {
                return false;
            }

            // 2. Acquire the safe file system lock
            using FileStream databaseLock = AcquireFileLock(PasswordFile);

            // 3. Open a transient reader stream safely since we know it exists
            using var stream = new FileStream(
                PasswordFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);

            string actualPassword = reader.ReadToEnd().Trim();

            // 4. If the file is completely blank, treat it as access denied
            if (string.IsNullOrEmpty(actualPassword))
            {
                return false;
            }

            // 5. Compare the input directly and return the boolean true/false
            return inputPassword == actualPassword;
        }
    }

    public static Student? LoadStudent(int studentId)
    {
        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(StudentFile);
            if (!File.Exists(StudentFile))
                return null;

            using var stream = new FileStream(
                StudentFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);

            while (reader.ReadLine() is { } line)
            {
                if (TryParseStudent(line, out Student? student))
                {
                    if (student.StudentID == studentId)
                        return student;
                }
            }

            return null;
        }
    }

    public static Student[] LoadStudents()
    {
        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(StudentFile);
            if (!File.Exists(StudentFile))
                return Array.Empty<Student>();

            var result = new List<Student>();
            using var stream = new FileStream(
                StudentFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);

            while (reader.ReadLine() is { } line)
            {
                if (TryParseStudent(line, out Student? student))
                    result.Add(student);
            }

            return result.ToArray();
        }
    }

    public static Subject[] LoadSubjects()
    {
        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(SubjectFile);
            if (!File.Exists(SubjectFile))
                return Array.Empty<Subject>();

            var result = new List<Subject>();
            using var stream = new FileStream(
                SubjectFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);

            while (reader.ReadLine() is { } line)
            {
                if (TryParseSubject(line, out Subject? subject))
                    result.Add(subject);
            }

            return result.ToArray();
        }
    }

    public static Score[] LoadScoresForStudent(int studentId)
    {
        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(ScoresFile);
            if (!File.Exists(ScoresFile))
                return Array.Empty<Score>();

            var result = new List<Score>();
            using var stream = new FileStream(
                ScoresFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);

            while (reader.ReadLine() is { } line)
            {
                if (TryParseScore(line, out Score? score) && score.StudentID == studentId)
                    result.Add(score);
            }

            return result
                .GroupBy(score => score.SubjectID)
                .Select(group => group.First())
                .ToArray();
        }
    }

    public static Score[] LoadAllScores()
    {
        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(ScoresFile);
            if (!File.Exists(ScoresFile))
                return Array.Empty<Score>();

            var result = new List<Score>();
            using var stream = new FileStream(
                ScoresFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.SequentialScan);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);

            while (reader.ReadLine() is { } line)
            {
                if (TryParseScore(line, out Score? score))
                    result.Add(score);
            }

            return result
                .GroupBy(score => new { score.StudentID, score.SubjectID })
                .Select(group => group.First())
                .ToArray();
        }
    }

    public static int ReserveNextStudentId()
    {
        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(StudentFile);
            int highestExistingId = GetHighestStudentIdWithoutNestedLock();
            return ReserveNextId(NextStudentIdFile, highestExistingId + 1);
        }
    }

    public static int ReserveNextSubjectId()
    {
        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(SubjectFile);
            int highestExistingId = GetHighestSubjectIdWithoutNestedLock();
            return ReserveNextId(NextSubjectIdFile, highestExistingId + 1);
        }
    }

    public static Student CreateStudent(string name, string password)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Student password cannot be empty.", nameof(password));

        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(StudentFile);
            int highestExistingId = GetHighestStudentIdWithoutNestedLock();
            int newId = ReserveNextId(NextStudentIdFile, highestExistingId + 1);

            var student = new Student(newId, name.Trim(), password);
            List<string> lines = ReadAllLinesPreservingMalformed(StudentFile);
            lines.Add(SerializeStudent(student));
            WriteAllLinesAtomically(StudentFile, lines);
            return student;
        }
    }

    public static Subject CreateSubject(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subject name cannot be empty.", nameof(name));

        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(SubjectFile);
            int highestExistingId = GetHighestSubjectIdWithoutNestedLock();
            int newId = ReserveNextId(NextSubjectIdFile, highestExistingId + 1);

            var subject = new Subject(newId, name.Trim());
            List<string> lines = ReadAllLinesPreservingMalformed(SubjectFile);
            lines.Add(SerializeSubject(subject));
            WriteAllLinesAtomically(SubjectFile, lines);
            return subject;
        }
    }

    public static bool UpdateStudentSubjects(int studentId, IEnumerable<int> subjectIds)
    {
        ArgumentNullException.ThrowIfNull(subjectIds);

        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(StudentFile);
            List<string> lines = ReadAllLinesPreservingMalformed(StudentFile);
            bool updated = false;
            var cleanedIds = subjectIds.Distinct().ToList();

            for (int index = 0; index < lines.Count; index++)
            {
                if (!TryParseStudent(lines[index], out Student? student) || student.StudentID != studentId)
                    continue;

                student.OfferedSubjectIDs = cleanedIds.ToList();
                lines[index] = SerializeStudent(student);
                updated = true;
                break;
            }

            if (updated)
                WriteAllLinesAtomically(StudentFile, lines);

            return updated;
        }
    }

    public static bool DeleteStudent(int studentId)
    {
        lock (DataLock)
        {
            using FileStream studentLock = AcquireFileLock(StudentFile);
            using FileStream scoreLock = AcquireFileLock(ScoresFile);

            List<string> studentLines = ReadAllLinesPreservingMalformed(StudentFile);
            bool removed = false;
            var remainingStudents = new List<string>(studentLines.Count);

            foreach (string line in studentLines)
            {
                if (TryParseStudent(line, out Student? student) && student.StudentID == studentId)
                {
                    removed = true;
                    continue;
                }
                remainingStudents.Add(line);
            }

            if (!removed)
                return false;

            List<string> scoreLines = ReadAllLinesPreservingMalformed(ScoresFile);
            var remainingScores = new List<string>(scoreLines.Count);
            foreach (string line in scoreLines)
            {
                if (TryParseScore(line, out Score? score) && score.StudentID == studentId)
                    continue;
                remainingScores.Add(line);
            }

            WriteAllLinesAtomically(StudentFile, remainingStudents);
            WriteAllLinesAtomically(ScoresFile, remainingScores);
            return true;
        }
    }

    public static bool SaveGrade(int studentId, int subjectId, int? grade)
    {
        if (grade.HasValue && (grade.Value < 0 || grade.Value > 100))
            throw new ArgumentOutOfRangeException(nameof(grade), "Grade must be between 0 and 100.");

        lock (DataLock)
        {
            using FileStream studentLock = AcquireFileLock(StudentFile);
            using FileStream subjectLock = AcquireFileLock(SubjectFile);
            using FileStream scoreLock = AcquireFileLock(ScoresFile);

            Student? student = FindStudentInLines(studentId, ReadAllLinesPreservingMalformed(StudentFile));
            if (student == null || !student.OfferedSubjectIDs.Contains(subjectId))
                return false;

            List<string> subjectLines = ReadAllLinesPreservingMalformed(SubjectFile);
            if (!subjectLines.Any(line => TryParseSubject(line, out Subject? subject) && subject.SubjectID == subjectId))
                return false;

            List<string> scoreLines = ReadAllLinesPreservingMalformed(ScoresFile);
            bool found = false;
            var rewritten = new List<string>(scoreLines.Count + 1);

            foreach (string line in scoreLines)
            {
                if (TryParseScore(line, out Score? score) &&
                    score.StudentID == studentId &&
                    score.SubjectID == subjectId)
                {
                    found = true;
                    if (grade.HasValue)
                        rewritten.Add($"{studentId},{subjectId},{grade.Value}");
                    continue;
                }

                rewritten.Add(line);
            }

            if (!found && grade.HasValue)
                rewritten.Add($"{studentId},{subjectId},{grade.Value}");

            WriteAllLinesAtomically(ScoresFile, rewritten);
            return true;
        }
    }

    public static bool ChangeStudentPassword(int studentId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(newPassword));

        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(StudentFile);
            List<string> lines = ReadAllLinesPreservingMalformed(StudentFile);

            for (int index = 0; index < lines.Count; index++)
            {
                if (!TryParseStudent(lines[index], out Student? student) || student.StudentID != studentId)
                    continue;

                student.StudentPassword = newPassword;
                lines[index] = SerializeStudent(student);
                WriteAllLinesAtomically(StudentFile, lines);
                return true;
            }

            return false;
        }
    }

    public static bool ChangeStudentName(int studentId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty.", nameof(newName));

        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(StudentFile);
            List<string> lines = ReadAllLinesPreservingMalformed(StudentFile);

            for (int index = 0; index < lines.Count; index++)
            {
                if (!TryParseStudent(lines[index], out Student? student) || student.StudentID != studentId)
                    continue;

                student.Name = newName.Trim();
                lines[index] = SerializeStudent(student);
                WriteAllLinesAtomically(StudentFile, lines);
                return true;
            }

            return false;
        }
    }

    public static bool ChangeAdminPassword(string newPassword, string currentPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(newPassword));

        lock (DataLock)
        {
            using FileStream databaseLock = AcquireFileLock(PasswordFile);
            string current = File.Exists(PasswordFile) ? ReadTextFile(PasswordFile).Trim() : "Messi";
            if (!string.Equals(current, currentPassword, StringComparison.Ordinal))
                return false;

            WriteAllTextAtomically(PasswordFile, newPassword);
            return true;
        }
    }

    private static Student? FindStudentInLines(int studentId, IEnumerable<string> lines)
    {
        foreach (string line in lines)
        {
            if (TryParseStudent(line, out Student? student) && student.StudentID == studentId)
                return student;
        }

        return null;
    }

    private static int GetHighestStudentIdWithoutNestedLock()
    {
        int highest = 0;
        if (!File.Exists(StudentFile))
            return highest;

        using var stream = new FileStream(StudentFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        while (reader.ReadLine() is { } line)
        {
            if (TryParseStudent(line, out Student? student))
                highest = Math.Max(highest, student.StudentID);
        }

        return highest;
    }

    private static int GetHighestSubjectIdWithoutNestedLock()
    {
        int highest = 0;
        if (!File.Exists(SubjectFile))
            return highest;

        using var stream = new FileStream(SubjectFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        while (reader.ReadLine() is { } line)
        {
            if (TryParseSubject(line, out Subject? subject))
                highest = Math.Max(highest, subject.SubjectID);
        }

        return highest;
    }

    private static int ReserveNextId(string counterFile, int minimumNext)
    {
        int storedNext = 1;
        if (File.Exists(counterFile) && int.TryParse(ReadTextFile(counterFile).Trim(), out int parsed))
            storedNext = Math.Max(1, parsed);

        int next = Math.Max(storedNext, minimumNext);
        WriteAllTextAtomically(counterFile, checked((next + 1).ToString()));
        return next;
    }

    private static bool TryParseStudent(string line, out Student? student)
    {
        student = null;
        string[] parts = line.Split(',');
        if (parts.Length < 3 || !int.TryParse(parts[0], out int id))
        {
            if (!string.IsNullOrWhiteSpace(line))
                LogMalformed("Student.txt", line);
            return false;
        }

        student = new Student(id, parts[1], parts[2]);

        if (parts.Length >= 4 && !int.TryParse(parts[3], out int declaredCount))
        {
            LogMalformed("Student.txt", line);
            return false;
        }

        if (parts.Length > 4)
        {
            for (int index = 4; index < parts.Length; index++)
            {
                if (int.TryParse(parts[index], out int subjectId))
                    student.OfferedSubjectIDs.Add(subjectId);
                else if (!string.IsNullOrWhiteSpace(parts[index]))
                    LogMalformed("Student.txt", line);
            }
        }

        return true;
    }

    private static bool TryParseSubject(string line, out Subject? subject)
    {
        subject = null;
        string[] parts = line.Split(',');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int id))
        {
            if (!string.IsNullOrWhiteSpace(line))
                LogMalformed("Subject.txt", line);
            return false;
        }

        subject = new Subject(id, parts[1]);
        return true;
    }

    private static bool TryParseScore(string line, out Score? score)
    {
        score = null;
        string[] parts = line.Split(',');
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out int studentId) ||
            !int.TryParse(parts[1], out int subjectId) ||
            !int.TryParse(parts[2], out int grade) ||
            grade < 0 || grade > 100)
        {
            if (!string.IsNullOrWhiteSpace(line))
                LogMalformed("Scores.txt", line);
            return false;
        }

        score = new Score(studentId, subjectId, grade);
        return true;
    }

    private static string SerializeStudent(Student student)
    {
        List<int> ids = student.OfferedSubjectIDs?.Distinct().ToList() ?? new List<int>();
        string subjectPart = ids.Count == 0 ? "" : "," + string.Join(",", ids);
        return $"{student.StudentID},{student.Name},{student.StudentPassword},{ids.Count}{subjectPart}";
    }

    private static string SerializeSubject(Subject subject) => $"{subject.SubjectID},{subject.SubjectName}";

    private static List<string> ReadAllLinesPreservingMalformed(string path)
    {
        if (!File.Exists(path))
            return new List<string>();

        var lines = new List<string>();
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        while (reader.ReadLine() is { } line)
            lines.Add(line);
        return lines;
    }

    private static string ReadTextFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        return reader.ReadToEnd();
    }

    private static void WriteAllLinesAtomically(string path, IEnumerable<string> lines)
    {
        WriteAllTextAtomically(path, string.Join(Environment.NewLine, lines));
    }

    private static void WriteAllTextAtomically(string path, string content)
    {
        string tempPath = path + ".tmp";
        try
        {
            using (var stream = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                4096,
                FileOptions.WriteThrough))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
                writer.Flush();
                stream.Flush(true);
            }

            if (File.Exists(path))
            {
                string backupPath = path + ".bak";
                File.Replace(tempPath, path, backupPath, true);
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }
            else
            {
                File.Move(tempPath, path);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { /* Preserve the original failure. */ }
            }
        }
    }

    private static FileStream AcquireFileLock(string dataFile)
    {
        string lockPath = dataFile + ".lock";
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(lockPath))!);

        for (int attempt = 0; attempt < 40; attempt++)
        {
            try
            {
                return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) when (attempt < 39)
            {
                Thread.Sleep(25);
            }
        }

        throw new IOException($"Unable to acquire the database lock for {dataFile}.");
    }

    private static void LogMalformed(string fileName, string line)
    {
        Debug.WriteLine($"[Records DB] Malformed line preserved in {fileName}: {line}");
    }
}
