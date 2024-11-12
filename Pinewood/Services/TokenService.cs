using System.Net.Http.Headers;
using System.Text.Json;

namespace Pinewood.Services
{
    public class TokenService : ITokenService
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public TokenService(HttpClient httpClient, ICacheService cacheService, IConfiguration configuration)
        {
            _configuration = configuration;
            var apiUrl = _configuration.GetValue<string>("ApiUrl");
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri($"{apiUrl}/api/auth/");
            _cacheService = cacheService;            
        }

        public async Task<string> LoginAsync(Pinewood.Models.LoginRequest loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("token", out JsonElement tokenElement))
                    {
                        var token = tokenElement.GetString();
                        _cacheService.Update<string>($"jwt_token_{loginRequest.Email}", new List<string>() { token });
                        return token;
                    }
                    else
                    {
                        throw new Exception("Token property not found in response.");
                    }
                }
            }
            else
            {
                throw new Exception("Login failed. Please check your credentials.");
            }
        }

        public async Task<bool> LoggedIn(string email)
        {
            var token = GetToken(email);

            if (token != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                return false;
            }

            _httpClient.DefaultRequestHeaders.Referrer = new System.Uri("https://web.pinewood.co.uk");

            var response = await _httpClient.GetAsync("secure");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string? GetToken(string email)
        {
            return _cacheService.Get<string>($"jwt_token_{email}").FirstOrDefault();
        }

        public void DeleteToken(string email)
        {
            _cacheService.Remove($"jwt_token_{email}");
        }
    }
}
