using Telegram.Bot.Polling;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Globalization;
using System.Text.Json;

namespace MapClasteringTelegramBot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient { get; set; }
        private static ReceiverOptions _receiverOptions { get; set; }
        static async Task Main(string[] args)
        {
            _botClient = new TelegramBotClient("8249706098:AAFh-cwpwpe8T02n2HlrQSnRza5htZZH2ro");

            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
    {
                    UpdateType.Message
                },
                DropPendingUpdates = true
            };

            using var cts = new CancellationTokenSource();
            _botClient.StartReceiving(Bot.Handlers.UpdateHandler, Bot.Handlers.ErrorHandler, _receiverOptions, cts.Token);

            await Task.Delay(-1);
        }      
    }
}
