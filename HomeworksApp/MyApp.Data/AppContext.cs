using Microsoft.EntityFrameworkCore;
using MyApp.Data.Models;

namespace MyApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<Registration> Registrations { get; set; }

    public static string DbPath
    {
        get
        {
            var folder =
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "komnata.db");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}
