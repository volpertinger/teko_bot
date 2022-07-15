// ReSharper disable StringLiteralTypo

namespace teko_bot;

using Telegram.Bot.Types.ReplyMarkups;

public static class BotConfiguration
{
    public const string BotToken = "5446428708:AAEOFR_1BHWh0G6W8yCNzDgJkU7tSfBgS3g";
    public const string BotName = "teko_test_bot";
    public const string DbSource = "teko_bot.db";
    public const string DbLogPath = "DbLog.txt";

    public static readonly ApplicationContext Db = new ApplicationContext();

    // размер страницы при выводе данных с БД
    public const int PageSize = 10;

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
}

// Команды, на которые реагирует бот
public static class Commands
{
    public const string Usage = "/help";
    public const string Clear = "/clear";
    public const string AddCompany = "Добавить компанию";
    public const string LogInCompany = "Войти по id компании";
    public const string CreateBill = "Создать счёт";
    public const string CheckBills = "Посмотреть последние операции";
    public const string GetSum = "Получить всю сумму платежей";
    public const string GetCompanies = "Посмотреть зарегистрированные компании";
    public const string Left = "Влево";
    public const string Right = "Вправо";
    public const string Back = "Назад";
    public const string Confirm = "Подтвердить";
    public const string Cancel = "Отменить";
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
            new KeyboardButton[] { Commands.LogInCompany, Commands.AddCompany },
            new KeyboardButton[] { Commands.GetCompanies }
        })
    {
        ResizeKeyboard = true
    };

    public static readonly ReplyKeyboardMarkup InCompanyKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Commands.CreateBill },
            new KeyboardButton[] { Commands.CheckBills },
            new KeyboardButton[] { Commands.GetSum },
        })
    {
        ResizeKeyboard = true
    };

    public static readonly ReplyKeyboardMarkup CheckDbListsKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Commands.Left, Commands.Right },
            new KeyboardButton[] { Commands.Back }
        })
    {
        ResizeKeyboard = true
    };

    public static readonly ReplyKeyboardMarkup BillCreateKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Commands.Cancel }
        })
    {
        ResizeKeyboard = true
    };

    public static readonly ReplyKeyboardMarkup BillConfirmKeyboard = new(
        new[]
        {
            new KeyboardButton[] { Commands.Confirm },
            new KeyboardButton[] { Commands.Cancel }
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