namespace AirWave.Shared.Helpers;

public static class AqiHelper
{
    public static string GetAqiCategory(int aqiValue)
    {
        return aqiValue switch
        {
            <= 50 => "Good",
            <= 100 => "Moderate",
            <= 150 => "Unhealthy for Sensitive Groups",
            _ => "Unhealthy"
        };
    }

    public static string GetAqiColor(int aqiValue)
    {
        return aqiValue switch
        {
            <= 50 => "green",
            <= 100 => "yellow",
            <= 150 => "orange",
            _ => "red"
        };
    }
}
