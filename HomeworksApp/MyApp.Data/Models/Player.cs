namespace MyApp.Data.Models;

public class Player
{
    public int Id { get; set; }
    public string Nickname { get; set; } = "";
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public DateTime  BirthDate { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsVip { get; set; }

    public List<Registration> Registrations { get; set; } = new();
    public List<Tournament> WonTournaments { get; set; } = new();
    
    public override string ToString()
    {
        return $"{Nickname} - {FullName}, телефон : {PhoneNumber}, дата рождения : {BirthDate}";
    }
}
