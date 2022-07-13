using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using teko_bot;

var bot = new TelegramBotClient(BotConfiguration.BotToken);
var me = await bot.GetMeAsync();
using var cts = new CancellationTokenSource();
var receiverOptions = new ReceiverOptions()
{
    AllowedUpdates = Array.Empty<UpdateType>(),
    ThrowPendingUpdates = true,
};

Console.Title = me.Username ?? BotConfiguration.BotName;

bot.StartReceiving(updateHandler: UpdateHandlers.HandleUpdateAsync,
    pollingErrorHandler: UpdateHandlers.PollingErrorHandler,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token);

Console.WriteLine($"Подключен к @{me.Username}");

// Ждем считывание с консоли для остановки бота
Console.ReadLine();

// Отправка запроса для остановки бота
cts.Cancel();