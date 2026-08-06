using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZMPdesktop.Services;

namespace ZMPdesktop.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly ApiService _apiService;
    private readonly Action _onLoginSuccess;

    [ObservableProperty]
    private string _email = "admin@library.com";

    [ObservableProperty]
    private string _password = "Admin123!";

    [ObservableProperty]
    private string _baseUrl = "http://localhost:5228";

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public LoginViewModel(ApiService apiService, Action onLoginSuccess)
    {
        _apiService = apiService;
        _onLoginSuccess = onLoginSuccess;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Proszę podać adres e-mail oraz hasło.";
            return;
        }

        IsBusy = true;
        try
        {
            _apiService.BaseUrl = BaseUrl;
            var (success, errorMessage) = await _apiService.LoginAsync(Email, Password);

            if (success)
            {
                if (_apiService.CurrentUser?.Role != "Admin")
                {
                    ErrorMessage = "Dostęp przyznawany jest tylko dla kont z uprawnieniami bibliotekarza (Admin).";
                    _apiService.Logout();
                    return;
                }

                _onLoginSuccess();
            }
            else
            {
                ErrorMessage = errorMessage ?? "Wystąpił błąd podczas logowania.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
