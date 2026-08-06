using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZMPdesktop.Models;
using ZMPdesktop.Services;

namespace ZMPdesktop.ViewModels;

public partial class ReservationsViewModel : ViewModelBase
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<ReservationModel> _reservations = new();

    [ObservableProperty]
    private ReservationModel? _selectedReservation;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ReservationsViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsBusy = true;
        StatusMessage = "Pobieranie listy rezerwacji...";
        try
        {
            var list = await _apiService.GetAdminReservationsAsync();
            Reservations = new ObservableCollection<ReservationModel>(list);
            StatusMessage = $"Wczytano {Reservations.Count} rezerwacji.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Błąd pobierania rezerwacji: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelReservationAsync()
    {
        if (SelectedReservation == null) return;

        IsBusy = true;
        try
        {
            var (success, err) = await _apiService.CancelReservationAdminAsync(SelectedReservation.Id);
            if (success)
            {
                StatusMessage = $"Anulowano rezerwację #{SelectedReservation.Id}.";
                await LoadDataAsync();
            }
            else
            {
                StatusMessage = $"Błąd anulowania rezerwacji: {err}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
