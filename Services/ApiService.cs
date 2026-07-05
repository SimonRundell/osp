/**
 * HTTP client wrapper for the OSP Hours Tracker PHP REST API.
 *
 * Attaches the Bearer JWT to every request. The API issues a single
 * 8-hour access token at login with no refresh mechanism — on HTTP 401
 * the session is considered expired and the caller is logged out so the
 * user can sign in again. Tokens are held in memory only (no persistence
 * between app launches).
 *
 * © 2026 Exeter College — Creative Commons NC-BY-SA 4.0
 */
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using OSPTracker.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OSPTracker.Services
{
    public class ApiService
    {
        // Singleton shared across the application
        public static readonly ApiService Instance = new ApiService();

        private readonly HttpClient _http;
        private string _token;
        public StaffDto CurrentStaff { get; private set; }
        public bool IsAuthenticated => CurrentStaff != null;

        private ApiService()
        {
            _http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        private string BaseUrl => AppConfig.Settings.ApiBaseUrl.TrimEnd('/');

        // ----------------------------------------------------------------
        // Auth
        // ----------------------------------------------------------------

        /// <summary>Login with username and password. Stores the access token in memory.</summary>
        public async Task LoginAsync(string username, string password)
        {
            var payload  = new { username, password };
            var response = await PostRawAsync("/auth/login.php", payload);
            var result   = JsonConvert.DeserializeObject<LoginResponse>(response);
            _token       = result.Data.Token;
            CurrentStaff = result.Data.Staff;
        }

        /// <summary>Clear the stored token and current staff member.</summary>
        public void Logout()
        {
            _token       = null;
            CurrentStaff = null;
        }

        /// <summary>Change the current user's password.</summary>
        public async Task ChangePasswordAsync(string currentPassword, string newPassword)
        {
            await PostAsync<SingleResponse<MessageResult>>("/auth/change-password.php", new
            {
                current_password = currentPassword,
                new_password     = newPassword,
            });
        }

        // ----------------------------------------------------------------
        // Public typed request helpers
        // ----------------------------------------------------------------

        /// <summary>HTTP GET, deserialises response as T.</summary>
        public async Task<T> GetAsync<T>(string path, string query = null)
        {
            string url = BaseUrl + path + (query != null ? "?" + query : "");
            var req = BuildRequest(HttpMethod.Get, url);
            string body = await SendAsync(req);
            return JsonConvert.DeserializeObject<T>(body);
        }

        /// <summary>HTTP POST, deserialises response as T.</summary>
        public async Task<T> PostAsync<T>(string path, object data)
        {
            string body = await PostRawAsync(path, data);
            return JsonConvert.DeserializeObject<T>(body);
        }

        /// <summary>HTTP PUT, deserialises response as T.</summary>
        public async Task<T> PutAsync<T>(string path, object data)
        {
            string url  = BaseUrl + path;
            string json = JsonConvert.SerializeObject(data);
            var req = BuildRequest(HttpMethod.Put, url, json);
            string body = await SendAsync(req);
            return JsonConvert.DeserializeObject<T>(body);
        }

        /// <summary>HTTP DELETE with a JSON body (required by endpoints that read fields from php://input).</summary>
        public async Task<T> DeleteAsync<T>(string path, object data)
        {
            string url  = BaseUrl + path;
            string json = JsonConvert.SerializeObject(data);
            var req = BuildRequest(HttpMethod.Delete, url, json);
            string body = await SendAsync(req);
            return JsonConvert.DeserializeObject<T>(body);
        }

        // ----------------------------------------------------------------
        // Internals
        // ----------------------------------------------------------------

        private async Task<string> PostRawAsync(string path, object data)
        {
            string url  = BaseUrl + path;
            string json = JsonConvert.SerializeObject(data);
            var req = BuildRequest(HttpMethod.Post, url, json);
            return await SendAsync(req);
        }

        private HttpRequestMessage BuildRequest(HttpMethod method, string url, string jsonBody = null)
        {
            var req = new HttpRequestMessage(method, url);
            if (!string.IsNullOrEmpty(_token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            if (jsonBody != null)
                req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            return req;
        }

        /// <summary>
        /// Sends the request. A 401 on an authenticated request (one that carried a
        /// Bearer token) means the session has expired — logs out and throws a
        /// specific message. A 401 with no token attached (e.g. a login attempt with
        /// bad credentials) is not a session expiry — it falls through so the
        /// server's actual error message ("Invalid username or password") surfaces.
        /// </summary>
        private async Task<string> SendAsync(HttpRequestMessage req)
        {
            bool hadToken = req.Headers.Authorization != null;
            var response = await _http.SendAsync(req);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && hadToken)
            {
                Logout();
                throw new UnauthorizedAccessException("Session expired. Please log in again.");
            }

            await EnsureSuccessAsync(response);
            return await response.Content.ReadAsStringAsync();
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            string body = string.Empty;
            try { body = await response.Content.ReadAsStringAsync(); } catch { }

            string message = response.ReasonPhrase;
            try
            {
                var err = JObject.Parse(body);
                string errMsg = err["error"]?.ToString() ?? err["message"]?.ToString();
                if (!string.IsNullOrEmpty(errMsg)) message = errMsg;
            }
            catch { }

            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {message}");
        }
    }
}
