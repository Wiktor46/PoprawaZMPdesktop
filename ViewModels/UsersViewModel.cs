using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZMPdesktop.Models;
using ZMPdesktop.Services;

namespace ZMPdesktop.ViewModels;

public partial class UsersViewModel : ViewModelBase
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<UserModel> _users = new();

    [ObservableProperty]
    private UserModel? _selectedUser;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _filterIndex = 0; // 0 = Wszyscy, 1 = On-line, 2 = Off-line

    [ObservableProperty]
    private bool _isAddDialogVisible;

    [ObservableProperty]
    private string _newFullName = string.Empty;

    [ObservableProperty]
    private string _newEmail = string.Empty;

    public UsersViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    partial void OnFilterIndexChanged(int value)
    {
        _ = LoadDataAsync();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsBusy = true;
        StatusMessage = "Wczytywanie listy użytkowników...";
        try
        {
            bool? isOffline = FilterIndex switch
            {
                1 => false,
                2 => true,
                _ => null
            };

            var list = await _apiService.GetUsersAsync(isOffline);
            Users = new ObservableCollection<UserModel>(list);
            StatusMessage = $"Znaleziono {Users.Count} użytkowników.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Błąd ładowania użytkowników: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenAddDialog()
    {
        NewFullName = string.Empty;
        NewEmail = string.Empty;
        IsAddDialogVisible = true;
    }

    [RelayCommand]
    private void CancelAddDialog()
    {
        IsAddDialogVisible = false;
    }

    [RelayCommand]
    private async Task SaveOfflineUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewFullName))
        {
            StatusMessage = "Proszę podać Imię i Nazwisko czytelnika.";
            return;
        }

        IsBusy = true;
        try
        {
            var (success, createdUser, err) = await _apiService.CreateOfflineUserAsync(
                NewFullName.Trim(), 
                string.IsNullOrWhiteSpace(NewEmail) ? null : NewEmail.Trim());

            if (success && createdUser != null)
            {
                StatusMessage = $"Zarejestrowano czytelnika off-line: {createdUser.FullName}";
                IsAddDialogVisible = false;
                await LoadDataAsync();
            }
            else
            {
                StatusMessage = $"Błąd dodawania czytelnika: {err}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
