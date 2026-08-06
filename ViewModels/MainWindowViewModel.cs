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
    }

    private void OnLoginSuccess()
    {
        var mainVM = new MainViewModel(_apiService, _signalRService, OnLogout);
        CurrentView = mainVM;
        _ = mainVM.RefreshCurrentTabAsync();
    }

    private void OnLogout()
    {
        CurrentView = new LoginViewModel(_apiService, OnLoginSuccess);
    }
}
