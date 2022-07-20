// ReSharper disable StringLiteralTypo

namespace teko_bot;

using Telegram.Bot.Types.ReplyMarkups;
using System.Text.Json;

// чтобы адекватно считывать настройки с json
public class RootConfigurationJson
{
    public BotConfigurationJson BotConfiguration { get; set; }
    public CommandsJson Commands { get; set; }
}

public class BotConfigurationJson
{
    public string BotToken { get; set; }
    public string BotName { get; set; }
    public string DbSource { get; set; }
    public string DbLogPath { get; set; }

    public int PageSize { get; set; }
}

public class CommandsJson
{
    public string Usage { get; set; }
    public string Clear { get; set; }
    public string AddCompany { get; set; }
    public string LogInCompany { get; set; }

    public string CreateBill { get; set; }
    public string CheckBills { get; set; }
    public string GetSum { get; set; }
    public string GetCompanies { get; set; }
    public string Left { get; set; }
    public string Right { get; set; }
    public string Back { get; set; }
    public string Confirm { get; set; }
    public string Cancel { get; set; }
}

public class BotConfiguration
{
    public string BotToken { get; set; }
    public string BotName { get; set; }
    public string DbSource { get; set; }

    public string DbLogPath { get; set; }

    // размер страницы при выводе данных с БД
    public int PageSize { get; set; }

    public static readonly ApplicationContext Db = new ApplicationContext();

    // каждому состоянию в соответствие ставится клавиатура
    public static readonly Dictionary<States, ReplyKeyboardMarkup> StatesKeyboards =
        new Dictionary<States, ReplyKeyboardMarkup>()
        {
            { States.Default, Keyboards.StartKeyboard },
            { States.InCompany, Keyboards.InCompanyKeyboard },
            { States.CheckCompanies, Keyboards.CheckDbListsKeyboard },
            { States.CheckBills, Keyboards.CheckDbListsKeyboard },
            { States.BillCreate, Keyboards.BillCreateKeyboard },
            { States.BillSum, Keyboards.BillCreateKeyboard },
            { States.BillEmail, Keyboards.BillCreateKeyboard },
            { States.BillDescription, Keyboards.BillConfirmKeyboard },
        };

    public BotConfiguration()
    {
        var configFile = Program.ConfigFile;
        var jsonText = File.ReadAllText(configFile);
        var config = JsonSerializer.Deserialize<RootConfigurationJson>(jsonText)!;
        BotToken = config.BotConfiguration.BotToken;
        BotName = config.BotConfiguration.BotName;
        DbSource = config.BotConfiguration.DbSource;
        DbLogPath = config.BotConfiguration.DbLogPath;
        PageSize = config.BotConfiguration.PageSize;
    }
}

// Команды, на которые реагирует бот
public class Commands
{
    public string Usage { get; set; }
    public string Clear { get; set; }
    public string AddCompany { get; set; }
    public string LogInCompany { get; set; }

    public string CreateBill { get; set; }
    public string CheckBills { get; set; }
    public string GetSum { get; set; }
    public string GetCompanies { get; set; }
    public string Left { get; set; }
    public string Right { get; set; }
    public string Back { get; set; }
    public string Confirm { get; set; }
    public string Cancel { get; set; }

    public Commands()
    {
        var configFile = Program.ConfigFile;
        var jsonText = File.ReadAllText(configFile);
        var config = JsonSerializer.Deserialize<RootConfigurationJson>(jsonText)!;
        Usage = config.Commands.Usage;
        Clear = config.Commands.Clear;
        AddCompany = config.Commands.AddCompany;
        LogInCompany = config.Commands.LogInCompany;
        CreateBill = config.Commands.CreateBill;
        CheckBills = config.Commands.CheckBills;
        GetSum = config.Commands.GetSum;
        GetCompanies = config.Commands.GetCompanies;
        Left = config.Commands.Left;
        Right = config.Commands.Right;
        Back = config.Commands.Back;
        Confirm = config.Commands.Confirm;
        Cancel = config.Commands.Cancel;
    }
}

// Текстовые ответы, которыми бот делится
public static class Answers
{
    public const string Usage = "Использование:\n" +
                                "Введите id компании. Если она не существует, то ее можно добавить." +
                                "Если она существует, то можно выполнить следующие действия:\n" +
                                "1) создать счёт (сумма, описание счёта, email для отправки счёта)\n" +
                                "2) посмотреть последние операции\n" +
                                "3) получить информацию по общей сумме платежей\n";

    public const string CompanyAddSuccess = "Компания успешно добавлена\n";
    public const string CompanyAddUnSuccess = "Что - то пошло не так, компания не добавлена\n";
    public const string CompanyAddInstruction = "Введите название компании\n";
    public const string CompanyLogInInstruction = "Введите id компании\n";
    public const string CompanyLogInSuccess = "Вы успешно вошли в компанию\n";
    public const string CompanyLogInUnSuccess = "Что - то пошло не так, вход не выполнен\n";
    public const string WrongState = "Не с правильного места вызвал ты комманду, хитрец\n";
    public const string ClearText = "Начнём сначала\n";
    public const string WrongCommand = "Я тебя не понимаю(\n";
    public const string Back = "Вернемся назад\n";
    public const string OutOfPagesRange = "В той стороне нет больше данных\n";
    public const string EmptyList = "Нет здесь ничего, нужно сначала хоть что - то добавить\n";
    public const string BillCreateHelp = "Сейчас будет создан счет компании\nВведите сумму счета\n";
    public const string BillCreateSumHelp = "Введена сумма\nВведите email\n";
    public const string BillCreateEmailHelp = "Введен email\nВведите описание\n";
    public const string BillCreateDiscHelp = "Введено описание\nПодтвердите создание счета\n";
    public const string BillCreateUnSuccess = "Произошла ошибка при создании счета, попробуйте снова\n";
    public const string BillCreateSuccess = "Счёт создан\n";
    public const string BillCancel = "Создание счета отменено\n";
    public const string GetSum = "Сумма платежей по данной комании: \n";
}

// Различные клавиатуры для разных остояний
public static class Keyboards
{
    public static readonly ReplyKeyboardMarkup StartKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Program.Commands.LogInCompany, Program.Commands.AddCompany },
            new KeyboardButton[] { Program.Commands.GetCompanies }
        })
    {
        ResizeKeyboard = true
    };

    public static readonly ReplyKeyboardMarkup InCompanyKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Program.Commands.CreateBill },
            new KeyboardButton[] { Program.Commands.CheckBills },
            new KeyboardButton[] { Program.Commands.GetSum },
        })
    {
        ResizeKeyboard = true
    };

    public static readonly ReplyKeyboardMarkup CheckDbListsKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Program.Commands.Left, Program.Commands.Right },
            new KeyboardButton[] { Program.Commands.Back }
        })
    {
        ResizeKeyboard = true
    };

    public static readonly ReplyKeyboardMarkup BillCreateKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Program.Commands.Cancel }
        })
    {
        ResizeKeyboard = true
    };

    public static readonly ReplyKeyboardMarkup BillConfirmKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Program.Commands.Confirm },
            new KeyboardButton[] { Program.Commands.Cancel }
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
    CheckCompanies,
    BillCreate,
    BillSum,
    BillEmail,
    BillDescription,
    CheckBills,
}