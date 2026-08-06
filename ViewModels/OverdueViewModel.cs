using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZMPdesktop.Models;
using ZMPdesktop.Services;

namespace ZMPdesktop.ViewModels;

public partial class OverdueViewModel : ViewModelBase
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<LoanModel> _overdueLoans = new();

    [ObservableProperty]
    private LoanModel? _selectedLoan;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public OverdueViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsBusy = true;
        StatusMessage = "Pobieranie czytelników zalegających...";
        try
        {
            var list = await _apiService.GetOverdueLoansAsync();
            OverdueLoans = new ObservableCollection<LoanModel>(list);
            StatusMessage = $"Znaleziono {OverdueLoans.Count} zaległych wypożyczeń.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Błąd pobierania danych: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ProcessReturnAsync()
    {
        if (SelectedLoan == null || SelectedLoan.Book == null) return;

        IsBusy = true;
        try
        {
            var (success, err) = await _apiService.ReturnBookAsync(SelectedLoan.Book.Id);
            if (success)
            {
                StatusMessage = $"Pomyślnie zarejestrowano zwrot książki '{SelectedLoan.Book.Title}'.";
                await LoadDataAsync();
            }
            else
            {
                StatusMessage = $"Błąd rejestracji zwrotu: {err}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
