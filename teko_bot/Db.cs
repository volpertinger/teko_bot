using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

// ReSharper disable All

namespace teko_bot;

// Содержит id и имя компании, чтобы было, чему выставлять счета
public class Company
{
    public int CompanyId { get; set; }
    public string? Name { get; set; }

    public List<Bill> Bills { get; set; } = new();

    public static async Task<bool> AddToDb(string name)
    {
        var db = BotConfiguration.Db;
        if (db is null)
            return false;
        db.Add(new Company { Name = name });
        await db.SaveChangesAsync();
        return true;
    }

    public static async Task<int> GetAmount()
    {
        var db = BotConfiguration.Db;
        return await db.Companies.CountAsync();
    }

    public static async Task<List<Company>> GetCompanies(int page)
    {
        if (page < 1)
        {
            page = 1;
        }

        var db = BotConfiguration.Db;
        var result = await db.Companies.Skip((page - 1) * Program.BotConfiguration.PageSize)
            .Take(Program.BotConfiguration.PageSize)
            .ToListAsync();
        return result;
    }

    public static async Task<int> getId(int id)
    {
        var db = BotConfiguration.Db;
        var company = await db.Companies.FindAsync(id);
        return company is null ? 0 : id;
    }

    public static async Task<int> getSum(int id)
    {
        // просто обработка случая, если сейчас пользователь не в компании (но тогда он не сможет вызвать команды,
        //приводящие к вызову жтой функции, поэтому проверка это ради проверки
        if (id < 1)
            return 0;
        var db = BotConfiguration.Db;
        var company = await db.Companies.FindAsync(id);
        if (company is null)
            return 0;
        var bills = company.Bills;
        int result = 0;
        foreach (var bill in bills)
        {
            result += bill.Sum;
        }

        return result;
    }

    public override string ToString()
    {
        string result = "";
        result += "Id компании: " + CompanyId.ToString() + ", Название: " + Name?.ToString();
        return result;
    }
}

// Содержит черновик счета, чтобы каждый пользователь мог сначала ввести нужные данные, а потом уже все сохранить
// Связь 1 - 1 с User
public class BillDraft
{
    public int BillDraftId { get; set; }

    public string? Description { get; set; }

    public int Sum { get; set; }

    public string? Email { get; set; }

    // возвращает id только что созданного черновика, чтобы потом можно было привязать его к пользователю
    public static async Task<int> addToDb(int sum)
    {
        var db = BotConfiguration.Db;

        var draft = new BillDraft { Sum = sum };
        db.Add(draft);
        await db.SaveChangesAsync();
        return draft.BillDraftId;
    }

    // добавляет в черновик email
    public static async Task<bool> addEmail(int id, string email)
    {
        var db = BotConfiguration.Db;
        if (db is null)
            return false;
        var draft = await db.Drafts.FindAsync(id);
        if (draft is null)
            return false;
        draft.Email = email;
        await db.SaveChangesAsync();
        return true;
    }

    // добавляет в черновик описание
    public static async Task<bool> addDesc(int id, string desc)
    {
        var db = BotConfiguration.Db;
        if (db is null)
            return false;

        var draft = await db.Drafts.FindAsync(id);
        if (draft is null)
            return false;
        draft.Description = desc;
        await db.SaveChangesAsync();
        return true;
    }
}

// Содержит все данные о счетах: связь Company - Bill: 1 - *
public class Bill
{
    public int BillId { get; set; }
    public string? Description { get; set; }

    public int Sum { get; set; }
    public string? Email { get; set; }
    public string? Date { get; set; }

    // Связь 1 - много: Company - Bill
    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    public static async Task<bool> addToDb(string? description, string email, int companyId, int sum)
    {
        var db = BotConfiguration.Db;
        if (db is null)
            return false;
        // чтобы не было ошибки при добавлении счета к несуществующей компании
        var company = await db.Companies.FindAsync(companyId);
        if (company is null)
            return false;

        db.Add(new Bill
        {
            Description = description, Email = email, Date = DateTime.Now.ToString(CultureInfo.InvariantCulture),
            CompanyId = companyId, Sum = sum
        });
        await db.SaveChangesAsync();
        return true;
    }

    public static async Task<int> GetAmount()
    {
        var db = BotConfiguration.Db;
        return await db.Bills.CountAsync();
    }

    public static async Task<List<Bill>> GetBills(int page)
    {
        if (page < 1)
        {
            page = 1;
        }

        var db = BotConfiguration.Db;
        var result = await db.Bills.Skip((page - 1) * Program.BotConfiguration.PageSize)
            .Take(Program.BotConfiguration.PageSize)
            .ToListAsync();
        return result;
    }

    public override string ToString()
    {
        string result = "";
        result += "*) Id компании: " + CompanyId.ToString() + ", Сумма: " + Sum.ToString() + ", Email: " + Email +
                  "\nОписание: " + Description + "\nДата: " + Date?.ToString();
        return result;
    }
}

