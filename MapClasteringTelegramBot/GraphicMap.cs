using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
            int width = 600;
            int height = 450;
            // Формируем строку точек
            List<string> ptParams = new List<string>();
            foreach (var p in points)
            {
                if (p.Application.Type == ApplicationType.G_PON)
                    // 'pm2rdm' - маркер с красным лицом
                    ptParams.Add($"{p.Longitude.ToString().Replace(',', '.')},{p.Latitude.ToString().Replace(',', '.')},pm2rdm");
                else if (p.Application.Type == ApplicationType.PACKET)
                    // 'pm2dbm' - маркер с синим лицом
                    ptParams.Add($"{p.Longitude.ToString().Replace(',', '.')},{p.Latitude.ToString().Replace(',', '.')},pm2dbm");

            }
            string ptString = string.Join("|", ptParams);

            string url = $"{baseUrl}?size={width},{height}&l=map&pt={ptString}";
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
