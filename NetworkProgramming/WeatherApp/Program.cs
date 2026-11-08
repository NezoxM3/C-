
using System.Text.Json;

record City(string Name, double Latitude, double Longitude);

List<City> cities = new List<City>
{
    new("Вінниця", 49.2331, 28.4682),
    new("Луцьк", 50.7472, 25.3254),
    new("Дніпро", 48.4647, 35.0462),
    new("Донецьк", 48.0159, 37.8029),
    new("Житомир", 50.2547, 28.6587),
    new("Ужгород", 48.6208, 22.2879),
    new("Запоріжжя", 47.8388, 35.1396),
    new("Івано-Франківськ", 48.9226, 24.7111),
    new("Київ", 50.4501, 30.5234),
    new("Кропивницький", 48.5079, 32.2623),
    new("Луганськ", 48.5740, 39.3078),
    new("Львів", 49.8397, 24.0297),
    new("Миколаїв", 46.9750, 31.9946),
    new("Одеса", 46.4825, 30.7233),
    new("Полтава", 49.5883, 34.5514),
    new("Рівне", 50.6199, 26.2516),
    new("Суми", 50.9077, 34.7981),
    new("Тернопіль", 49.5535, 25.5948),
    new("Харків", 49.9935, 36.2304),
    new("Херсон", 46.6354, 32.6169),
    new("Хмельницький", 49.4229, 26.9871),
    new("Черкаси", 49.4444, 32.0598),
    new("Чернівці", 48.2915, 25.9403),
    new("Чернігів", 51.4982, 31.2893),
    new("Сімферополь", 44.9521, 34.1024)
};

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("       UKRAINE WEATHER APP");
Console.WriteLine();

Console.WriteLine("Choose a city:");

for (int i = 0; i < cities.Count; i++)
{
    Console.WriteLine($"{i + 1}. {cities[i].Name}");
}

Console.WriteLine();
Console.Write("Enter city number: ");

if (!int.TryParse(Console.ReadLine(), out int choice) ||
    choice < 1 || choice > cities.Count)
{
    Console.WriteLine("Invalid city number.");
    return;
}

City selectedCity = cities[choice - 1];

Console.WriteLine();
Console.WriteLine($"Getting weather for {selectedCity.Name}...");
Console.WriteLine();

using HttpClient client = new HttpClient();

string url =
    "https://api.open-meteo.com/v1/forecast" +
    $"?latitude={selectedCity.Latitude}" +
    $"&longitude={selectedCity.Longitude}" +
    "&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation,weather_code,wind_speed_10m" +
    "&daily=temperature_2m_max,temperature_2m_min,weather_code" +
    "&timezone=auto" +
    "&forecast_days=1";

try
{
    string response = await client.GetStringAsync(url);

    using JsonDocument document = JsonDocument.Parse(response);

    JsonElement root = document.RootElement;
    JsonElement current = root.GetProperty("current");
    JsonElement daily = root.GetProperty("daily");

    string temperature = current.GetProperty("temperature_2m").ToString();
    string feelsLike = current.GetProperty("apparent_temperature").ToString();
    string humidity = current.GetProperty("relative_humidity_2m").ToString();
    string precipitation = current.GetProperty("precipitation").ToString();
    string windSpeed = current.GetProperty("wind_speed_10m").ToString();

    int weatherCode = current.GetProperty("weather_code").GetInt32();

    double maxTemperature =
        daily.GetProperty("temperature_2m_max")[0].GetDouble();

    double minTemperature =
        daily.GetProperty("temperature_2m_min")[0].GetDouble();

    int dailyWeatherCode =
        daily.GetProperty("weather_code")[0].GetInt32();

    Console.WriteLine($"Weather in {selectedCity.Name}");

    Console.WriteLine($"Condition: {GetWeatherDescription(weatherCode)}");
    Console.WriteLine($"Temperature: {temperature} °C");
    Console.WriteLine($"Feels like: {feelsLike} °C");
    Console.WriteLine($"Humidity: {humidity}%");
    Console.WriteLine($"Precipitation: {precipitation} mm");
    Console.WriteLine($"Wind speed: {windSpeed} km/h");

    Console.WriteLine();
    Console.WriteLine("Today's forecast:");
    Console.WriteLine($"Maximum temperature: {maxTemperature} °C");
    Console.WriteLine($"Minimum temperature: {minTemperature} °C");
    Console.WriteLine($"Forecast: {GetWeatherDescription(dailyWeatherCode)}");

}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"Error reading weather data: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}


string GetWeatherDescription(int code)
{
    return code switch
    {
        0 => "Clear sky",
        1 => "Mainly clear",
        2 => "Partly cloudy",
        3 => "Overcast",
        45 or 48 => "Fog",
        51 or 53 or 55 => "Drizzle",
        56 or 57 => "Freezing drizzle",
        61 or 63 or 65 => "Rain",
        66 or 67 => "Freezing rain",
        71 or 73 or 75 => "Snowfall",
        77 => "Snow grains",
        80 or 81 or 82 => "Rain showers",
        85 or 86 => "Snow showers",
        95 => "Thunderstorm",
        96 or 99 => "Thunderstorm with hail",
        _ => "Unknown weather condition"
    };
}