// Содержит данные о пользователях, которые используют бота и состояних каждого пользователя
public class User
{
    // Username будет ключом
    [Key] public string? Username { get; set; }

    // сотсояние бота для конкретного пользователя
    public States State { get; set; }

    // клмпания, в которую выполнен пользователеем вход (0 - никуда вход не выполнен)
    public int CurrentCompanyId { get; set; }

    // текущая страница при просмотре данных с БД (0 - не просматривает данные)
    public int CurrentPage { get; set; }

    // Текущий черновик счета, 0 - если черновика нет
    public int CurrentDraft { get; set; }

    // проверяет, что пользователь с Username существует, иначе - добавляет его в базу
    public static async Task<bool> UserCheck(string username)
    {
        var db = BotConfiguration.Db;
        if (db is null)
            return false;
        var user = await db.Users.FindAsync(username);
        if (user is not null)
            return true;
        db.Users.Add(new User { Username = username, State = States.Default, CurrentCompanyId = 0 });
        await db.SaveChangesAsync();
        return true;
    }

    public static async Task<States> GetState(string? username)
    {
        if (username is null)
            return States.Default;
        await UserCheck(username);
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        if (user != null) return user.State;
        return States.Default;
    }

    public static async Task<bool> SetState(string? username, States state)
    {
        if (username is null)
            return false;
        await UserCheck(username);
        var db = BotConfiguration.Db;
        if (db is null)
            return false;
        var user = await db.Users.FindAsync(username);
        if (user is null)
            return false;
        user.State = state;
        await db.SaveChangesAsync();
        return true;
    }

    public static async Task<int> GetPage(string? username)
    {
        if (username is null)
            return 0;
        await UserCheck(username);
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        if (user is null)
            return 1;
        return user.CurrentPage;
    }

    public static async Task<bool> SetPage(string? username, int page)
    {
        if (username is null)
            return false;
        await UserCheck(username);
        var db = BotConfiguration.Db;
        if (db is null)
            return false;
        var user = await db.Users.FindAsync(username);
        if (user is null)
            return false;
        user.CurrentPage = page;
        await db.SaveChangesAsync();
        return true;
    }

    public static async Task<int> GetDraft(string? username)
    {
        if (username is null)
            return 0;
        await UserCheck(username);
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        if (user is null)
            return -1;
        return user.CurrentDraft;
    }

    public static async Task<bool> SetDraft(string? username, int draftId)
    {
        if (username is null)
            return false;
        await UserCheck(username);
        var db = BotConfiguration.Db;
        if (db is null)
            return false;
        var user = await db.Users.FindAsync(username);
        if (user is null)
            return false;
        user.CurrentDraft = draftId;
        await db.SaveChangesAsync();
        return true;
    }

    public static async Task<int> GetCurrentCompanyId(string? username)
    {
        if (username is null)
            return 0;
        await UserCheck(username);
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        if (user is null)
            return -1;
        return user.CurrentCompanyId;
    }

    public static async Task<bool> SetCurrentCompanyId(string? username, int newId)
    {
        if (username is null)
            return false;
        await UserCheck(username);
        var db = BotConfiguration.Db;
        if (db is null)
            return false;
        var user = await db.Users.FindAsync(username);
        if (user is null)
            return false;
        user.CurrentCompanyId = newId;
        await db.SaveChangesAsync();
        return true;
    }

    // удаляет черновик счета и создает запись в таблице счетов
    public static async Task<bool> CreateBill(string? username)
    {
        if (username is null)
            return false;
        await UserCheck(username);
        var db = BotConfiguration.Db;
        if (db is null)
            return false;
        var user = await db.Users.FindAsync(username);
        if (user is null)
            return false;
        var draft = await db.Drafts.FindAsync(user.CurrentDraft);
        if (draft is null)
            return false;
        if (draft.Email is null)
            return false;
        await Bill.addToDb(draft.Description, draft.Email, user.CurrentCompanyId, draft.Sum);
        db.Drafts.Remove(draft);
        user.CurrentDraft = 0;
        await db.SaveChangesAsync();
        return true;
    }
}

public sealed class ApplicationContext : DbContext
{
    readonly StreamWriter logStream = new StreamWriter(Program.BotConfiguration.DbLogPath, true);

    //readonly StreamWriter logStream = new StreamWriter("/f.txt", true);
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<User> Users => Set<User>();
    public DbSet<BillDraft> Drafts => Set<BillDraft>();

    public ApplicationContext()
    {
        // гарантируем, что БД создана и если нет, то создаем
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={Program.BotConfiguration.DbSource}");
        //optionsBuilder.UseSqlite($"Data Source={"teko_bot.db"}");
        optionsBuilder.LogTo(logStream.WriteLine);
    }

    public override void Dispose()
    {
        base.Dispose();
        logStream.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await logStream.DisposeAsync();
    }
}