using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZMPdesktop.Models;
using ZMPdesktop.Services;

namespace ZMPdesktop.ViewModels;

public partial class LoansViewModel : ViewModelBase
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<LoanModel> _loans = new();

    [ObservableProperty]
    private ObservableCollection<LoanModel> _filteredLoans = new();

    [ObservableProperty]
    private LoanModel? _selectedLoan;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _filterIndex = 0; // 0 = Wszystkie, 1 = Tylko Aktywne, 2 = Zwrócone

    public LoansViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterLoans();
    }

    partial void OnFilterIndexChanged(int value)
    {
        _ = LoadDataAsync();
    }

    private void FilterLoans()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredLoans = new ObservableCollection<LoanModel>(Loans);
        }
        else
        {
            var query = SearchText.Trim().ToLower();
            var filtered = Loans.Where(l => 
                (l.Book != null && (l.Book.Title.ToLower().Contains(query) || l.Book.Author.ToLower().Contains(query) || l.Book.ISBN.ToLower().Contains(query))) ||
                (l.Borrower != null && ((l.Borrower.FullName ?? "").ToLower().Contains(query) || l.Borrower.Email.ToLower().Contains(query))));
            FilteredLoans = new ObservableCollection<LoanModel>(filtered);
        }
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsBusy = true;
        StatusMessage = "Wczytywanie listy wypożyczeń...";
        try
        {
            bool? activeFilter = FilterIndex switch
            {
                1 => true,
                2 => false,
                _ => null
            };

            var list = await _apiService.GetLoansAsync(active: activeFilter);
            Loans = new ObservableCollection<LoanModel>(list);
            FilterLoans();
            StatusMessage = $"Wczytano {FilteredLoans.Count} wypożyczeń.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Błąd pobierania wypożyczeń: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReturnBookAsync()
    {
        if (SelectedLoan == null || SelectedLoan.Book == null || SelectedLoan.ReturnedAt.HasValue) return;

        IsBusy = true;
        try
        {
            var (success, err) = await _apiService.ReturnBookAsync(SelectedLoan.Book.Id);
            if (success)
            {
                StatusMessage = $"Pomyślnie zwrócono książkę '{SelectedLoan.Book.Title}'.";
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
