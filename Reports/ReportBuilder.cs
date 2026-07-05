/**
 * ReportBuilder — generates a self-contained HTML report for a project.
 *
 * HTML is loaded into the WinForms WebBrowser control for display,
 * printing and "Save as PDF" via the browser's own print dialog. Mirrors
 * the three-section layout of the original React app's ReportPage
 * (Project Summary / Session Log / Student Summary).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Linq;
using System.Text;
using OSPTracker.Models;

namespace OSPTracker.Reports
{
    public static class ReportBuilder
    {
        private const string Css = @"
            body   { font-family: 'Trebuchet MS', sans-serif; font-size: 11px; margin: 20px; color: #222; }
            h1     { color: #1a5276; font-size: 18px; margin-bottom: 4px; }
            h2     { font-size: 13px; margin: 18px 0 6px; color: #1a5276; border-bottom: 1px solid #ddd; padding-bottom: 3px; }
            .meta  { color: #555; font-size: 11px; margin-bottom: 12px; }
            table  { border-collapse: collapse; width: 100%; margin-top: 6px; font-size: 11px; }
            th     { background: #1a5276; color: #fff; padding: 5px 8px; text-align: left; white-space: nowrap; }
            td     { padding: 4px 8px; border-bottom: 1px solid #ddd; vertical-align: middle; }
            tr:nth-child(even) td { background: #f4f8fc; }
            tfoot td { font-weight: bold; border-top: 2px solid #999; }
            .danger { color: #c0392b; font-weight: bold; }
            .foot   { margin-top: 20px; font-size: 10px; color: #888; }
            .progress-track { background: #e2e2e2; border-radius: 3px; height: 11px; width: 70px;
                               display: inline-block; vertical-align: middle; overflow: hidden; }
            .progress-bar   { height: 11px; float: left; }
            .progress-bar.success { background: #27ae60; }
            .progress-bar.accent  { background: #e67e22; }
            .progress-bar.danger  { background: #c0392b; }
            .pct-label { display: inline-block; margin-left: 6px; vertical-align: middle; white-space: nowrap; }
            @media print { body { margin: 8mm; } @page { size: A4 landscape; } }
        ";

        public static string Build(ReportOverviewDto data)
        {
            var sb = new StringBuilder();
            sb.Append($"<!DOCTYPE html><html><head><meta charset='utf-8'><title>{Enc(data.Project.Name)} — Report</title><style>{Css}</style></head><body>");
            sb.Append($"<h1>{Enc(data.Project.Name)}</h1>");
            sb.Append($"<p class='meta'>Year: {data.Project.Year} &bull; Centre: {Enc(data.Project.CentreNumber)} &bull; Generated: {Enc(data.GeneratedAt)}</p>");

            // Section 1: Project Summary
            sb.Append("<h2>Project Summary</h2><table><tbody>");
            sb.Append($"<tr><td style='font-weight:600;width:220px'>Base Hours</td><td>{data.Project.BaseHours}h ({data.TotalProjectMinutes} mins)</td></tr>");
            sb.Append($"<tr><td style='font-weight:600'>Sessions Completed</td><td>{data.Sessions.Count}</td></tr>");
            sb.Append($"<tr><td style='font-weight:600'>Total Scheduled Time</td><td>{data.ScheduledMinutes} mins ({data.ScheduledMinutes / 60.0:0.0}h)</td></tr>");
            string remCls = data.RemainingMinutes < 0 ? " class='danger'" : "";
            sb.Append($"<tr><td style='font-weight:600'>Remaining Unscheduled Time</td><td{remCls}>{data.RemainingMinutes} mins ({data.RemainingMinutes / 60.0:0.0}h)</td></tr>");
            sb.Append($"<tr><td style='font-weight:600'>Students Enrolled</td><td>{data.Students.Count}</td></tr>");
            sb.Append($"<tr><td style='font-weight:600'>Start Date</td><td>{Enc(data.Project.StartDate ?? "—")}</td></tr>");
            sb.Append($"<tr><td style='font-weight:600'>End Date</td><td>{Enc(data.Project.EndDate ?? "—")}</td></tr>");
            sb.Append("</tbody></table>");

            // Section 2: Session Log
            sb.Append("<h2>Session Log</h2><table><thead><tr>");
            sb.Append("<th>#</th><th>Date</th><th>Start</th><th>End</th><th>Available Mins</th><th>Type</th><th>Supervisor</th>");
            sb.Append("</tr></thead><tbody>");
            foreach (var s in data.Sessions)
            {
                sb.Append($"<tr><td>{s.SessionNumber}</td><td>{Enc(s.SessionDate)}</td><td>{Enc(s.StartTimeShort)}</td><td>{Enc(s.EndTimeShort)}</td>");
                sb.Append($"<td>{(int)System.Math.Round(s.AvailableMinutes)}</td><td>{Enc(s.SessionType)}</td><td>{Enc(s.SupervisorName)}</td></tr>");
            }
            sb.Append("</tbody><tfoot>");
            sb.Append($"<tr><td colspan='4' style='text-align:right'>Total scheduled:</td><td>{data.ScheduledMinutes} mins</td><td colspan='2'></td></tr>");
            sb.Append($"<tr><td colspan='4' style='text-align:right'>Remaining unscheduled:</td><td{remCls}>{data.RemainingMinutes} mins</td><td colspan='2'></td></tr>");
            sb.Append("</tfoot></table>");

            // Section 3: Student Summary
            sb.Append("<h2>Student Summary</h2><table><thead><tr>");
            sb.Append("<th>Candidate #</th><th>CIS Ref</th><th>Surname</th><th>First Name</th><th>+Ext%</th><th>Rest Breaks</th>");
            sb.Append("<th>Allowed</th><th>Used</th><th>Remaining</th><th>% Used</th>");
            foreach (var s in data.Sessions) sb.Append($"<th>S{s.SessionNumber}</th>");
            sb.Append("</tr></thead><tbody>");
            foreach (var st in data.Students)
            {
                string remStuCls = st.MinutesRemaining < 0 ? " class='danger'" : "";
                sb.Append("<tr>");
                sb.Append($"<td>{Enc(st.CandidateNumber)}</td><td>{Enc(st.CisRef ?? "—")}</td><td>{Enc(st.Surname)}</td><td>{Enc(st.FirstName)}</td>");
                sb.Append($"<td>{(st.TimeExtensionPercent > 0 ? "+" + st.TimeExtensionPercent + "%" : "—")}</td>");
                sb.Append($"<td>{(st.RestBreaks == 1 ? "Yes" : "—")}</td>");
                sb.Append($"<td>{st.TotalMinutesAllowed}</td><td>{st.TotalMinutesUsed}</td><td{remStuCls}>{st.MinutesRemaining}</td><td>{ProgressBarHtml(st.PercentUsed)}</td>");
                foreach (var s in data.Sessions) sb.Append($"<td style='text-align:right'>{st.MinutesFor(s.SessionNumber)}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table>");

            sb.Append("<p class='foot'>OSP Hours Tracker &mdash; © 2026 Exeter College &mdash; Creative Commons NC-BY-SA 4.0</p>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        /// <summary>Renders a percent-used progress bar (green &lt;80%, amber 80-99%, red &gt;=100%), matching AttendanceEntryForm/ProjectDetailForm's colour thresholds.</summary>
        private static string ProgressBarHtml(int percent)
        {
            string cls = percent >= 100 ? "danger" : percent >= 80 ? "accent" : "success";
            int barWidth = System.Math.Max(0, System.Math.Min(percent, 100));
            return $"<div class='progress-track'><div class='progress-bar {cls}' style='width:{barWidth}%'></div></div>" +
                   $"<span class='pct-label'>{percent}%</span>";
        }

        private static string Enc(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");
    }
}
