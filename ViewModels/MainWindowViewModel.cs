using CommunityToolkit.Mvvm.ComponentModel;
using ZMPdesktop.Services;

namespace ZMPdesktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ApiService _apiService;
    private readonly SignalRService _signalRService;

    [ObservableProperty]
    private ViewModelBase _currentView;

    public MainWindowViewModel()
    {
        _apiService = new ApiService();
        _signalRService = new SignalRService();

        _currentView = new LoginViewModel(_apiService, OnLoginSuccess);
        _apiService.OnSessionExpired += HandleSessionExpired;
    }

    private void OnLoginSuccess()
    {
        var mainVm = new MainViewModel(_apiService, _signalRService, OnLogout);
        CurrentView = mainVm;
        _ = mainVm.RefreshCurrentTabAsync();
    }

    private void OnLogout()
    {
        CurrentView = new LoginViewModel(_apiService, OnLoginSuccess);
    }
    
    private void HandleSessionExpired()
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await _signalRService.DisconnectAsync();
            _apiService.Logout(); 
            
            var loginVm = new LoginViewModel(_apiService, OnLoginSuccess)
            {
                ErrorMessage = "⏰ Sesja wygasła (token JWT stracił ważność). Zaloguj się ponownie."
            };
            
            
            CurrentView = loginVm;
        });
    }
}
