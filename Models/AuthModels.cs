/**
 * Authentication DTOs returned by /auth/login.php.
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using Newtonsoft.Json;

namespace OSPTracker.Models
{
    public class StaffDto
    {
        [JsonProperty("id")]                    public int     Id                  { get; set; }
        [JsonProperty("username")]              public string  Username            { get; set; }
        [JsonProperty("email")]                 public string  Email               { get; set; }
        [JsonProperty("first_name")]            public string  FirstName           { get; set; }
        [JsonProperty("last_name")]             public string  LastName            { get; set; }
        [JsonProperty("role")]                  public string  Role                { get; set; }
        [JsonProperty("is_active")]             public int     IsActive            { get; set; }
        [JsonProperty("must_change_password")]  public int     MustChangePassword  { get; set; }
        [JsonProperty("last_login")]            public string  LastLogin           { get; set; }
        [JsonProperty("created_at")]            public string  CreatedAt           { get; set; }

        public bool IsAdmin => Role == "admin";
        public string FullName => $"{FirstName} {LastName}";
        public override string ToString() => FullName;
    }

    public class LoginResponseData
    {
        [JsonProperty("token")] public string   Token { get; set; }
        [JsonProperty("staff")] public StaffDto Staff { get; set; }
    }

    public class LoginResponse
    {
        [JsonProperty("data")] public LoginResponseData Data { get; set; }
    }
}
