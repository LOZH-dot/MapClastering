using System.Globalization;
using System.Text.Json;

namespace MapClastering
{
    internal class Program
    {
        static HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            // Входной массив адресов
            Application[] applications = new Application[]
            {
                new Application { Address = "Владимир, Лакина, 2", Type = ApplicationType.G_PON },
                new Application { Address = "Владимир, Лакина, 2А", Type = ApplicationType.G_PON },

                new Application { Address = "Владимир, Тракторная, 58", Type = ApplicationType.PACKET },
                new Application { Address = "Владимир, Тракторная, 1", Type = ApplicationType.G_PON },
                new Application { Address = "Владимир, Промышленный проезд, 5", Type = ApplicationType.PACKET },

                new Application { Address = "Владимир, Верхняя Дуброва, 6", Type = ApplicationType.G_PON },
                new Application { Address = "Владимир, Верхняя Дуброва 22", Type = ApplicationType.PACKET },
            };

            // Геокодируем адреса
            List<Coordinates> coordsList = new List<Coordinates>();
            foreach (var application in applications)
            {
                var coords = await GeocodeAddress(application.Address);
                if (coords != null)
                {
                    coords.Address = application.Address;
                    coordsList.Add(coords);
                }
            }

            // Параметр кластеризации: максимальное расстояние в метрах внутри кластера
            double maxClusterDistance = 500; // 2 км
            Console.WriteLine($"Максимальное расстояние между двумя точками: {maxClusterDistance} метров");

            // Кластеризация
            var clusters = ClusterCoordinates(coordsList, maxClusterDistance);

            // Вывод результатов
            for (int i = 0; i < clusters.Count; i++)
            {
                Console.WriteLine($"Кластер {i + 1}:");
                foreach (var coord in clusters[i])
                {
                    Console.WriteLine($"  {coord.Address} ({coord.Latitude}, {coord.Longitude})");
                }
            }
        }

        static async Task<Coordinates> GeocodeAddress(string address)
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
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при geocode адреса {address}: {ex.Message}");
                return null;
            }
        }

        static List<List<Coordinates>> ClusterCoordinates(List<Coordinates> coords, double maxDistance)
        {
            var clusters = new List<List<Coordinates>>();
            var unassigned = new List<Coordinates>(coords);

            while (unassigned.Count > 0)
            {
                var cluster = new List<Coordinates>();
                var seed = unassigned[0];
                cluster.Add(seed);
                unassigned.RemoveAt(0);

                bool added;
                do
                {
                    added = false;
                    for (int i = unassigned.Count - 1; i >= 0; i--)
                    {
                        var point = unassigned[i];
                        // Проверка, если точка в пределах maxDistance от любой точки в кластере
                        foreach (var c in cluster)
                        {
                            if (HaversineDistance(c.Latitude, c.Longitude, point.Latitude, point.Longitude) <= maxDistance)
                            {
                                cluster.Add(point);
                                unassigned.RemoveAt(i);
                                added = true;
                                break;
                            }
                        }
                    }
                } while (added);
                clusters.Add(cluster);
            }

            return clusters;
        }

        // Расчет расстояния между двумя точками по формуле Haversine
        static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // радиус Земли в метрах
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
