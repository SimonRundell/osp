/**
 * Generic response wrappers and admin DTOs for staff and student management.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OSPTracker.Models
{
    // Generic list wrapper
    public class ListResponse<T>
    {
        [JsonProperty("data")] public List<T> Data { get; set; }
    }

    public class SingleResponse<T>
    {
        [JsonProperty("data")] public T Data { get; set; }
    }

    public class MessageResult
    {
        [JsonProperty("message")]       public string Message      { get; set; }
        [JsonProperty("id")]            public int    Id           { get; set; }
        [JsonProperty("temp_password")] public string TempPassword { get; set; }
        [JsonProperty("saved")]         public int    Saved        { get; set; }
    }

    public class StudentDto
    {
        [JsonProperty("id")]                public int    Id               { get; set; }
        [JsonProperty("candidate_number")]  public string CandidateNumber  { get; set; }
        [JsonProperty("cis_ref")]           public string CisRef           { get; set; }
        [JsonProperty("surname")]           public string Surname          { get; set; }
        [JsonProperty("first_name")]        public string FirstName        { get; set; }
        [JsonProperty("is_active")]         public int    IsActive         { get; set; }
        public override string ToString() => $"{Surname}, {FirstName} ({CandidateNumber})";
    }

    /// <summary>One row sent to /students/import.php — new students are created active by default.</summary>
    public class StudentImportRow
    {
        [JsonProperty("candidate_number")] public string CandidateNumber { get; set; }
        [JsonProperty("cis_ref")]          public string CisRef          { get; set; }
        [JsonProperty("surname")]          public string Surname         { get; set; }
        [JsonProperty("first_name")]       public string FirstName       { get; set; }
    }

    public class StudentImportResultDto
    {
        [JsonProperty("imported")] public int                         Imported { get; set; }
        [JsonProperty("updated")]  public int                         Updated  { get; set; }
        [JsonProperty("errors")]   public List<StudentImportErrorDto> Errors   { get; set; }
    }

    public class StudentImportErrorDto
    {
        [JsonProperty("row")]     public int    Row     { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
    }
}
