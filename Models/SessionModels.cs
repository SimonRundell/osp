/**
 * DTOs for the sessions endpoint (session_summary view + attendance rows).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OSPTracker.Models
{
    public class SessionDto
    {
        [JsonProperty("session_id")]         public int    SessionId         { get; set; }
        [JsonProperty("project_id")]         public int    ProjectId         { get; set; }
        [JsonProperty("project_name")]       public string ProjectName       { get; set; }
        [JsonProperty("session_number")]     public int    SessionNumber     { get; set; }
        [JsonProperty("session_date")]       public string SessionDate       { get; set; }
        [JsonProperty("start_time")]         public string StartTime         { get; set; }
        [JsonProperty("end_time")]           public string EndTime           { get; set; }
        [JsonProperty("available_minutes")]  public double AvailableMinutes  { get; set; }
        [JsonProperty("session_type")]       public string SessionType       { get; set; }
        [JsonProperty("supervisor_id")]      public int    SupervisorId      { get; set; }
        [JsonProperty("supervisor_name")]    public string SupervisorName    { get; set; }
        [JsonProperty("notes")]              public string Notes             { get; set; }

        public string StartTimeShort => (StartTime ?? "").Length >= 5 ? StartTime.Substring(0, 5) : StartTime;
        public string EndTimeShort   => (EndTime   ?? "").Length >= 5 ? EndTime.Substring(0, 5)   : EndTime;
    }

    public class AttendanceRowDto
    {
        [JsonProperty("id")]                  public int    Id                 { get; set; }
        [JsonProperty("project_student_id")]  public int    ProjectStudentId   { get; set; }
        [JsonProperty("minutes_present")]     public int    MinutesPresent     { get; set; }
        [JsonProperty("candidate_number")]    public string CandidateNumber    { get; set; }
        [JsonProperty("surname")]             public string Surname            { get; set; }
        [JsonProperty("first_name")]          public string FirstName          { get; set; }
    }

    public class SessionDetailDto : SessionDto
    {
        [JsonProperty("attendance")] public List<AttendanceRowDto> Attendance { get; set; }
    }
}
