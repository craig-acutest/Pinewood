using Newtonsoft.Json;
using System.Net.Http.Headers;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public AuthApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _configuration = configuration;
        var apiUrl = _configuration.GetValue<string>("ApiUrl");
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(apiUrl);
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(bool IsAuthenticated, List<string> Roles)> IsLoggedInAsync()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["PinewoodAuthToken"];
        if (string.IsNullOrEmpty(token))
        {
            return (false, null);
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync("api/auth/is-logged-in");
        if (!response.IsSuccessStatusCode)
        {
            return (false, null);
        }

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<AuthResponse>(content);

        return (true, result.Roles);
    }
}

// Define a model to hold the response from the API
public class AuthResponse
{
    public string Data { get; set; }
    public List<string> Roles { get; set; }
}
