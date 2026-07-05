/**
 * DTOs for the attendance endpoint (attendance entry form + student summary).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OSPTracker.Models
{
    public class AttendanceSessionInfoDto
    {
        [JsonProperty("project_id")] public int    ProjectId { get; set; }
        [JsonProperty("start_time")] public string StartTime { get; set; }
        [JsonProperty("end_time")]   public string EndTime   { get; set; }
    }

    public class AttendanceStudentDto
    {
        [JsonProperty("project_student_id")]      public int    ProjectStudentId      { get; set; }
        [JsonProperty("student_id")]               public int    StudentId             { get; set; }
        [JsonProperty("candidate_number")]         public string CandidateNumber       { get; set; }
        [JsonProperty("cis_ref")]                  public string CisRef                { get; set; }
        [JsonProperty("surname")]                  public string Surname               { get; set; }
        [JsonProperty("first_name")]               public string FirstName             { get; set; }
        [JsonProperty("time_extension_percent")]  public int    TimeExtensionPercent  { get; set; }
        [JsonProperty("rest_breaks")]              public int    RestBreaks            { get; set; }
        [JsonProperty("total_minutes_allowed")]   public int    TotalMinutesAllowed   { get; set; }
        [JsonProperty("total_minutes_used")]      public int    TotalMinutesUsed      { get; set; }
        [JsonProperty("minutes_remaining")]       public int    MinutesRemaining      { get; set; }
        [JsonProperty("minutes_present")]         public int    MinutesPresent        { get; set; }
        [JsonProperty("attendance_id")]            public int?   AttendanceId          { get; set; }
    }

    public class AttendanceForSessionDto
    {
        [JsonProperty("session")]            public AttendanceSessionInfoDto  Session           { get; set; }
        [JsonProperty("available_minutes")]  public double                    AvailableMinutes  { get; set; }
        [JsonProperty("students")]           public List<AttendanceStudentDto> Students         { get; set; }
    }

    public class AttendanceRecordUpdate
    {
        [JsonProperty("project_student_id")] public int ProjectStudentId { get; set; }
        [JsonProperty("minutes_present")]    public int MinutesPresent   { get; set; }
    }

    public class StudentSummarySessionDto
    {
        [JsonProperty("id")]                 public int    Id                { get; set; }
        [JsonProperty("session_id")]         public int    SessionId         { get; set; }
        [JsonProperty("session_number")]     public int    SessionNumber     { get; set; }
        [JsonProperty("session_date")]       public string SessionDate       { get; set; }
        [JsonProperty("start_time")]         public string StartTime         { get; set; }
        [JsonProperty("end_time")]           public string EndTime           { get; set; }
        [JsonProperty("available_minutes")]  public double AvailableMinutes  { get; set; }
        [JsonProperty("minutes_present")]    public int    MinutesPresent    { get; set; }
    }

    public class StudentSummaryTotalsDto
    {
        [JsonProperty("total_minutes_allowed")] public int TotalMinutesAllowed { get; set; }
        [JsonProperty("total_minutes_used")]    public int TotalMinutesUsed    { get; set; }
        [JsonProperty("minutes_remaining")]     public int MinutesRemaining    { get; set; }
    }

    public class StudentSummaryDto
    {
        [JsonProperty("sessions")] public List<StudentSummarySessionDto> Sessions { get; set; }
        [JsonProperty("totals")]   public StudentSummaryTotalsDto        Totals   { get; set; }
    }
}
