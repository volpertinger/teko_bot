// ReSharper disable StringLiteralTypo

namespace teko_bot;

using Telegram.Bot.Types.ReplyMarkups;
using System.Text.Json;

// чтобы адекватно считывать настройки с json
public class RootConfigurationJson
{
    public BotConfigurationJson BotConfiguration { get; set; }
    public CommandsJson Commands { get; set; }

    public AnswersJson Answers { get; set; }
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

public class AnswersJson
{
    public string Usage { get; set; }
    public string CompanyAddSuccess { get; set; }
    public string CompanyAddUnSuccess { get; set; }
    public string CompanyAddInstruction { get; set; }
    public string CompanyLogInInstruction { get; set; }
    public string CompanyLogInSuccess { get; set; }
    public string CompanyLogInUnSuccess { get; set; }
    public string WrongState { get; set; }
    public string ClearText { get; set; }
    public string Back { get; set; }
    public string OutOfPagesRange { get; set; }
    public string EmptyList { get; set; }
    public string BillCreateHelp { get; set; }
    public string BillCreateSumHelp { get; set; }
    public string BillCreateEmailHelp { get; set; }
    public string BillCreateDiscHelp { get; set; }
    public string BillCreateUnSuccess { get; set; }
    public string BillCreateSuccess { get; set; }
    public string BillCancel { get; set; }
    public string GetSum { get; set; }
}

public class BotConfiguration
{
    public readonly string BotToken;
    public readonly string BotName;
    public readonly string DbSource;
    public readonly string DbLogPath;

    // размер страницы при выводе данных с БД
    public readonly int PageSize;

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
    public readonly string Usage;
    public readonly string Clear;
    public readonly string AddCompany;
    public readonly string LogInCompany;
    public readonly string CreateBill;
    public readonly string CheckBills;
    public readonly string GetSum;
    public readonly string GetCompanies;
    public readonly string Left;
    public readonly string Right;
    public readonly string Back;
    public readonly string Confirm;
    public readonly string Cancel;

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
public class Answers
{
    public readonly string Usage;
    public readonly string CompanyAddSuccess;
    public readonly string CompanyAddUnSuccess;
    public readonly string CompanyAddInstruction;
    public readonly string CompanyLogInInstruction;
    public readonly string CompanyLogInSuccess;
    public readonly string CompanyLogInUnSuccess;
    public readonly string WrongState;
    public readonly string ClearText;
    public readonly string Back;
    public readonly string OutOfPagesRange;
    public readonly string EmptyList;
    public readonly string BillCreateHelp;
    public readonly string BillCreateSumHelp;
    public readonly string BillCreateEmailHelp;
    public readonly string BillCreateDiscHelp;
    public readonly string BillCreateUnSuccess;
    public readonly string BillCreateSuccess;
    public readonly string BillCancel;
    public readonly string GetSum;

    public Answers()
    {
        var configFile = Program.ConfigFile;
        var jsonText = File.ReadAllText(configFile);
        var config = JsonSerializer.Deserialize<RootConfigurationJson>(jsonText)!;
        Usage = config.Answers.Usage;
        CompanyAddSuccess = config.Answers.CompanyAddSuccess;
        CompanyAddUnSuccess = config.Answers.CompanyAddUnSuccess;
        CompanyAddInstruction = config.Answers.CompanyAddInstruction;
        CompanyLogInInstruction = config.Answers.CompanyLogInInstruction;
        CompanyLogInSuccess = config.Answers.CompanyLogInSuccess;
        CompanyLogInUnSuccess = config.Answers.CompanyLogInUnSuccess;
        WrongState = config.Answers.WrongState;
        ClearText = config.Answers.ClearText;
        Back = config.Answers.Back;
        OutOfPagesRange = config.Answers.OutOfPagesRange;
        EmptyList = config.Answers.EmptyList;
        BillCreateHelp = config.Answers.BillCreateHelp;
        BillCreateSumHelp = config.Answers.BillCreateSumHelp;
        BillCreateEmailHelp = config.Answers.BillCreateEmailHelp;
        BillCreateDiscHelp = config.Answers.BillCreateDiscHelp;
        BillCreateUnSuccess = config.Answers.BillCreateUnSuccess;
        BillCreateSuccess = config.Answers.BillCreateSuccess;
        BillCancel = config.Answers.BillCancel;
        GetSum = config.Answers.GetSum;
    }
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