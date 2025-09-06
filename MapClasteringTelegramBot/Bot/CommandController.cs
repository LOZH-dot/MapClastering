using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MapClasteringTelegramBot.Bot
{
    internal class CommandController
    {
        public static async void Start(ITelegramBotClient client, Update update)
        {
            var message = update.Message;
            var from = message!.From;

            // Входной массив адресов
            Application[] applications = new Application[]
            {
                new Application { Address = "Владимир, Лакина, 2", Type = ApplicationType.G_PON, DateTime = DateTime.Now },
                new Application { Address = "Владимир, Лакина, 2А", Type = ApplicationType.G_PON, DateTime = DateTime.Now.AddHours(2) },

                new Application { Address = "Владимир, Тракторная, 58", Type = ApplicationType.PACKET, DateTime = DateTime.Now.AddHours(3) },
                new Application { Address = "Владимир, Тракторная, 1", Type = ApplicationType.G_PON, DateTime = DateTime.Now.AddHours(4)},
                new Application { Address = "Владимир, Промышленный проезд, 5", Type = ApplicationType.PACKET, DateTime = DateTime.Now.AddHours(5) },

                new Application { Address = "Владимир, Верхняя Дуброва, 6", Type = ApplicationType.G_PON, DateTime = DateTime.Now.AddHours(6)},
                new Application { Address = "Владимир, Верхняя Дуброва 22", Type = ApplicationType.PACKET, DateTime = DateTime.Now.AddHours(1)},
            };

            // Геокодируем адреса
            List<Coordinates> coordsList = new List<Coordinates>();
            foreach (var application in applications)
            {
                var coords = await GeocodeAPI.GeocodeAddress(application.Address);
                if (coords != null)
                {
                    coords.Application = application;
                    coordsList.Add(coords);
                }
            }

            // Параметр кластеризации: максимальное расстояние в метрах внутри кластера
            double maxClusterDistance = 1000;

            // Кластеризация
            var clusters = Clasterization.ClusterCoordinates(coordsList, maxClusterDistance);
            var result = string.Empty;

            result += $"Максимальное расстояние между двумя точками: {maxClusterDistance} метров\n\n";

            // Вывод результатов
            for (int i = 0; i < clusters.Count; i++)
            {
                result += ($"Кластер {i + 1}:\n");
                foreach (var coord in clusters[i])
                {
                    result += ($"  {coord.Application.Address} ({coord.Latitude}, {coord.Longitude}) [{coord.Application.DateTime.ToString("HH:mm")}] - {coord.Application.Type.ToString()}\n");
                }
                result += '\n';
            }

            await client.SendMessage(message.Chat, result);

            // Построение URL для Static Map
            List<Coordinates> AllPoints = new List<Coordinates>();

            foreach (var item in clusters)
                foreach (var point in item)
                    AllPoints.Add(point);


            string mapUrl = GraphicMap.GenerateStaticMapUrl(AllPoints);

            // Загружаем изображение карты
            byte[] imageBytes = await GraphicMap.DownloadImageAsync(mapUrl);

            await client.SendPhoto(
                chatId: message.Chat,
                photo: new System.IO.MemoryStream(imageBytes),
                caption: "Карта с точками для кластера"
                );
        }
    }
}
