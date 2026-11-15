using MyApp.Data.Models;

namespace MyApp.Data;

public static class DbSeed
{
    public static void Seed(AppDbContext db)
    {
        if (db.Players.Any())
            return;

        var players = new List<Player>
        {
            new Player
            {
                Nickname = "ytghblevfkf",
                FullName = "Цаголова Ксения Тимуровна",
                PhoneNumber = "+79991112233",
                BirthDate = new DateTime(2000, 5, 12),
                RegistrationDate = DateTime.Now,
                IsVip = true
            },
            new Player
            {
                Nickname = "jgznmytghblevfkf",
                FullName = "Центрова Алина Игоревна",
                PhoneNumber = "+79992223344",
                BirthDate = new DateTime(1998, 8, 20),
                RegistrationDate = DateTime.Now,
                IsVip = false
            },
            new Player
            {
                Nickname = "qwerty",
                FullName = "Тумасов Алексей Иванович",
                PhoneNumber = "+79993334455",
                BirthDate = new DateTime(2001, 3, 15),
                RegistrationDate = DateTime.Now,
                IsVip = false
            }
        };

        db.Players.AddRange(players);
        db.SaveChanges();

        var tournaments = new List<Tournament>
        {
            new Tournament
            {
                Name = "Friday Poker",
                StartDate = new DateTime(2026, 11, 20, 18, 0, 0),
                EndDate = new DateTime(2026, 11, 20, 23, 0, 0),
                IsPrivate = false,
                MaxPlayers = 20,
                BuyIn = 3000,
                WinnerPlayerId = 1
            },
            new Tournament
            {
                Name = "Night Tournament",
                StartDate = new DateTime(2026, 11, 25, 20, 0, 0),
                IsPrivate = true,
                MaxPlayers = 12,
                BuyIn = 5000
            },
            new Tournament
            {
                Name = "Weekend Poker",
                StartDate = new DateTime(2026, 11, 28, 16, 0, 0),
                IsPrivate = false,
                MaxPlayers = 30,
                BuyIn = 2000
            }
        };

        db.Tournaments.AddRange(tournaments);
        db.SaveChanges();

        var registrations = new List<Registration>
        {
            new Registration
            {
                PlayerId = 1,
                TournamentId = 1,
                RegistrationDate = DateTime.Now,
                FinalPlace = 1,
                PrizeMoney = 15000
            },
            new Registration
            {
                PlayerId = 2,
                TournamentId = 1,
                RegistrationDate = DateTime.Now,
                FinalPlace = 2,
                PrizeMoney = 7000
            },
            new Registration
            {
                PlayerId = 3,
                TournamentId = 1,
                RegistrationDate = DateTime.Now,
                FinalPlace = 3,
                PrizeMoney = 3000
            },
            new Registration
            {
                PlayerId = 1,
                TournamentId = 2,
                RegistrationDate = DateTime.Now
            },
            new Registration
            {
                PlayerId = 2,
                TournamentId = 3,
                RegistrationDate = DateTime.Now
            }
        };

        db.Registrations.AddRange(registrations);
        db.SaveChanges();
    }
}
