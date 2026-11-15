namespace MyApp.Data.Models;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsPrivate { get; set; }
    public int MaxPlayers { get; set; }
    public int BuyIn { get; set; }

    public int? WinnerPlayerId { get; set; }
    public Player? WinnerPlayer { get; set; }

    public List<Registration> Registrations { get; set; } = new();

    public override string ToString()
    {
        return $"{Name} - начало : {StartDate}, взнос : {BuyIn} руб.";
    }
}
