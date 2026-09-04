# Records DB — Student Management App

A Windows desktop student records management application built as a direct WPF/XAML port of the original Records DB console application.

The project is intentionally simple: the original C# data structures and core logic are preserved, while the console interface has been replaced with a Windows graphical interface.

## Overview

Records DB manages:

- Students
- Subjects
- Student-subject offerings
- Student scores and letter grades
- Admin authentication
- Student authentication
- Password changes
- Student details

Data is persisted locally using plain text files rather than a database server or SQLite.

## Technology

- **C#**
- **WPF / XAML**
- **.NET 11 for Windows**
- Plain-text `.txt` files for persistence

The project file targets `net11.0-windows` and enables WPF.

## How It Works

The application follows the same basic flow as the original console version:

```text
TXT files
   ↓
LoadMemory()
   ↓
C# Student / Subject / Score objects
   ↓
Application logic
   ↓
WPF/XAML interface
   ↓
SaveMemory()
   ↓
TXT files
```

The application is therefore an in-memory records system with file-based persistence, not a traditional relational database engine.

## Data Files

The application uses these files:

| File | Purpose |
|---|---|
| `Student.txt` | Student IDs, names, passwords and offered subject IDs |
| `Subject.txt` | Subject IDs and subject names |
| `Scores.txt` | Student/subject scores |
| `AdminPassword.txt` | Admin password |
| `NextStudentID.txt` | Persistent next student ID counter |
| `NextSubjectID.txt` | Persistent next subject ID counter |

These files are part of the application's data and should be kept if the records need to persist between runs.

## IDs

Student and subject IDs are automatically assigned.

IDs are permanent unique identifiers. If an item is deleted, its ID is not reused.

For example:

```text
Students:
1
2
3
4

Delete student 3

1
2
4

Add a new student → 5
```

The same rule applies to subjects.

## Scores

A score has three possible states:

| Stored state | Meaning | Display |
|---|---|---|
| No score record | Subject is offered but no score has been entered | `—` |
| `0` | A real score of zero was entered | `0` |
| `1–100` | A real score was entered | The score |

This means an ungraded subject is not incorrectly treated as a score of zero.

Letter grades follow the original grading logic:

| Score | Grade |
|---:|:---|
| 80–100 | A |
| 70–79 | B |
| 60–69 | C |
| 50–59 | P |
| Below 50 | F |

## Subject Rules

A student's offered subjects and their scores are separate concepts.

For example:

```text
Student offers Mathematics
        ↓
Mathematics → —
        ↓
Admin enters 85
        ↓
Mathematics → 85
```

A score cannot exist for a subject that the student has not offered.

When a subject is dropped:

1. The subject is removed from the student's offered subjects.
2. Any score belonging to that student/subject is deleted.
3. If the subject is offered again later, it starts with `—`.

## User Roles

### Admin

The admin can:

- View all student results
- Add students
- Remove students
- Manage student subjects
- Add subjects to the master subject list
- Enter/change student scores
- Change the admin password

### Student

A student can only manage their own account:

- View their grades
- Offer subjects
- Drop subjects
- Change their password
- Edit their details

Students cannot view or modify another student's records and cannot access admin functions.

## Project Structure

The main WPF windows are divided by responsibility:

- `LoginWindow` — authentication
- `MainWindow` — main dashboard
- `StudentManagementWindow` — admin student management
- `AddStudentWindow` — adding students
- `AddSubjectWindow` — adding subjects
- `AssignSubjectsWindow` — offering/dropping subjects
- `EditResultWindow` — admin score management
- `ViewStudentsWindow` — admin all-results view
- `ViewGradesWindow` — student's own grades
- `StudentPasswordWindow` — student password changes
- `EditStudentDetailsWindow` — student detail editing
- `ChangeAdminPasswordWindow` — admin password changes

## Running the Project

### Requirements

You need a Windows development environment with the appropriate .NET SDK installed.

Check the installed SDK with:

```powershell
dotnet --version
```

### Build

From the project directory:

```powershell
dotnet build
```

### Run

```powershell
dotnet run
```

The application starts at the login window.

## Development Notes

This project intentionally avoids unnecessary architectural changes. It is a GUI port of the original console application rather than a complete rewrite.

The following are intentionally **not** used as the primary storage architecture:

- Entity Framework Core
- SQLite
- SQL Server
- JSON-based records storage
- Repository/service-layer architecture
- MVVM refactoring

The `.txt` files and the original in-memory C# structures remain the foundation of the application.

## Important

The `.gitignore` excludes generated build output such as `bin/` and `obj/`. Source code, XAML, project configuration and application data files remain trackable.

For a real production school records system, authentication and storage would need stronger security and a proper database. This project currently preserves the simple file-based design of the original Records DB application.

## Project Status

**Current status:** Active development / Windows WPF port.

The focus is maintaining the original Records DB behavior while making the application usable through a native Windows graphical interface.
