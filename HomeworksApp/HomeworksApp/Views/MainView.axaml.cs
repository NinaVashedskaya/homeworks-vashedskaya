using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;

namespace HomeworksApp.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();
            DbSeed.Seed(db);
        }
        
        DisplayPlayers();
        DisplayTournaments();
        DisplayRegistrations();
    }
    private void DisplayPlayers()
    {
        using var db = new AppDbContext();
        PlayersList.ItemsSource = db.Players
            .OrderBy(p => p.Nickname)
            .ToList();
    }
    private void DisplayTournaments()
    {
        using var db = new AppDbContext();
        TournamentsList.ItemsSource = db.Tournaments
            .Include(t=>t.WinnerPlayer)
            .OrderBy(t=>t.StartDate)
            .ToList();
    }
    private void DisplayRegistrations()
    {
        using var db = new AppDbContext();
        RegistrationsList.ItemsSource = db.Registrations
            .Include(r=>r.Player)
            .Include(r=>r.Tournament)
            .OrderByDescending(r=>r.RegistrationDate)
            .ToList();
    }
}
