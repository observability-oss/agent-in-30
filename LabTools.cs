using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace AgentLab;

/// <summary>
/// The agent's tools. Plain C# methods — what makes them visible in a trace
/// is that the agent invokes them through the instrumented pipeline, so each
/// call becomes a tool span with its arguments, result and duration.
///
/// GetWeather calls a real API (Open-Meteo — free, no key), so its span shows
/// genuine network time.
/// </summary>
public static class LabTools
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    [Description("Gets the current weather for a city.")]
    public static async Task<string> GetWeather(
        [Description("City name, e.g. Sofia")] string city)
    {
        try
        {
            var place = await Geocode(city);
            if (place is null) return $"I couldn't find a place called \"{city}\".";

            var url = "https://api.open-meteo.com/v1/forecast" +
                      $"?latitude={Coord(place.Value.Latitude)}" +
                      $"&longitude={Coord(place.Value.Longitude)}" +
                      "&current=temperature_2m,weather_code,wind_speed_10m";
            using var doc = JsonDocument.Parse(await Http.GetStringAsync(url));

            if (!doc.RootElement.TryGetProperty("current", out var current))
                return $"The weather service returned no current conditions for {place.Value.Name}.";

            var temperature = current.GetProperty("temperature_2m").GetDouble();
            var windSpeed = current.GetProperty("wind_speed_10m").GetDouble();
            var conditions = Describe(current.GetProperty("weather_code").GetInt32());

            return $"{place.Value.Name}: {temperature:0.#}°C, {conditions}, wind {windSpeed:0.#} km/h.";
        }
        catch (Exception ex)
        {
            // Returned rather than thrown so the agent can tell the user what
            // happened — and so the tool span records the failure honestly.
            return $"I couldn't reach the weather service for \"{city}\": {ex.Message}";
        }
    }

    [Description("Gets the current date and time in UTC.")]
    public static string GetCurrentTime() =>
        $"UTC now: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

    /// <summary>Turns a city name into coordinates, shared by the location tools.</summary>
    private static async Task<(double Latitude, double Longitude, string Name)?> Geocode(string city)
    {
        var url = "https://geocoding-api.open-meteo.com/v1/search" +
                  $"?name={Uri.EscapeDataString(city)}&count=1&language=en&format=json";
        using var doc = JsonDocument.Parse(await Http.GetStringAsync(url));

        if (!doc.RootElement.TryGetProperty("results", out var results) ||
            results.GetArrayLength() == 0)
        {
            return null;
        }

        var place = results[0];
        var name = place.TryGetProperty("name", out var n) ? n.GetString() : city;
        var country = place.TryGetProperty("country", out var c) ? c.GetString() : null;

        return (place.GetProperty("latitude").GetDouble(),
                place.GetProperty("longitude").GetDouble(),
                country is null ? name! : $"{name}, {country}");
    }

    /// <summary>Coordinates must use a dot, whatever the machine's locale says.</summary>
    private static string Coord(double value) =>
        value.ToString("0.####", CultureInfo.InvariantCulture);

    /// <summary>WMO weather interpretation codes, grouped to the nearest useful phrase.</summary>
    private static string Describe(int code) => code switch
    {
        0 => "clear sky",
        1 => "mainly clear",
        2 => "partly cloudy",
        3 => "overcast",
        45 or 48 => "foggy",
        >= 51 and <= 57 => "drizzle",
        >= 61 and <= 67 => "rain",
        >= 71 and <= 77 => "snow",
        >= 80 and <= 82 => "rain showers",
        85 or 86 => "snow showers",
        >= 95 and <= 99 => "thunderstorm",
        _ => "mixed conditions",
    };
}
