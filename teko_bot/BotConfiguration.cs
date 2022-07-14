// ReSharper disable StringLiteralTypo

namespace teko_bot;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

public static class BotConfiguration
{
    public const string BotToken = "5446428708:AAEOFR_1BHWh0G6W8yCNzDgJkU7tSfBgS3g";
    public const string BotName = "teko_test_bot";
    public const string DbSource = "teko_bot.db";
    public const string DbLogPath = "DbLog.txt";
    public static readonly ApplicationContext Db = new ApplicationContext();
    public static States State = States.Default;
    public static int CurrentCompanyId = 0;

    // каждому состоянию в соответствие ставится клавиатура
    public static readonly Dictionary<States, ReplyKeyboardMarkup> StatesKeyboards =
        new Dictionary<States, ReplyKeyboardMarkup>()
        {
            { States.Default, Keyboards.StartKeyboard },
            { States.InCompany, Keyboards.InCompanyKeyboard },
        };
}

// Команды, на которые реагирует бот
public static class Commands
{
    public const string Usage = "/help";
    public const string Clear = "/clear";
    public const string AddCompany = "Добавить компанию";
    public const string LogInCompany = "Войти по id компании";
    public const string CreateBill = "Создать счёт";
    public const string CheckLastOperations = "Посмотреть последние операции";
    public const string GetSum = "Получить всю сумму за последние дни";
}

// Текстовые ответы, которыми бот делится
public static class Answers
{
    public const string Usage = "Использование:\n" +
                                "Введите id компании. Если она не существует, то ее можно добавить." +
                                "Если она существует, то можно выполнить следующие действия:\n" +
                                "1) создать счёт (сумма, описание счёта, email для отправки счёта)\n" +
                                "2) посмотреть последние 10 операций\n" +
                                "3) получить информацию по общей сумме платежей за день\n";

    public const string CompanyAddSuccess = "Компания успешно добавлена\n";
    public const string CompanyAddUnSuccess = "Что - то пошло не так, компания не добавлена\n";
    public const string CompanyAddInstruction = "Введите название компании\n";
    public const string CompanyLogInInstruction = "Введите id компании\n";
    public const string CompanyLogInSuccess = "Вы успешно вошли в компанию\n";
    public const string CompanyLogInUnSuccess = "Что - то пошло не так, вход не выполнен\n";
    public const string WrongState = "Не с правильного места вызвал ты комманду, хитрец\n";
    public const string ClearText = "Начнём сначала\n";
    public const string WrongCommand = "Я тебя не понимаю(\n";
}

// Различные клавиатуры для разных остояний
public static class Keyboards
{
    public static readonly ReplyKeyboardMarkup StartKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Commands.LogInCompany, Commands.AddCompany }
        })
    {
        ResizeKeyboard = true
    };

    public static readonly ReplyKeyboardMarkup InCompanyKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Commands.CreateBill },
            new KeyboardButton[] { Commands.CheckLastOperations },
            new KeyboardButton[] { Commands.GetSum },
        })
    {
        ResizeKeyboard = true
    };
}

// Перечисления состояний, в которых может находиться бот
public enum States
{
    Default,
    InCompany,
    AddingCompany,
    LogInCompany,
}