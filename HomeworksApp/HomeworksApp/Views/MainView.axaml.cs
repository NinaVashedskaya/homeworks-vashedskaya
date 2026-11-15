using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Data.Models;

namespace HomeworksApp.Views;

public partial class MainView : UserControl
{
    private Player? _selectedPlayer;
    private Tournament? _selectedTournament;
    private Registration? _selectedRegistration;

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
            .Include(t => t.WinnerPlayer)
            .OrderBy(t => t.StartDate)
            .ToList();
    }

    private void DisplayRegistrations()
    {
        using var db = new AppDbContext();

        RegistrationsList.ItemsSource = db.Registrations
            .Include(r => r.Player)
            .Include(r => r.Tournament)
            .OrderByDescending(r => r.RegistrationDate)
            .ToList();
    }

    private static DateTime CombineDateAndTime(
        DateTimeOffset? date,
        TimeSpan? time)
    {
        return date!.Value.Date + (time ?? TimeSpan.Zero);
    }

    // ИГРОКИ 

    private void ShowPlayerError(string message)
    {
        PlayerEditError.Text = message;
        PlayerEditError.IsVisible = true;
    }

    private void HidePlayerError()
    {
        PlayerEditError.Text = "";
        PlayerEditError.IsVisible = false;
    }

    private bool ValidatePlayer(
        out string nickname,
        out string fullName,
        out string phone,
        out DateTime birthDate,
        out DateTime registrationDate)
    {
        nickname = (EditNicknameBox.Text ?? "").Trim();
        fullName = (EditFullNameBox.Text ?? "").Trim();
        phone = (EditPhoneBox.Text ?? "").Trim();

        birthDate = DateTime.MinValue;
        registrationDate = DateTime.MinValue;

        if (string.IsNullOrWhiteSpace(nickname))
        {
            ShowPlayerError("введите никнейм");
            return false;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ShowPlayerError("введите ФИО");
            return false;
        }

        if (!fullName.All(c =>
                char.IsLetter(c) || c == ' ' || c == '-' || c == '.'))
        {
            ShowPlayerError("проверьте ФИО");
            return false;
        }

        if (string.IsNullOrWhiteSpace(phone) ||
            !phone.All(c => char.IsDigit(c) || c == '+') ||
            phone.IndexOf('+') > 0 ||
            phone.Count(char.IsDigit) < 5)
        {
            ShowPlayerError("проверьте номер телефона");
            return false;
        }

        if (EditPlayerBirthDatePicker.SelectedDate is null)
        {
            ShowPlayerError("выберите дату рождения");
            return false;
        }

        if (EditPlayerRegistrationDatePicker.SelectedDate is null)
        {
            ShowPlayerError("выберите дату регистрации");
            return false;
        }

        birthDate = EditPlayerBirthDatePicker.SelectedDate.Value.Date;
        registrationDate =
            EditPlayerRegistrationDatePicker.SelectedDate.Value.Date;

        if (birthDate > DateTime.Today)
        {
            ShowPlayerError("дата рождения не может быть в будущем");
            return false;
        }

        if (registrationDate.Year < 2000 ||
            registrationDate > DateTime.Now.AddDays(1))
        {
            ShowPlayerError("проверьте дату регистрации");
            return false;
        }

        return true;
    }

    private void OnPlayerSelected(object? sender, SelectionChangedEventArgs e)
    {
        bool selected = PlayersList.SelectedItem is Player;

        PlayerActionsPanel.IsVisible = selected;

        if (!selected)
            PlayerEditPanel.IsVisible = false;
    }

    private void OnAddPlayerClick(object? sender, RoutedEventArgs e)
    {
        _selectedPlayer = null;

        PlayerEditTitle.Text = "добавить игрока";
        HidePlayerError();

        EditNicknameBox.Text = "";
        EditFullNameBox.Text = "";
        EditPhoneBox.Text = "";

        EditPlayerBirthDatePicker.SelectedDate = null;
        EditPlayerRegistrationDatePicker.SelectedDate = DateTime.Today;
        EditPlayerIsVipCheckBox.IsChecked = false;

        PlayerActionsPanel.IsVisible = false;
        PlayerEditPanel.IsVisible = true;
    }

    private void OnEditPlayerClick(object? sender, RoutedEventArgs e)
    {
        if (PlayersList.SelectedItem is not Player player)
            return;

        _selectedPlayer = player;

        PlayerEditTitle.Text = "редактировать игрока";
        HidePlayerError();

        EditNicknameBox.Text = player.Nickname;
        EditFullNameBox.Text = player.FullName;
        EditPhoneBox.Text = player.PhoneNumber;

        EditPlayerBirthDatePicker.SelectedDate = player.BirthDate;
        EditPlayerRegistrationDatePicker.SelectedDate =
            player.RegistrationDate;

        EditPlayerIsVipCheckBox.IsChecked = player.IsVip;

        PlayerEditPanel.IsVisible = true;
    }

    private void OnSavePlayerClick(object? sender, RoutedEventArgs e)
    {
        HidePlayerError();

        if (!ValidatePlayer(
                out var nickname,
                out var fullName,
                out var phone,
                out var birthDate,
                out var registrationDate))
            return;

        try
        {
            using var db = new AppDbContext();

            if (_selectedPlayer is null)
            {
                db.Players.Add(new Player
                {
                    Nickname = nickname,
                    FullName = fullName,
                    PhoneNumber = phone,
                    BirthDate = birthDate,
                    RegistrationDate = registrationDate,
                    IsVip = EditPlayerIsVipCheckBox.IsChecked == true
                });
            }
            else
            {
                var player = db.Players
                    .FirstOrDefault(p => p.Id == _selectedPlayer.Id);

                if (player is null)
                {
                    ShowPlayerError("игрок не найден");
                    return;
                }

                player.Nickname = nickname;
                player.FullName = fullName;
                player.PhoneNumber = phone;
                player.BirthDate = birthDate;
                player.RegistrationDate = registrationDate;
                player.IsVip = EditPlayerIsVipCheckBox.IsChecked == true;
            }

            db.SaveChanges();

            PlayerEditPanel.IsVisible = false;
            _selectedPlayer = null;
            PlayersList.SelectedItem = null;
            PlayerActionsPanel.IsVisible = false;

            DisplayPlayers();
            DisplayTournaments();
            DisplayRegistrations();
        }
        catch (Exception ex)
        {
            ShowPlayerError($"ошибка сохранения : {ex.Message}");
        }
    }

    private void OnCancelEditClick(object? sender, RoutedEventArgs e)
    {
        PlayerEditPanel.IsVisible = false;
        _selectedPlayer = null;
        HidePlayerError();
    }

    private void OnDeletePlayerClick(object? sender, RoutedEventArgs e)
    {
        if (PlayersList.SelectedItem is not Player selected)
            return;

        try
        {
            using var db = new AppDbContext();

            var player = db.Players
                .FirstOrDefault(p => p.Id == selected.Id);

            if (player is null)
                return;

            var tournaments = db.Tournaments
                .Where(t => t.WinnerPlayerId == player.Id)
                .ToList();

            foreach (var tournament in tournaments)
                tournament.WinnerPlayerId = null;

            var registrations = db.Registrations
                .Where(r => r.PlayerId == player.Id)
                .ToList();

            db.Registrations.RemoveRange(registrations);
            db.Players.Remove(player);

            db.SaveChanges();

            PlayersList.SelectedItem = null;
            PlayerActionsPanel.IsVisible = false;
            PlayerEditPanel.IsVisible = false;

            DisplayPlayers();
            DisplayTournaments();
            DisplayRegistrations();
        }
        catch (Exception ex)
        {
            ShowPlayerError($"ошибка удаления : {ex.Message}");
        }
    }

    // ТУРНИРЫ 

    private void ShowTournamentError(string message)
    {
        TournamentEditError.Text = message;
        TournamentEditError.IsVisible = true;
    }

    private void HideTournamentError()
    {
        TournamentEditError.Text = "";
        TournamentEditError.IsVisible = false;
    }

    private void LoadTournamentPlayers()
    {
        using var db = new AppDbContext();

        EditTournamentWinnerBox.ItemsSource = db.Players
            .OrderBy(p => p.Nickname)
            .ToList();
    }

    private bool ValidateTournament(
        out string name,
        out DateTime startDate,
        out DateTime? endDate,
        out int maxPlayers,
        out int buyIn)
    {
        name = (EditTournamentNameBox.Text ?? "").Trim();
        startDate = DateTime.MinValue;
        endDate = null;
        maxPlayers = 0;
        buyIn = 0;

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowTournamentError("введите название турнира");
            return false;
        }

        if (EditTournamentDatePicker.SelectedDate is null)
        {
            ShowTournamentError("выберите дату начала");
            return false;
        }

        startDate = CombineDateAndTime(
            EditTournamentDatePicker.SelectedDate,
            EditTournamentTimePicker.SelectedTime);

        if (EditTournamentEndDatePicker.SelectedDate is not null)
        {
            endDate = EditTournamentEndDatePicker.SelectedDate.Value.Date;

            if (endDate.Value < startDate.Date)
            {
                ShowTournamentError("дата окончания раньше даты начала");
                return false;
            }
        }

        if (!int.TryParse(EditTournamentMaxPlayersBox.Text, out maxPlayers) ||
            maxPlayers <= 0)
        {
            ShowTournamentError("максимум игроков должен быть положительным числом");
            return false;
        }

        if (!int.TryParse(EditTournamentBuyInBox.Text, out buyIn) ||
            buyIn < 0)
        {
            ShowTournamentError("бай-ин должен быть положительным числом");
            return false;
        }

        return true;
    }

    private void OnTournamentSelected(object? sender, SelectionChangedEventArgs e)
    {
        bool selected = TournamentsList.SelectedItem is Tournament;

        TournamentActionsPanel.IsVisible = selected;

        if (!selected)
            TournamentEditPanel.IsVisible = false;
    }

    private void OnAddTournamentClick(object? sender, RoutedEventArgs e)
    {
        _selectedTournament = null;

        TournamentEditTitle.Text = "добавить турнир";
        HideTournamentError();
        LoadTournamentPlayers();

        EditTournamentNameBox.Text = "";
        EditTournamentDatePicker.SelectedDate = DateTime.Today;
        EditTournamentTimePicker.SelectedTime = new TimeSpan(18, 0, 0);
        EditTournamentEndDatePicker.SelectedDate = null;
        EditTournamentMaxPlayersBox.Text = "16";
        EditTournamentBuyInBox.Text = "0";
        EditTournamentIsPrivateCheckBox.IsChecked = false;
        EditTournamentWinnerBox.SelectedItem = null;

        TournamentActionsPanel.IsVisible = false;
        TournamentEditPanel.IsVisible = true;
    }

    private void OnEditTournamentClick(object? sender, RoutedEventArgs e)
    {
        if (TournamentsList.SelectedItem is not Tournament tournament)
            return;

        _selectedTournament = tournament;

        TournamentEditTitle.Text = "редактировать турнир";
        HideTournamentError();
        LoadTournamentPlayers();

        EditTournamentNameBox.Text = tournament.Name;
        EditTournamentDatePicker.SelectedDate = tournament.StartDate;
        EditTournamentTimePicker.SelectedTime =
            tournament.StartDate.TimeOfDay;

        EditTournamentEndDatePicker.SelectedDate = tournament.EndDate;
        EditTournamentMaxPlayersBox.Text = tournament.MaxPlayers.ToString();
        EditTournamentBuyInBox.Text = tournament.BuyIn.ToString();
        EditTournamentIsPrivateCheckBox.IsChecked = tournament.IsPrivate;

        if (tournament.WinnerPlayerId is int winnerId)
        {
            var players = EditTournamentWinnerBox.ItemsSource
                ?.Cast<Player>()
                .ToList();

            EditTournamentWinnerBox.SelectedItem =
                players?.FirstOrDefault(p => p.Id == winnerId);
        }
        else
        {
            EditTournamentWinnerBox.SelectedItem = null;
        }

        TournamentEditPanel.IsVisible = true;
    }

    private void OnSaveTournamentClick(object? sender, RoutedEventArgs e)
    {
        HideTournamentError();

        if (!ValidateTournament(
                out var name,
                out var startDate,
                out var endDate,
                out var maxPlayers,
                out var buyIn))
            return;

        int? winnerId =
            (EditTournamentWinnerBox.SelectedItem as Player)?.Id;

        try
        {
            using var db = new AppDbContext();

            if (_selectedTournament is null)
            {
                db.Tournaments.Add(new Tournament
                {
                    Name = name,
                    StartDate = startDate,
                    EndDate = endDate,
                    MaxPlayers = maxPlayers,
                    BuyIn = buyIn,
                    IsPrivate = EditTournamentIsPrivateCheckBox.IsChecked == true,
                    WinnerPlayerId = winnerId
                });
            }
            else
            {
                var tournament = db.Tournaments
                    .FirstOrDefault(t => t.Id == _selectedTournament.Id);

                if (tournament is null)
                {
                    ShowTournamentError("турнир не найден");
                    return;
                }

                tournament.Name = name;
                tournament.StartDate = startDate;
                tournament.EndDate = endDate;
                tournament.MaxPlayers = maxPlayers;
                tournament.BuyIn = buyIn;
                tournament.IsPrivate =
                    EditTournamentIsPrivateCheckBox.IsChecked == true;
                tournament.WinnerPlayerId = winnerId;
            }

            db.SaveChanges();

            TournamentEditPanel.IsVisible = false;
            _selectedTournament = null;
            TournamentsList.SelectedItem = null;
            TournamentActionsPanel.IsVisible = false;

            DisplayTournaments();
            DisplayRegistrations();
        }
        catch (Exception ex)
        {
            ShowTournamentError($"ошибка сохранения : {ex.Message}");
        }
    }

    private void OnCancelTournamentEditClick(object? sender, RoutedEventArgs e)
    {
        TournamentEditPanel.IsVisible = false;
        _selectedTournament = null;
        HideTournamentError();
    }

    private void OnDeleteTournamentClick(object? sender, RoutedEventArgs e)
    {
        if (TournamentsList.SelectedItem is not Tournament selected)
            return;

        try
        {
            using var db = new AppDbContext();

            var tournament = db.Tournaments
                .FirstOrDefault(t => t.Id == selected.Id);

            if (tournament is null)
                return;

            var registrations = db.Registrations
                .Where(r => r.TournamentId == tournament.Id)
                .ToList();

            db.Registrations.RemoveRange(registrations);
            db.Tournaments.Remove(tournament);

            db.SaveChanges();

            TournamentsList.SelectedItem = null;
            TournamentActionsPanel.IsVisible = false;
            TournamentEditPanel.IsVisible = false;

            DisplayTournaments();
            DisplayRegistrations();
        }
        catch (Exception ex)
        {
            ShowTournamentError($"ошибка удаления : {ex.Message}");
        }
    }

    // РЕГИСТРАЦИИ  

    private void ShowRegistrationError(string message)
    {
        RegistrationEditError.Text = message;
        RegistrationEditError.IsVisible = true;
    }

    private void HideRegistrationError()
    {
        RegistrationEditError.Text = "";
        RegistrationEditError.IsVisible = false;
    }

    private void LoadRegistrationOptions()
    {
        using var db = new AppDbContext();

        EditRegistrationPlayerBox.ItemsSource = db.Players
            .OrderBy(p => p.Nickname)
            .ToList();

        EditRegistrationTournamentBox.ItemsSource = db.Tournaments
            .OrderBy(t => t.Name)
            .ToList();
    }

    private bool ValidateRegistration(
        out int playerId,
        out int tournamentId,
        out DateTime registrationDate,
        out int? finalPlace,
        out int? prizeMoney)
    {
        playerId = 0;
        tournamentId = 0;
        registrationDate = DateTime.MinValue;
        finalPlace = null;
        prizeMoney = null;

        if (EditRegistrationPlayerBox.SelectedItem is not Player player)
        {
            ShowRegistrationError("выберите игрока");
            return false;
        }

        if (EditRegistrationTournamentBox.SelectedItem is not Tournament tournament)
        {
            ShowRegistrationError("выберите турнир");
            return false;
        }

        if (EditRegistrationDatePicker.SelectedDate is null)
        {
            ShowRegistrationError("выберите дату регистрации");
            return false;
        }

        registrationDate = CombineDateAndTime(
            EditRegistrationDatePicker.SelectedDate,
            EditRegistrationTimePicker.SelectedTime);

        if (registrationDate.Year < 2000 ||
            registrationDate > DateTime.Now.AddDays(1))
        {
            ShowRegistrationError("проверьте дату регистрации");
            return false;
        }

        string placeText =
            (EditRegistrationFinalPlaceBox.Text ?? "").Trim();

        if (!string.IsNullOrWhiteSpace(placeText))
        {
            if (!int.TryParse(placeText, out int place) || place <= 0)
            {
                ShowRegistrationError("место должно быть положительным числом");
                return false;
            }

            finalPlace = place;
        }

        string prizeText =
            (EditRegistrationPrizeMoneyBox.Text ?? "").Trim();

        if (!string.IsNullOrWhiteSpace(prizeText))
        {
            if (!int.TryParse(prizeText, out int prize) || prize < 0)
            {
                ShowRegistrationError("выигрыш должен быть неотрицательным числом");
                return false;
            }

            prizeMoney = prize;
        }

        playerId = player.Id;
        tournamentId = tournament.Id;

        return true;
    }

    private void OnRegistrationSelected(object? sender, SelectionChangedEventArgs e)
    {
        bool selected = RegistrationsList.SelectedItem is Registration;

        RegistrationActionsPanel.IsVisible = selected;

        if (!selected)
            RegistrationEditPanel.IsVisible = false;
    }

    private void OnAddRegistrationClick(object? sender, RoutedEventArgs e)
    {
        _selectedRegistration = null;

        RegistrationEditTitle.Text = "добавить регистрацию";
        HideRegistrationError();
        LoadRegistrationOptions();

        EditRegistrationPlayerBox.SelectedItem = null;
        EditRegistrationTournamentBox.SelectedItem = null;

        EditRegistrationDatePicker.SelectedDate = DateTime.Today;
        EditRegistrationTimePicker.SelectedTime = DateTime.Now.TimeOfDay;

        EditRegistrationFinalPlaceBox.Text = "";
        EditRegistrationPrizeMoneyBox.Text = "";

        RegistrationActionsPanel.IsVisible = false;
        RegistrationEditPanel.IsVisible = true;
    }

    private void OnEditRegistrationClick(object? sender, RoutedEventArgs e)
    {
        if (RegistrationsList.SelectedItem is not Registration registration)
            return;

        _selectedRegistration = registration;

        RegistrationEditTitle.Text = "редактировать регистрацию";
        HideRegistrationError();
        LoadRegistrationOptions();

        var players = EditRegistrationPlayerBox.ItemsSource
            ?.Cast<Player>()
            .ToList();

        EditRegistrationPlayerBox.SelectedItem =
            players?.FirstOrDefault(p => p.Id == registration.PlayerId);

        var tournaments = EditRegistrationTournamentBox.ItemsSource
            ?.Cast<Tournament>()
            .ToList();

        EditRegistrationTournamentBox.SelectedItem =
            tournaments?.FirstOrDefault(t => t.Id == registration.TournamentId);

        EditRegistrationDatePicker.SelectedDate =
            registration.RegistrationDate;

        EditRegistrationTimePicker.SelectedTime =
            registration.RegistrationDate.TimeOfDay;

        EditRegistrationFinalPlaceBox.Text =
            registration.FinalPlace?.ToString() ?? "";

        EditRegistrationPrizeMoneyBox.Text =
            registration.PrizeMoney?.ToString() ?? "";

        RegistrationEditPanel.IsVisible = true;
    }

    private void OnSaveRegistrationClick(object? sender, RoutedEventArgs e)
    {
        HideRegistrationError();

        if (!ValidateRegistration(
                out var playerId,
                out var tournamentId,
                out var registrationDate,
                out var finalPlace,
                out var prizeMoney))
            return;

        try
        {
            using var db = new AppDbContext();

            if (_selectedRegistration is null)
            {
                db.Registrations.Add(new Registration
                {
                    PlayerId = playerId,
                    TournamentId = tournamentId,
                    RegistrationDate = registrationDate,
                    FinalPlace = finalPlace,
                    PrizeMoney = prizeMoney
                });
            }
            else
            {
                var registration = db.Registrations
                    .FirstOrDefault(r => r.Id == _selectedRegistration.Id);

                if (registration is null)
                {
                    ShowRegistrationError("регистрация не найдена");
                    return;
                }

                registration.PlayerId = playerId;
                registration.TournamentId = tournamentId;
                registration.RegistrationDate = registrationDate;
                registration.FinalPlace = finalPlace;
                registration.PrizeMoney = prizeMoney;
            }

            db.SaveChanges();

            RegistrationEditPanel.IsVisible = false;
            _selectedRegistration = null;
            RegistrationsList.SelectedItem = null;
            RegistrationActionsPanel.IsVisible = false;

            DisplayRegistrations();
        }
        catch (Exception ex)
        {
            ShowRegistrationError($"ошибка сохранения : {ex.Message}");
        }
    }

    private void OnCancelRegistrationEditClick(object? sender, RoutedEventArgs e)
    {
        RegistrationEditPanel.IsVisible = false;
        _selectedRegistration = null;
        HideRegistrationError();
    }

    private void OnDeleteRegistrationClick(object? sender, RoutedEventArgs e)
    {
        if (RegistrationsList.SelectedItem is not Registration selected)
            return;

        try
        {
            using var db = new AppDbContext();

            var registration = db.Registrations
                .FirstOrDefault(r => r.Id == selected.Id);

            if (registration is null)
                return;

            db.Registrations.Remove(registration);
            db.SaveChanges();

            RegistrationsList.SelectedItem = null;
            RegistrationActionsPanel.IsVisible = false;
            RegistrationEditPanel.IsVisible = false;

            DisplayRegistrations();
        }
        catch (Exception ex)
        {
            ShowRegistrationError($"ошибка удаления : {ex.Message}");
        }
    }
}
