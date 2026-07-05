/**
 * DTOs for the projects endpoint and project-student enrolment.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using Newtonsoft.Json;

namespace OSPTracker.Models
{
    public class ProjectDto
    {
        [JsonProperty("id")]                 public int     Id                { get; set; }
        [JsonProperty("name")]               public string  Name              { get; set; }
        [JsonProperty("description")]        public string  Description       { get; set; }
        [JsonProperty("year")]                public int     Year              { get; set; }
        [JsonProperty("centre_number")]      public string  CentreNumber      { get; set; }
        [JsonProperty("base_hours")]         public decimal BaseHours         { get; set; }
        [JsonProperty("start_date")]         public string  StartDate         { get; set; }
        [JsonProperty("end_date")]           public string  EndDate           { get; set; }
        [JsonProperty("created_by")]         public int     CreatedBy         { get; set; }
        [JsonProperty("is_active")]          public int     IsActive          { get; set; }
        [JsonProperty("created_at")]         public string  CreatedAt         { get; set; }
        [JsonProperty("creator_name")]       public string  CreatorName       { get; set; }
        [JsonProperty("student_count")]      public int     StudentCount      { get; set; }
        [JsonProperty("session_count")]      public int     SessionCount      { get; set; }
        [JsonProperty("scheduled_minutes")]  public double  ScheduledMinutes  { get; set; }

        public int TotalProjectMinutes => (int)System.Math.Round(BaseHours * 60);
        public int RemainingUnscheduledMinutes => TotalProjectMinutes - (int)System.Math.Round(ScheduledMinutes);
        public override string ToString() => Name;
    }

    /// <summary>An enrolled student's access arrangements and running time totals for a project.</summary>
    public class ProjectStudentDto
    {
        [JsonProperty("project_student_id")]      public int    ProjectStudentId      { get; set; }
        [JsonProperty("student_id")]               public int    StudentId             { get; set; }
        [JsonProperty("candidate_number")]         public string CandidateNumber       { get; set; }
        [JsonProperty("cis_ref")]                  public string CisRef                { get; set; }
        [JsonProperty("surname")]                  public string Surname               { get; set; }
        [JsonProperty("first_name")]               public string FirstName             { get; set; }
        [JsonProperty("time_extension_percent")]  public int    TimeExtensionPercent  { get; set; }
        [JsonProperty("rest_breaks")]              public int    RestBreaks            { get; set; }
        [JsonProperty("notes")]                    public string Notes                 { get; set; }
        [JsonProperty("total_minutes_allowed")]   public int    TotalMinutesAllowed   { get; set; }
        [JsonProperty("total_minutes_used")]      public int    TotalMinutesUsed      { get; set; }
        [JsonProperty("minutes_remaining")]       public int    MinutesRemaining      { get; set; }

        public int PercentUsed => TotalMinutesAllowed > 0
            ? (int)System.Math.Round(100.0 * TotalMinutesUsed / TotalMinutesAllowed)
            : 0;
        public override string ToString() => $"{Surname}, {FirstName} ({CandidateNumber})";
    }
}
