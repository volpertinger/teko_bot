using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

// ReSharper disable StringLiteralTypo

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
        Console.WriteLine($"Тип полученного сообщения: {message.Type}\n" +
                          $"Содержание: {message.Text}");
        var messageText = message.Text;
        if (messageText is null)
            return;

        var action = messageText switch
        {
            Commands.Usage => Usage(botClient, message),
            Commands.Clear => Clear(botClient, message),
            Commands.AddCompany => AddCompany(botClient, message),
            Commands.LogInCompany => LogInCompany(botClient, message),
            _ => DefaultCase(botClient, message)
        };
        var sentMessage = await action;
        Console.WriteLine($"Сообщение было отправлено с id: {sentMessage.MessageId}");
    }

    // сообщение - справка об использовании бота 
    private static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: Answers.Usage,
            replyMarkup: GetKeyboard(message));
    }

    // обработка добавления компании
    private static async Task<Message> AddCompany(ITelegramBotClient botClient, Message message)
    {
        if (User.GetState(message.Chat.Username) != States.Default)
        {
            return await WrongStateProcessing(botClient, message);
        }

        //BotConfiguration.State = States.AddingCompany;
        User.SetState(message.Chat.Username, States.AddingCompany);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: Answers.CompanyAddInstruction,
            replyMarkup: new ReplyKeyboardRemove());
    }

    // обработка входа по id компании
    private static async Task<Message> LogInCompany(ITelegramBotClient botClient, Message message)
    {
        if (User.GetState(message.Chat.Username) != States.Default)
        {
            return await WrongStateProcessing(botClient, message);
        }

        //BotConfiguration.State = States.LogInCompany;
        User.SetState(message.Chat.Username, States.LogInCompany);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: Answers.CompanyLogInInstruction,
            replyMarkup: new ReplyKeyboardRemove());
    }

    // обработка случая с несуществующей командой
    private static async Task<Message> WrongCommandProcessing(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Я тебя не понимаю(",
            replyMarkup: GetKeyboard(message));
    }

    // обработка комманд, когда выполнен вход в компанию
    private static async Task<Message> InCompanyProcessing(ITelegramBotClient botClient, Message message)
    {
        //Console.WriteLine(message.Chat.Username);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: "Ты в компании",
            replyMarkup: GetKeyboard(message));
    }

    // Отправка сообщения при правильной команде, но не в том состоянии
    private static async Task<Message> WrongStateProcessing(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: Answers.WrongState,
            replyMarkup: GetKeyboard(message));
    }

    // Обраотка при добавлении компании
    private static async Task<Message> AddCompanyProcessing(ITelegramBotClient botClient, Message message)
    {
        //BotConfiguration.State = States.Default;
        User.SetState(message.Chat.Username, States.Default);
        if (message.Text is null)
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: Answers.CompanyAddUnSuccess,
                replyMarkup: GetKeyboard(message));
        Company.AddToDb(message.Text);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: Answers.CompanyAddSuccess,
            replyMarkup: GetKeyboard(message));
    }

    // Обработка неудач при входе в компанию
    private static async Task<Message> LogInCompanyUnSuccessProcessing(ITelegramBotClient botClient, Message message)
    {
        //BotConfiguration.State = States.Default;
        User.SetState(message.Chat.Username, States.Default);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: Answers.CompanyLogInUnSuccess,
            replyMarkup: GetKeyboard(message));
    }

    // Обработка процесса входа в компанию
    private static async Task<Message> LogInCompanyProcessing(ITelegramBotClient botClient, Message message)
    {
        if (message.Text is null)
            return await LogInCompanyUnSuccessProcessing(botClient, message);

        //BotConfiguration.CurrentCompanyId = await Company.getId(int.Parse(message.Text));
        User.SetCurrentCompanyId(message.Chat.Username, await Company.getId(int.Parse(message.Text)));
        if (User.GetCurrentCompanyId(message.Chat.Username) == 0)
            return await LogInCompanyUnSuccessProcessing(botClient, message);

        //BotConfiguration.State = States.InCompany;
        User.SetState(message.Chat.Username, States.InCompany);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: Answers.CompanyLogInSuccess,
            replyMarkup: GetKeyboard(message));
    }

    // Обработка сообщений, не являющимися командами
    private static async Task<Message> DefaultCase(ITelegramBotClient botClient, Message message)
    {
        switch (User.GetState(message.Chat.Username))
        {
            case States.Default:
                return await WrongCommandProcessing(botClient, message);
            case States.InCompany:
                return await InCompanyProcessing(botClient, message);
            case States.AddingCompany:
                return await AddCompanyProcessing(botClient, message);
            case States.LogInCompany:
                return await LogInCompanyProcessing(botClient, message);
            default:
                return await WrongCommandProcessing(botClient, message);
        }
    }

    // Обработка возврата в начало
    private static async Task<Message> Clear(ITelegramBotClient botClient, Message message)
    {
        //BotConfiguration.State = States.Default;
        User.SetState(message.Chat.Username, States.Default);
        User.SetCurrentCompanyId(message.Chat.Username, 0);
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            text: Answers.ClearText,
            replyMarkup: GetKeyboard(message));
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
        // ReSharper disable once StringLiteralTypo
        Console.WriteLine($"Результат встроенного запроса: {chosenInlineResult.ResultId}\n id бота: {botClient.BotId}");
        return Task.CompletedTask;
    }

    // Обработка неизвестных команд
    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Console.WriteLine($"Неизвестная команда: {update.Type}\n id бота: {botClient.BotId}");
        return Task.CompletedTask;
    }


    // получение текущей клавиатуры в зависимости от состояния
    private static ReplyKeyboardMarkup GetKeyboard(Message message)
    {
        return BotConfiguration.StatesKeyboards[User.GetState(message.Chat.Username)];
    }
}