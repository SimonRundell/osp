# OSP Hours Tracker

**Version 0.0.0.3**

A Windows desktop application for recording and reporting student time spent on Open-Supervised Projects (OSPs) — timed examination tasks typically 30 hours in total, taken under supervised conditions with individual access arrangements (extra time, rest breaks).

It replaces a manual spreadsheet previously used by teaching staff to track exam-board time allowances across multiple supervised sessions per student, per project.

Built by Simon Rundell, CodeMonkey Design Ltd. for Exeter College.

---

## Table of Contents

1. [Overview](#overview)
2. [Technology Stack](#technology-stack)
3. [Directory Structure](#directory-structure)
4. [Database](#database)
5. [Configuration](#configuration)
6. [Installation](#installation)
7. [First Login](#first-login)
8. [User Roles](#user-roles)
9. [Key Workflows](#key-workflows)
10. [Authentication](#authentication)
11. [API Reference](#api-reference)
12. [Application Screens](#application-screens)
13. [Reporting & Export](#reporting--export)
14. [Security Notes](#security-notes)
15. [Deployment](#deployment)
16. [Changelog](#changelog)
17. [License](#license)

---

## Overview

The OSP Hours Tracker lets teaching and exam staff:

- Create and manage OSP projects, each representing a distinct exam task with a standard base-hours allowance (e.g. 30 hours).
- Enrol students onto projects with individual access arrangements — extra time (+10%, +20%, +25%) and/or a rest-breaks flag.
- Record supervised sessions, either whole-class or one-to-one catch-up sessions.
- Enter per-student attendance in minutes for each session, with live warnings if an entry would exceed the session length or the student's total allowance.
- See running totals at a glance — minutes used, minutes remaining, percentage consumed via a colour-coded progress bar (green under 80%, amber 80-99%, red 100%+) — consistently on the Project Detail screen, the Attendance Entry screen (updating live as minutes are typed), the Session List screen, and the printed/exported report.
- Generate a professional per-project report: print to PDF, or export to CSV or Excel.
- Bulk-import students from a CSV spreadsheet.
- Manage staff accounts (admin) with temporary-password creation and reset, automatically copied to the clipboard ready to paste into an email.

Access is login-only — there is no public registration or self-service student access. An admin account manages system configuration (staff, students, projects); ordinary staff accounts are restricted to session and attendance entry, with read-only access to students and projects.

---

## Technology Stack

| Layer          | Technology                                    |
|----------------|------------------------------------------------|
| Desktop client | C# / .NET 8 / Windows Forms                    |
| HTTP / JSON    | `System.Net.Http.HttpClient`, Newtonsoft.Json  |
| Excel export   | ClosedXML                                      |
| Reporting      | Built-in WinForms `WebBrowser` control (HTML → print/PDF) |
| Backend        | PHP 8.x                                        |
| Auth           | JWT (HS256) via `firebase/php-jwt`             |
| Database       | MySQL / MariaDB                                |
| Web server     | Apache (mod_rewrite for the `Authorization` header) |

---

## Directory Structure

```
OSP/
├── OSPTracker.csproj          WinForms project file
├── OSPTracker.slnx            Visual Studio solution
├── Program.cs                 Entry point
├── config.json                Client config (API base URL)
├── osptracker.ico              Application icon
│
├── Forms/
│   ├── LoginForm.cs            Username/password login
│   ├── MainForm.cs             Application shell — Dashboard / Projects / Admin tabs
│   ├── ChangePasswordForm.cs   Voluntary and forced password change
│   ├── ProjectsPanel.cs        Read-only browse list of all projects
│   ├── ProjectDetailForm.cs    Project info, enrolled students, enrolment management
│   ├── SessionListForm.cs      Sessions for a project; scheduled/remaining time footer
│   ├── AttendanceEntryForm.cs  Per-student minutes entry with live over-time warnings
│   └── Dashboard/
│       └── DashboardPanel.cs   Project overview cards
│
├── Dialogs/
│   ├── SessionFormDialog.cs    Create/edit a session
│   └── EnrolStudentDialog.cs   Enrol a student / edit access arrangements
│
├── Admin/
│   ├── AdminPanelBase.cs       Shared CRUD grid + toolbar boilerplate
│   ├── AdminTabPanel.cs        Staff / Students / Projects sub-tabs
│   ├── StaffPanel.cs, StaffEditDialog.cs
│   ├── StudentsPanel.cs, StudentEditDialog.cs, StudentImportDialog.cs
│   └── ProjectAdminPanel.cs, ProjectEditDialog.cs
│
├── Reports/
│   ├── ReportForm.cs           Print/PDF report window
│   ├── ReportBuilder.cs        Generates the report's HTML
│   └── ExportHelper.cs         CSV and Excel (ClosedXML) export
│
├── Models/                     DTOs: AuthModels, AdminModels, ProjectModels,
│                                SessionModels, AttendanceModels, ReportModels
├── Services/
│   ├── ApiService.cs           HTTP client wrapper, Bearer-token auth
│   └── AppConfig.cs            Loads config.json
├── Utils/
│   ├── Theme.cs                   Shared colour palette / fonts
│   ├── CsvParser.cs               RFC 4180-ish CSV tokenizer for student import
│   └── ProgressCellRenderer.cs    Shared % Used progress-bar painter for DataGridView cells
│
├── data/
│   └── schema.sql              Full MySQL schema, views
│
└── osp/                         PHP REST API (see below)
```

---

## Database

A single MySQL/MariaDB database, `osp_tracker`, `utf8mb4`. Full schema — including the two reporting views — lives in [`data/schema.sql`](data/schema.sql).

### Tables

| Table                | Purpose                                              |
|----------------------|-------------------------------------------------------|
| `staff`              | Login accounts (`admin` / `staff` roles), bcrypt passwords |
| `projects`           | OSP tasks — base hours, centre number, academic year  |
| `students`           | Student master records, keyed by exam-board candidate number |
| `project_students`   | Enrolment junction; stores per-student access arrangements |
| `sessions`           | Supervised working periods (class or individual)      |
| `session_attendance` | Minutes present per student per session                |

### Views

| View                       | Purpose                                                          |
|----------------------------|-------------------------------------------------------------------|
| `session_summary`          | Sessions joined with calculated `available_minutes` and supervisor name |
| `student_project_summary`  | Running totals (allowed / used / remaining minutes) per student per project |

### Key derived values

These are **never stored** — always calculated at query time by the views above:

```
total_minutes_allowed = base_hours × 60 × (1 + time_extension_percent / 100)
total_minutes_used    = SUM(session_attendance.minutes_present)
minutes_remaining     = total_minutes_allowed − total_minutes_used
available_minutes     = TIME_TO_SEC(TIMEDIFF(end_time, start_time)) / 60
```

---

## Configuration

### Desktop client — `config.json`

Sits next to the executable:

```json
{
  "apiBaseUrl": "http://localhost/osp"
}
```

Point `apiBaseUrl` at wherever the PHP backend is served — it should resolve directly to the folder containing `auth/login.php`, `students/index.php`, and so on.

### PHP backend — `osp/.config.json`

```json
{
  "db":  { "host": "127.0.0.1", "name": "osp_tracker", "user": "...", "pass": "..." },
  "jwt": { "secret": "REPLACE_WITH_STRONG_RANDOM_SECRET_MIN_32_CHARS", "accessExpiry": 28800 },
  "centre_number": "54221"
}
```

- `jwt.accessExpiry` is in seconds (28800 = 8 hours).
- `centre_number` is the default awarding-body centre number pre-filled when an admin creates a new project.
- **Never commit this file.** It is listed in `.gitignore`. Only `.backend.example.config.json` (placeholder values) belongs in source control.

---

## Installation

### Prerequisites

- Windows with .NET 8 Desktop Runtime (or the SDK, for building from source)
- PHP 8.1+ with the `pdo_mysql` extension
- MySQL 8.x or MariaDB
- Apache with `mod_rewrite` enabled
- Composer (to install the JWT library)

### Steps

1. **Create the database:**
   ```bash
   mysql -u root -p < data/schema.sql
   ```
   This creates the `osp_tracker` database, all tables and the two reporting views. You will need to insert at least one active `admin` staff row manually (bcrypt-hashed password) before you can log in — see [First Login](#first-login).

2. **Install the PHP backend's dependency:**
   ```bash
   cd osp
   composer install
   cp .backend.example.config.json .config.json
   ```
   Edit `.config.json` with your real database credentials and a strong, random JWT secret (`openssl rand -base64 48` is a good way to generate one).

3. **Serve the `osp/` folder from Apache** so that it's reachable at the URL you'll put in the desktop client's `config.json`.

4. **Build and run the desktop client:**
   ```bash
   dotnet build OSPTracker.csproj
   dotnet run --project OSPTracker.csproj
   ```
   Or open `OSPTracker.slnx` in Visual Studio 2022 and press F5. Make sure `config.json` points `apiBaseUrl` at your running API first.

---

## First Login

There is no self-service account creation. The very first admin account must be inserted directly into the `staff` table:

```sql
INSERT INTO staff (username, email, password_hash, first_name, last_name, role, must_change_password)
VALUES ('admin', 'admin@example.com', '<bcrypt hash>', 'System', 'Admin', 'admin', 1);
```

Generate the bcrypt hash with PHP: `php -r "echo password_hash('YourTempPassword', PASSWORD_BCRYPT, ['cost'=>12]);"`

Leaving `must_change_password = 1` forces a password change immediately after the first login, before anything else in the app is accessible.

**Password policy** (enforced server-side, independent of the client's own validation):
- Minimum 8 characters
- At least one uppercase letter [A-Z]
- At least one digit [0-9]

Once the first admin is in, all further staff accounts (admin or staff) are created from **Admin → Staff → + Add Staff** inside the app itself — each gets a random 12-character temporary password, shown once and copied to the clipboard, ready to paste into an email.

---

## User Roles

| Role    | Capabilities                                                                 |
|---------|-------------------------------------------------------------------------------|
| `admin` | Full CRUD on staff, students and projects. Enrol/unenrol students, edit access arrangements, edit/delete sessions, view all reports. |
| `staff` | Create sessions, enter attendance, view reports. Can create and edit student records (not admin-gated — matches the exam-office workflow where any invigilator may register a new candidate). Cannot manage staff accounts, create/edit projects, enrol students, or edit/delete sessions. |

Role is embedded in the signed JWT and re-checked on every write request server-side — the desktop client's tab/button visibility is a convenience, not the security boundary.

---

## Key Workflows

### Set up a new project

1. **Admin → Projects → + New Project** — name, academic year, centre number, base hours, optional start/end dates.
2. Open the project's **Detail** view and click **+ Enrol Student** for each student, setting their time extension (0/10/20/25%) and rest-breaks flag.

### Record a class session

1. From the project's **Sessions** list, click **+ Add Session** — date, start/end time, supervisor, Type = *Class*.
2. Click **Attendance** on the session row.
3. Enter minutes present per student. The row turns red with a warning if an entry exceeds the session's available time or would push the student over their total allowance.
4. Click **Save All**.

### Record an individual catch-up session

Same as above with Type = *Individual*, additionally choosing the one enrolled student the session is for. Only that student appears on the resulting attendance form. The session type and student cannot be changed after creation — delete and recreate the session if this is wrong.

### Bulk-import students

**Admin → Students → Import CSV...** — pick a UTF-8 CSV file (Excel's "CSV UTF-8" export format). Column headers are matched flexibly (looking for "candidate", "surname"/"lastname", "firstname"/"forename", "cis"), so minor naming differences in your spreadsheet aren't a problem. Every row is parsed and previewed first, with rows missing a required field flagged and excluded, before anything reaches the server. A candidate number that already exists is updated rather than duplicated; new students are created active by default.

### Generate a report

From a project's Dashboard card, Detail view, or Sessions list, click **Report**. This opens a three-section print-ready report: Project Summary, Session Log, and Student Summary (with a per-session minutes breakdown for every enrolled student). From there:
- **Print / Save PDF** — uses the system print dialog.
- **Export CSV** — a single file with a Sessions section and a Students section.
- **Export Excel** — a workbook with separate Sessions and Students sheets.

---

## Authentication

- JWT (HS256) via `firebase/php-jwt`. A single access token is issued at login, valid for 8 hours by default (`jwt.accessExpiry`).
- There is **no refresh token** — this is a deliberate simplification. When the token expires, the app detects the resulting 401 and returns to the login screen; the user simply signs in again.
- Token payload: `{ sub (staff id), role, must_change_password, iat, exp }`.
- Every request (other than login) carries `Authorization: Bearer <token>`.
- If `must_change_password === 1` on the account, the client forces a password change immediately after login, before the main application window ever opens.
- Tokens are held in memory only — closing the app logs the user out. There is no "remember me".

---

## API Reference

All endpoints live under the served `osp/` folder and return JSON in the shape `{ "success": true|false, "data": ..., "error": "..." }`, with an appropriate HTTP status code (`400` validation, `401` auth, `403` forbidden, `404` not found, `409` conflict, `500` server error).

### `auth/`

| Endpoint                | Method | Auth  | Description                                  |
|--------------------------|--------|-------|------------------------------------------------|
| `login.php`              | POST   | None  | `{ username, password }` → `{ token, staff }`  |
| `change-password.php`    | POST   | Any   | `{ current_password, new_password }`           |

### `staff/`

| Endpoint               | Method | Auth  | Description                                   |
|--------------------------|--------|-------|------------------------------------------------|
| `index.php`             | GET    | Any   | List all staff (active and inactive)          |
| `show.php?id=`          | GET    | Any   | Single staff record                            |
| `create.php`            | POST   | Admin | Create account → returns a temporary password  |
| `update.php`            | PUT    | Admin | Update name/email/role/active status           |
| `reset-password.php`    | POST   | Admin | Generate a new temporary password              |
| `delete.php`            | DELETE | Admin | Soft-deactivate (blocked if it's the last active admin) |

### `students/`

| Endpoint                | Method | Auth  | Description                                   |
|--------------------------|--------|-------|------------------------------------------------|
| `index.php`             | GET    | Any   | List active students                           |
| `show.php?id=`          | GET    | Any   | Single student                                 |
| `for-project.php?project_id=` | GET | Any | Enrolled students on a project, with running time totals |
| `create.php`            | POST   | Any   | Create a student                               |
| `update.php`            | PUT    | Any   | Update a student's details                     |
| `import.php`            | POST   | Any   | Bulk create/update from a parsed CSV — `{ students: [...] }` → `{ imported, updated, errors }` |
| `delete.php`            | DELETE | Admin | Soft-deactivate                                |

### `projects/`

| Endpoint                    | Method | Auth  | Description                               |
|-------------------------------|--------|-------|---------------------------------------------|
| `index.php`                 | GET    | Any   | All projects, with student count and scheduled minutes |
| `show.php?id=`              | GET    | Any   | Single project, with student/session counts |
| `create.php`                | POST   | Admin | Create a project                            |
| `update.php`                | PUT    | Admin | Update a project, including `is_active`     |
| `enrol.php`                 | POST   | Admin | Enrol a student with access arrangements    |
| `unenrol.php`               | DELETE | Admin | Remove a student (409 + `confirm:true` if attendance data exists) |
| `update-enrolment.php`      | PUT    | Admin | Change a student's access arrangements      |

### `sessions/`

| Endpoint                        | Method | Auth  | Description                             |
|-----------------------------------|--------|-------|--------------------------------------------|
| `index.php?project_id=`         | GET    | Any   | Sessions for a project                      |
| `show.php?id=`                  | GET    | Any   | A session plus its attendance records       |
| `next-number.php?project_id=`   | GET    | Any   | Preview the next sequential session number  |
| `create.php`                    | POST   | Any   | Create a session (transaction-safe numbering) |
| `update.php`                    | PUT    | Admin | Update mutable fields (not type or number)  |
| `delete.php`                    | DELETE | Admin | Delete (409 + `confirm:true` if attendance data exists) |

### `attendance/`

| Endpoint                              | Method | Auth | Description                              |
|------------------------------------------|--------|------|---------------------------------------------|
| `for-session.php?session_id=`           | GET    | Any  | Enrolled students + current minutes for a session |
| `save.php`                              | POST   | Any  | Upsert all attendance for a session in one call |
| `student-summary.php?project_student_id=` | GET  | Any  | Session-by-session breakdown for one student |

### `reports/`

| Endpoint                          | Method | Auth  | Description                                |
|--------------------------------------|--------|-------|-----------------------------------------------|
| `project-overview.php?project_id=` | GET    | Any   | Full report dataset (project, sessions, students with attendance) |
| `all-projects-summary.php`         | GET    | Admin | Aggregated stats across all active projects   |

---

## Application Screens

| Screen                 | Responsibility                                                         |
|--------------------------|--------------------------------------------------------------------------|
| `LoginForm`             | Username/password sign-in                                              |
| `ChangePasswordForm`    | Voluntary (menu) and forced (first login) password change              |
| `MainForm`              | Shell with Dashboard / Projects / Admin tabs, menu, status bar          |
| `DashboardPanel`        | Project cards — student count, base allowance, unscheduled time remaining |
| `ProjectsPanel`         | Browse-all-projects table with links to Detail/Sessions/Report         |
| `ProjectDetailForm`     | Project info, enrolled students (with % Used progress bar), enrol/edit/remove enrolment |
| `SessionListForm`       | Sessions for a project; scheduled-time and remaining-time footer, plus an Enrolled Students time-used panel with progress bars |
| `SessionFormDialog`     | Create or edit a session                                                |
| `AttendanceEntryForm`   | Per-student minutes entry with live over-time warnings and a live % Used progress bar |
| `ReportForm`            | Print-ready report: Project Summary / Session Log / Student Summary (with % Used progress bars) |
| `AdminTabPanel`         | Staff / Students / Projects management, admin only                     |
| `StaffPanel` / `StaffEditDialog` | Staff CRUD, password reset (clipboard-copies temp passwords)   |
| `StudentsPanel` / `StudentEditDialog` / `StudentImportDialog` | Student CRUD + CSV bulk import |
| `ProjectAdminPanel` / `ProjectEditDialog` | Project CRUD + activate/deactivate toggle         |

---

## Reporting & Export

### Print / PDF

The **Report** window renders a self-contained HTML report in a `WebBrowser` control. **Print / Save PDF** triggers the system print dialog — choose "Microsoft Print to PDF" (or any installed PDF printer) to save rather than print.

### CSV Export

A single `.csv` file with a Sessions section and a Students section (separated by a blank row and a label row), UTF-8 encoded with a BOM for correct accented-character display in Excel on Windows. The Students section includes one column per session showing that student's minutes for it.

### Excel Export

An `.xlsx` workbook (via ClosedXML) with two worksheets, **Sessions** and **Students**, column widths auto-fitted to content.

---

## Security Notes

- Database credentials and the JWT secret live only in `osp/.config.json`, which is git-ignored — never commit it.
- Rotate the JWT secret before any real deployment; a leaked secret lets anyone forge a valid token.
- All database queries use PDO prepared statements.
- Staff passwords are bcrypt-hashed (cost 12). Temporary passwords (new account / reset) are shown once and copied to the clipboard for the admin to communicate — they are never logged or stored in plain text.
- `Access-Control-Allow-Origin` is reflected only for `http://localhost[:port]` origins in `cors.php` — this exists for convenience if you ever point a browser-based tool at the API during development; the desktop client itself doesn't send an `Origin` header and isn't affected by it either way.

---

## Deployment

For a real (non-development) deployment:

- [ ] `osp/.config.json` has real database credentials, a strong random `jwt.secret`, and is not web-accessible (the bundled `.htaccess` already denies `.json` and `.log` files — confirm this is respected by your server config).
- [ ] HTTPS is configured — JWTs are sent on every request and should never travel in the clear.
- [ ] The first admin account's temporary password has been changed.
- [ ] `dotnet publish` the desktop client (e.g. `dotnet publish -c Release -r win-x64 --self-contained false`) and distribute the output folder alongside a `config.json` pointing at the production API URL.

---

## Changelog

### 0.0.0.3

- Added a colour-coded % Used progress bar (green under 80%, amber 80-99%, red 100%+), consistent across the Project Detail screen, the Attendance Entry screen (recalculates live as minutes are typed), a new Session List time-used panel, and the printed/exported report — shared via `Utils/ProgressCellRenderer.cs` for the grid views and matching CSS for the HTML report.
- Session List screen gained a resizable "Enrolled Students — Time Used" panel below the sessions grid, so time usage is visible without switching to Project Detail.
- Attendance Entry's Warnings column is now hard-locked against accidental editing (`CellBeginEdit` guard), and the grid's background colour was fixed to match the rest of the app.

### 0.0.0.2

- Added CSV bulk student import (**Admin → Students → Import CSV...**), with flexible column-header matching and a validation preview — rows missing required fields are flagged and excluded before anything reaches the server; an existing candidate number is updated rather than duplicated.
- Staff temporary passwords (new account creation and password reset) are now copied to the clipboard automatically, ready to paste into an email.
- Fixed a WinForms lifecycle bug where logging out could silently close the entire application, rather than returning to the login screen.
- Fixed a bug where a failed login attempt was incorrectly reported as "Session expired" instead of the actual reason (e.g. wrong password).

---

## License

Released under the **Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0)** license.

You are free to share and adapt this work for non-commercial purposes, provided you give appropriate credit and distribute any derivatives under the same license.

© 2026 Exeter College.
