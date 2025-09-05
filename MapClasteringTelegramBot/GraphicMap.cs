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
            // Находим минимальные и максимальные координаты
            double minLon = double.MaxValue;
            double maxLon = double.MinValue;
            double minLat = double.MaxValue;
            double maxLat = double.MinValue;

            foreach (var point in points)
            {
                if (point.Longitude < minLon) minLon = point.Longitude;
                if (point.Longitude > maxLon) maxLon = point.Longitude;
                if (point.Latitude < minLat) minLat = point.Latitude;
                if (point.Latitude > maxLat) maxLat = point.Latitude;
            }

            // Центр карты
            double centerLon = (minLon + maxLon) / 2;
            double centerLat = (minLat + maxLat) / 2;

            // Диапазоны широты и долготы
            double deltaLon = maxLon - minLon;
            double deltaLat = maxLat - minLat;

            // Размер изображения в пикселях
            int width = 600;
            int height = 450;

            int zoom = CalculateZoomLevel(points, minLat, maxLat, minLon, maxLon);

            // Используем сервис Static Map API
            // Для этого примера возьмем [Static Map API от Yandex](https://tech.yandex.ru/maps/doc/geosearch/concepts/static-docpage/)
            // Их формат URL:
            // https://static-maps.yandex.ru/1.x/?ll=37.6173,55.7558&size=450,450&z=10&l=map&pt=37.6173,55.7558,pm2rdm|37.6491,55.6924,pm2rdm

            string baseUrl = "https://static-maps.yandex.ru/1.x/";

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

            string url = $"{baseUrl}?ll={centerLon.ToString().Replace(',', '.')},{centerLat.ToString().Replace(',', '.')}&size={width},{height}&z={zoom}&l=map&pt={ptString}";
            return url;
        }

        public static async Task<byte[]> DownloadImageAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetByteArrayAsync(url);
            }
        }

        static int CalculateZoomLevel(List<Coordinates> points, double minLat, double maxLat, double minLon, double maxLon)
        {
            // Определим глобальные границы
            double deltaLat = maxLat - minLat;
            double deltaLon = maxLon - minLon;

            // Размеры карты в пикселях
            int width = 600;
            int height = 400;

            // Минимальный масштаб
            int minZoom = 0;
            int maxZoom = 17; // максимально допустимый уровень zoom для Yandex Static Maps

            for (int zoom = maxZoom; zoom >= minZoom; zoom--)
            {
                // Расчет высоты и ширины области на данном zoom
                double scale = Math.Pow(2, zoom);

                // Высота и ширина области в метках (метка = 1/2^zoom границ в терминах карт)
                // Математика основана на том, что:

                // Функция для получения по ширине и высоте текущего zoom
                double mapHeightInMeters = GetMapHeightInMeters(zoom);
                double mapWidthInMeters = GetMapWidthInMeters(zoom);

                // Преобразуем в пиксели
                double latDegPerPixel = 360.0 / (256 * scale);
                double lonDegPerPixel = 360.0 / (256 * scale);

                // Расстояние в градусах по широте и долготе
                double latDeltaInPixels = deltaLat / latDegPerPixel;
                double lonDeltaInPixels = deltaLon / lonDegPerPixel;

                // Проверяем, помещаются ли все точки в текущий zoom
                if (latDeltaInPixels <= width && lonDeltaInPixels <= height)
                {
                    return zoom;
                }
            }

            return minZoom; // Если не помещаются, берем минимальный zoom
        }

        static double GetMapHeightInMeters(int zoom)
        {
            return 20037508.34 * 2;
        }

        static double GetMapWidthInMeters(int zoom)
        {
            return GetMapHeightInMeters(zoom);
        }
    }
}
