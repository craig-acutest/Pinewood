using Pinewood.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Pinewood.Services
{
    public class CustomerApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CustomerApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            var apiUrl = _configuration.GetValue<string>("ApiUrl");
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(apiUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));            
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            var response = await _httpClient.GetAsync("api/Customers");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Customer>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Customers/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Customer>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task CreateCustomerAsync(Customer customer)
        {
            var json = JsonSerializer.Serialize(customer);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Customers", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateCustomerAsync(int id, Customer customer)
        {
            var json = JsonSerializer.Serialize(customer);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/Customers/{id}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Customers/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
