using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapClasteringTelegramBot
{
    internal class GraphicMap
    {
        public static string GenerateStaticMapUrl(List<Coordinates> points)
        {
            // Используем сервис Static Map API
            // Для этого примера возьмем [Static Map API от Yandex](https://tech.yandex.ru/maps/doc/geosearch/concepts/static-docpage/)
            // Их формат URL:
            // https://static-maps.yandex.ru/1.x/?ll=37.6173,55.7558&size=450,450&z=10&l=map&pt=37.6173,55.7558,pm2rdm|37.6491,55.6924,pm2rdm

            string baseUrl = "https://static-maps.yandex.ru/1.x/";
            string size = "600,400"; // размеры изображения
            int zoom = 11; // уровень масштабирования

            // Вычисляем центр карты
            double avgLat = 0;
            double avgLon = 0;
            foreach (var p in points)
            {
                avgLat += p.Latitude;
                avgLon += p.Longitude;
            }
            avgLat /= points.Count;
            avgLon /= points.Count;

            // Формируем строку точек
            List<string> ptParams = new List<string>();
            foreach (var p in points)
            {
                // 'pm2rdm' - маркер с красным лицом, можно выбрать другой стиль
                ptParams.Add($"{p.Longitude.ToString().Replace(',', '.')},{p.Latitude.ToString().Replace(',', '.')},pm2rdm");
            }
            string ptString = string.Join("|", ptParams);

            string url = $"{baseUrl}?ll={avgLon.ToString().Replace(',', '.')},{avgLat.ToString().Replace(',', '.')}&size={size}&z={zoom}&l=map&pt={ptString}";
            return url;
        }

        public static async Task<byte[]> DownloadImageAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetByteArrayAsync(url);
            }
        }
    }
}
