/**
 * ExportHelper — CSV and Excel export for the project report, matching
 * the layout produced by the original React app's ExportButtons module.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClosedXML.Excel;
using OSPTracker.Models;

namespace OSPTracker.Reports
{
    public static class ExportHelper
    {
        /// <summary>Prompts for a save location and writes the report as a UTF-8 CSV file (two sections).</summary>
        public static void ExportCsv(ReportOverviewDto data, string projectName)
        {
            using var dlg = new SaveFileDialog
            {
                Filter   = "CSV files (*.csv)|*.csv",
                FileName = $"{SafeName(projectName)}_report.csv",
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var rows = new List<string[]>
            {
                new[] { $"OSP Hours Tracker — {data.Project.Name} ({data.Project.Year})" },
                Array.Empty<string>(),
                new[] { "SESSIONS" },
                new[] { "#", "Date", "Start", "End", "Available Mins", "Type", "Supervisor" },
            };
            foreach (var s in data.Sessions)
                rows.Add(new[] {
                    s.SessionNumber.ToString(), s.SessionDate, s.StartTimeShort, s.EndTimeShort,
                    ((int)Math.Round(s.AvailableMinutes)).ToString(), s.SessionType, s.SupervisorName,
                });
            rows.Add(new[] { "", "", "", "Total scheduled:", data.ScheduledMinutes.ToString() });
            rows.Add(new[] { "", "", "", "Remaining unscheduled:", data.RemainingMinutes.ToString() });
            rows.Add(Array.Empty<string>());
            rows.Add(new[] { "STUDENTS" });

            var studentHeader = new List<string> {
                "Candidate #", "CIS Ref", "Surname", "First Name", "+Ext%", "Rest Breaks",
                "Allowed Mins", "Used Mins", "Remaining", "% Used",
            };
            studentHeader.AddRange(data.Sessions.Select(s => $"Session {s.SessionNumber}"));
            rows.Add(studentHeader.ToArray());

            foreach (var st in data.Students)
            {
                var row = new List<string> {
                    st.CandidateNumber, st.CisRef ?? "", st.Surname, st.FirstName,
                    st.TimeExtensionPercent > 0 ? $"+{st.TimeExtensionPercent}%" : "0%",
                    st.RestBreaks == 1 ? "Yes" : "No",
                    st.TotalMinutesAllowed.ToString(), st.TotalMinutesUsed.ToString(),
                    st.MinutesRemaining.ToString(), $"{st.PercentUsed}%",
                };
                row.AddRange(data.Sessions.Select(s => st.MinutesFor(s.SessionNumber).ToString()));
                rows.Add(row.ToArray());
            }

            var sb = new StringBuilder();
            sb.Append('﻿');
            foreach (var row in rows)
                sb.AppendLine(string.Join(",", row.Select(CsvField)));

            File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>Prompts for a save location and writes the report as an Excel workbook (Sessions + Students sheets).</summary>
        public static void ExportExcel(ReportOverviewDto data, string projectName)
        {
            using var dlg = new SaveFileDialog
            {
                Filter   = "Excel workbook (*.xlsx)|*.xlsx",
                FileName = $"{SafeName(projectName)}_report.xlsx",
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            using var wb = new XLWorkbook();

            var wsS = wb.Worksheets.Add("Sessions");
            string[] sessHeaders = { "#", "Date", "Start", "End", "Available Mins", "Type", "Supervisor", "Notes" };
            for (int c = 0; c < sessHeaders.Length; c++) wsS.Cell(1, c + 1).Value = sessHeaders[c];
            int r = 2;
            foreach (var s in data.Sessions)
            {
                wsS.Cell(r, 1).Value = s.SessionNumber;
                wsS.Cell(r, 2).Value = s.SessionDate;
                wsS.Cell(r, 3).Value = s.StartTimeShort;
                wsS.Cell(r, 4).Value = s.EndTimeShort;
                wsS.Cell(r, 5).Value = (int)Math.Round(s.AvailableMinutes);
                wsS.Cell(r, 6).Value = s.SessionType;
                wsS.Cell(r, 7).Value = s.SupervisorName;
                wsS.Cell(r, 8).Value = s.Notes ?? "";
                r++;
            }
            wsS.Cell(r + 1, 4).Value = "Total scheduled:";
            wsS.Cell(r + 1, 5).Value = data.ScheduledMinutes;
            wsS.Cell(r + 2, 4).Value = "Remaining unscheduled:";
            wsS.Cell(r + 2, 5).Value = data.RemainingMinutes;
            wsS.Columns().AdjustToContents();

            var wsT = wb.Worksheets.Add("Students");
            var stuHeaders = new List<string> {
                "Candidate #", "CIS Ref", "Surname", "First Name", "Extension %", "Rest Breaks",
                "Allowed Mins", "Used Mins", "Remaining", "% Used",
            };
            stuHeaders.AddRange(data.Sessions.Select(s => $"Sess {s.SessionNumber}"));
            for (int c = 0; c < stuHeaders.Count; c++) wsT.Cell(1, c + 1).Value = stuHeaders[c];

            r = 2;
            foreach (var st in data.Students)
            {
                wsT.Cell(r, 1).Value = st.CandidateNumber;
                wsT.Cell(r, 2).Value = st.CisRef ?? "";
                wsT.Cell(r, 3).Value = st.Surname;
                wsT.Cell(r, 4).Value = st.FirstName;
                wsT.Cell(r, 5).Value = $"{st.TimeExtensionPercent}%";
                wsT.Cell(r, 6).Value = st.RestBreaks == 1 ? "Yes" : "No";
                wsT.Cell(r, 7).Value = st.TotalMinutesAllowed;
                wsT.Cell(r, 8).Value = st.TotalMinutesUsed;
                wsT.Cell(r, 9).Value = st.MinutesRemaining;
                wsT.Cell(r, 10).Value = $"{st.PercentUsed}%";
                int col = 11;
                foreach (var s in data.Sessions) wsT.Cell(r, col++).Value = st.MinutesFor(s.SessionNumber);
                r++;
            }
            wsT.Columns().AdjustToContents();

            wb.SaveAs(dlg.FileName);
        }

        private static string CsvField(string s)
        {
            s ??= "";
            return s.Contains(',') || s.Contains('"') || s.Contains('\n')
                ? "\"" + s.Replace("\"", "\"\"") + "\""
                : s;
        }

        private static string SafeName(string name) =>
            System.Text.RegularExpressions.Regex.Replace(name ?? "report", "[^a-zA-Z0-9_-]", "_");
    }
}
