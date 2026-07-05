/**
 * DTOs for the reports endpoint (full project overview and admin summary).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OSPTracker.Models
{
    public class ReportAttendanceDto
    {
        [JsonProperty("session_id")]      public int SessionId      { get; set; }
        [JsonProperty("session_number")]  public int SessionNumber  { get; set; }
        [JsonProperty("minutes_present")] public int MinutesPresent { get; set; }
    }

    /// <summary>A student row within the report overview, including per-session attendance.</summary>
    public class ReportStudentDto : ProjectStudentDto
    {
        [JsonProperty("attendance")] public List<ReportAttendanceDto> Attendance { get; set; }

        public int MinutesFor(int sessionNumber) =>
            Attendance?.Find(a => a.SessionNumber == sessionNumber)?.MinutesPresent ?? 0;
    }

    public class ReportOverviewDto
    {
        [JsonProperty("project")]                  public ProjectDto              Project                { get; set; }
        [JsonProperty("sessions")]                 public List<SessionDto>        Sessions               { get; set; }
        [JsonProperty("students")]                 public List<ReportStudentDto>  Students               { get; set; }
        [JsonProperty("total_available_minutes")] public double                   TotalAvailableMinutes  { get; set; }
        [JsonProperty("generated_at")]             public string                  GeneratedAt            { get; set; }

        public int TotalProjectMinutes => (int)System.Math.Round((double)Project.BaseHours * 60);
        public int ScheduledMinutes    => (int)System.Math.Round(TotalAvailableMinutes);
        public int RemainingMinutes    => TotalProjectMinutes - ScheduledMinutes;
    }

    public class AllProjectsSummaryDto
    {
        [JsonProperty("id")]                    public int     Id                   { get; set; }
        [JsonProperty("name")]                  public string  Name                 { get; set; }
        [JsonProperty("year")]                   public int     Year                 { get; set; }
        [JsonProperty("base_hours")]            public decimal BaseHours            { get; set; }
        [JsonProperty("is_active")]              public int     IsActive             { get; set; }
        [JsonProperty("student_count")]         public int     StudentCount         { get; set; }
        [JsonProperty("total_minutes_used")]    public int     TotalMinutesUsed     { get; set; }
        [JsonProperty("total_minutes_allowed")] public int     TotalMinutesAllowed  { get; set; }
    }
}
