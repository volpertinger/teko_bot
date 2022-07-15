using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

// ReSharper disable All

namespace teko_bot;

// Содержит id и имя компании, чтобы было, чему выставлять счета
public class Company
{
    public int CompanyId { get; set; }
    public string Name { get; set; }

    public List<Bill> Bills { get; set; } = new();

    public static async void AddToDb(string name)
    {
        var db = BotConfiguration.Db;
        db.Add(new Company { Name = name });
        await db.SaveChangesAsync();
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
        var result = await db.Companies.Skip((page - 1) * BotConfiguration.PageSize).Take(BotConfiguration.PageSize)
            .ToListAsync();
        return result;
    }

    public static async Task<int> getId(int id)
    {
        var db = BotConfiguration.Db;
        var company = await db.Companies.FindAsync(id);
        return company is null ? 0 : id;
    }

    public override string ToString()
    {
        string result = "";
        result += "Id компании: " + CompanyId.ToString() + ", Название: " + Name.ToString();
        return result;
    }
}

// Содержит все данные о счетах: связь Company - Bill: 1 - *
public class Bill
{
    public int BillId { get; set; }
    public string? Description { get; set; }
    public string Email { get; set; }
    public string Date { get; set; }

    // Связь 1 - много: Company - Bill
    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    public static async void addToDb(string? description, string email, int companyId)
    {
        var db = BotConfiguration.Db;
        // чтобы не было ошибки при добавлении счета к несуществующей компании
        var company = await db.Companies.FindAsync(companyId);
        if (company is null)
        {
            return;
        }

        db.Add(new Bill
        {
            Description = description, Email = email, Date = DateTime.Now.ToString(CultureInfo.InvariantCulture),
            CompanyId = companyId
        });
        await db.SaveChangesAsync();
    }
}

// Содержит данные о пользователях, которые используют бота и состояних каждого пользователя
public class User
{
    // Username будет ключом
    [Key] public string Username { get; set; }

    // сотсояние бота для конкретного пользователя
    public States State { get; set; }

    // клмпания, в которую выполнен пользователеем вход (0 - никуда вход не выполнен)
    public int CurrentCompanyId { get; set; }

    // текущая страница при просмотре ланных с БД (0 - не просматривает данные)
    public int CurrentPage { get; set; }

    // проверяет, что пользователь с Username существует, иначе - добавляет его в базу
    public static async void UserCheck(string username)
    {
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        if (user is not null) return;
        db.Users.Add(new User { Username = username, State = States.Default, CurrentCompanyId = 0 });
        await db.SaveChangesAsync();
    }

    public static async Task<States> GetState(string? username)
    {
        if (username is null)
            return States.Default;
        UserCheck(username);
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        return user.State;
    }

    public static async void SetState(string? username, States state)
    {
        if (username is null)
            return;
        UserCheck(username);
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        user.State = state;
        await db.SaveChangesAsync();
    }

    public static async Task<int> GetPage(string? username)
    {
        if (username is null)
            return 0;
        UserCheck(username);
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        return user.CurrentPage;
    }

    public static async void SetPage(string? username, int page)
    {
        if (username is null)
            return;
        UserCheck(username);
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        user.CurrentPage = page;
        await db.SaveChangesAsync();
    }

    public static async Task<int> GetCurrentCompanyId(string? username)
    {
        if (username is null)
            return 0;
        UserCheck(username);
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        return user.CurrentCompanyId;
    }

    public static async void SetCurrentCompanyId(string? username, int newId)
    {
        if (username is null)
            return;
        UserCheck(username);
        var db = BotConfiguration.Db;
        var user = await db.Users.FindAsync(username);
        user.CurrentCompanyId = newId;
        await db.SaveChangesAsync();
    }
}

public sealed class ApplicationContext : DbContext
{
    readonly StreamWriter logStream = new StreamWriter(BotConfiguration.DbLogPath, true);
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<User> Users => Set<User>();

    public ApplicationContext()
    {
        // TODO: убрать очистку
        // очистка для тестирования
        // Database.EnsureDeleted();
        // гарантируем, что БД создана и если нет, то создаем
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={BotConfiguration.DbSource}");
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