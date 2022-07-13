using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace teko_bot;

public static class UpdateHandlers
{
    // обработка ошибок при обработке запросов
    public static Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception.ToString();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errorMessage);
        Console.ResetColor();
        return Task.CompletedTask;
    }

    // обработка запросов
    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
            UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
            UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
            UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
            _ => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await PollingErrorHandler(botClient, exception, cancellationToken);
        }
    }

    // обработка команд
    private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine($"Тип полученного сообщения: {message.Type}");
        var messageText = message.Text;
        if (messageText is null)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/inline" => SendInlineKeyboard(botClient, message),
            "/keyboard" => SendReplyKeyboard(botClient, message),
            "/remove" => RemoveKeyboard(botClient, message),
            "/sticker" => SendSticker(botClient, message),
            "/request" => RequestContactAndLocation(botClient, message),
            _ => Usage(botClient, message)
        };
        var sentMessage = await action;
        Console.WriteLine($"Сообщение было отправлено с id: {sentMessage.MessageId}");
    }

    // удаление клавиатуры
    private static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Клавиатура уничтожена",
            replyMarkup: new ReplyKeyboardRemove());
    }

    // отправка стикера
    private static async Task<Message> SendSticker(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendStickerAsync(
            chatId: message.Chat.Id,
            sticker: "https://github.com/TelegramBots/book/raw/master/src/docs/sticker-fred.webp");
    }

    // отправка местоположения или контакта пользователя
    private static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message)
    {
        ReplyKeyboardMarkup requestReplyKeyboard = new(
            new[]
            {
                KeyboardButton.WithRequestLocation("Местоположение"),
                KeyboardButton.WithRequestContact("Контакт"),
            });

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Что тебе показать?",
            replyMarkup: requestReplyKeyboard);
    }

    // сообщение - справка об использовании бота 
    private static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
    {
        const string usage = "Использование:\n" +
                             "/inline   - Отправка клавиатуры - сообщения\n" +
                             "/keyboard - Добавление обычной клавиатуры\n" +
                             "/remove   - Удаление обычной клавиатуры\n" +
                             "/sticker  - Отправка стикера\n" +
                             "/request  - Выявление контакта или местоположения";

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: usage,
            replyMarkup: new ReplyKeyboardRemove());
    }

    // добавление обычной клавиатуры
    private static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new(
            new[]
            {
                new KeyboardButton[] { "1.1", "1.2" },
                new KeyboardButton[] { "2.1", "2.2" },
            })
        {
            ResizeKeyboard = true
        };

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Выбирай",
            replyMarkup: replyKeyboardMarkup);
    }

    // отправка клавиатуры - сообщения
    private static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                // Первый ряд
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("1.1", "11"),
                    InlineKeyboardButton.WithCallbackData("1.2", "12"),
                },
                // Второй ряд
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("2.1", "21"),
                    InlineKeyboardButton.WithCallbackData("2.2", "22"),
                },
            });

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Выбирай",
            replyMarkup: inlineKeyboard);
    }

    // обработка ответа от пользоваеля
    private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        await botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"Получен ответ {callbackQuery.Data}");

        await botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: $"Получен ответ {callbackQuery.Data}");
    }

    // обработка входящих встроенных запросов
    private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
    {
        Console.WriteLine($"Встроенный запрос получен от: {inlineQuery.From.Id}");

        InlineQueryResult[] results =
        {
            // показываемые результаты
            new InlineQueryResultArticle(
                id: "1",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent(
                    "inline запрос"
                )
            )
        };

        await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
            results: results,
            isPersonal: true,
            cacheTime: 0);
    }
    
    // обработка результата встроенного запроса
    private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient,
        ChosenInlineResult chosenInlineResult)
    {
        Console.WriteLine($"Результат встроенного запроса: {chosenInlineResult.ResultId}");
        return Task.CompletedTask;
    }

    // Обработка неизвестных команд
    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Console.WriteLine($"Неизвестная команда: {update.Type}");
        return Task.CompletedTask;
    }
}