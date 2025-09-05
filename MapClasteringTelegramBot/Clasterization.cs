using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapClasteringTelegramBot
{
    internal class Clasterization
    {
        public static List<List<Coordinates>> ClusterCoordinates(List<Coordinates> coords, double maxDistance)
        {
            var clusters = new List<List<Coordinates>>();

            // для G-PON
            var unassigned = new List<Coordinates>(coords.Where(x => x.Application.Type == ApplicationType.G_PON));

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
                            if ((HaversineDistance(c.Latitude, c.Longitude, point.Latitude, point.Longitude) <= maxDistance))
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

            // Для PACKET

            unassigned = new List<Coordinates>(coords.Where(x => x.Application.Type == ApplicationType.PACKET));

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
                            if ((HaversineDistance(c.Latitude, c.Longitude, point.Latitude, point.Longitude) <= maxDistance))
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
        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
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

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
