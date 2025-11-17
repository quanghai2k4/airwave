using AirWave.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace AirWave.Client.Services;

public class AqiService
{
    private readonly HttpClient _httpClient;

    public AqiService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(apiSettings.Value.BaseUrl);
    }

    public async Task<List<AqiDataDto>?> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<AqiDataDto>>("api/aqi");
    }

    public async Task<AqiDataDto?> GetLatestAsync()
    {
        return await _httpClient.GetFromJsonAsync<AqiDataDto>("api/aqi/latest");
    }

    public async Task<List<AqiDataDto>?> GetFilteredAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = "api/aqi/filter?";
        if (startDate.HasValue)
        {
            query += $"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ss}&";
        }
        if (endDate.HasValue)
        {
            query += $"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ss}";
        }

        return await _httpClient.GetFromJsonAsync<List<AqiDataDto>>(query);
    }
}

public class AqiDataDto
{
    public int Id { get; set; }
    public int AqiValue { get; set; }
    public DateTime Timestamp { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
