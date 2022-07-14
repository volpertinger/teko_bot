using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace teko_bot;

public class Company
{
    public int CompanyId { get; set; }
    public string Name;

    public List<Bill> Bills { get; set; } = new();

    public static async void addToDb(string name)
    {
        var db = BotConfiguration.Db;
        db.Add(new Company { Name = name });
        await db.SaveChangesAsync();
    }

    public static async Task<int> getId(int id)
    {
        var db = BotConfiguration.Db;
        var company = await db.Companies.FindAsync(id);
        return company is null ? 0 : id;
    }
}

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

public sealed class ApplicationContext : DbContext
{
    readonly StreamWriter logStream = new StreamWriter(BotConfiguration.DbLogPath, true);
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Bill> Bills => Set<Bill>();

    public ApplicationContext()
    {
        // TODO: убрать очистку
        // очистка для тестирования
        Database.EnsureDeleted();
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