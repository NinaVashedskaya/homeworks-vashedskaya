using Microsoft.EntityFrameworkCore;
using MyApp.Data;

using var db = new AppDbContext();

Console.WriteLine();
Console.WriteLine("ПОКЕРНЫЙ КЛУБ «КОМНАТА»");
Console.WriteLine();

Console.WriteLine($"путь к базе данных : {AppDbContext.DbPath}");

db.Database.EnsureCreated();

DbSeed.Seed(db);

Console.WriteLine();
Console.WriteLine("игроки");
Console.WriteLine();

foreach (var player in db.Players)
{
    Console.WriteLine(player);
}

Console.WriteLine();
Console.WriteLine("турниры");
Console.WriteLine();

foreach (var tournament in db.Tournaments
             .Include(t => t.WinnerPlayer))
{
    Console.WriteLine(tournament);

    if (tournament.WinnerPlayer != null)
        Console.WriteLine($"победитель : {tournament.WinnerPlayer.Nickname}");
}

Console.WriteLine();
Console.WriteLine("регистрации");
Console.WriteLine();

foreach (var registration in db.Registrations
             .Include(r => r.Player)
             .Include(r => r.Tournament))
{
    Console.WriteLine(registration);
}
