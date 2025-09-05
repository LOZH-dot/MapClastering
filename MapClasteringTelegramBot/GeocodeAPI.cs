using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MapClasteringTelegramBot
{
    internal class GeocodeAPI
    {
        private static HttpClient client = new HttpClient();
        public static async Task<Coordinates> GeocodeAddress(string address)
        {
            try
            {
                string url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}&limit=1";

                // Важно указывать User-Agent при использовании Nominatim API
                if (!client.DefaultRequestHeaders.Contains("User-Agent"))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Map-Clastering-App-Trytek");
                }

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var results = JsonSerializer.Deserialize<List<NominatimResult>>(json, options);

                if (results != null && results.Count > 0)
                {
                    return new Coordinates
                    {
                        Latitude = double.Parse(results[0].Lat, CultureInfo.InvariantCulture),
                        Longitude = double.Parse(results[0].Lon, CultureInfo.InvariantCulture)
                    };
                }
                else
                {
                    Console.WriteLine($"Не удалось geocode для адреса: {address}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при geocode адреса {address}: {ex.Message}");
                return null;
            }
        }
    }
}
