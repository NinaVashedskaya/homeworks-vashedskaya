namespace MyApp.Data.Models;

public class Registration
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public DateTime RegistrationDate { get; set; }
    public int? FinalPlace { get; set; }
    public int? PrizeMoney { get; set; }

    public override string ToString()
    {
        return $"{Player.Nickname} - {Tournament.Name}, место : {FinalPlace}, выигрыш : {PrizeMoney} руб.";
    }
}
