using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZMPdesktop.Models;
using ZMPdesktop.Services;

namespace ZMPdesktop.ViewModels;

public partial class 
    MainViewModel : ViewModelBase
{
    private readonly ApiService _apiService;
    private readonly SignalRService _signalRService;
    private readonly Action _onLogout;

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private string _librarianName = string.Empty;

    [ObservableProperty]
    private string _notificationBanner = string.Empty;

    [ObservableProperty]
    private bool _hasNotification;

    public BooksViewModel BooksVM { get; }
    public LoansViewModel LoansVM { get; }
    public UsersViewModel UsersVM { get; }
    public ReservationsViewModel ReservationsVM { get; }
    public OverdueViewModel OverdueVM { get; }

    public MainViewModel(ApiService apiService, SignalRService signalRService, Action onLogout)
    {
        _apiService = apiService;
        _signalRService = signalRService;
        _onLogout = onLogout;

        BooksVM = new BooksViewModel(_apiService);
        LoansVM = new LoansViewModel(_apiService);
        UsersVM = new UsersViewModel(_apiService);
        ReservationsVM = new ReservationsViewModel(_apiService);
        OverdueVM = new OverdueViewModel(_apiService);

        LibrarianName = _apiService.CurrentUser?.FullName ?? _apiService.CurrentUser?.Email ?? "Bibliotekarz";

        _signalRService.OnNotificationReceived += HandleNotification;
        _ = ConnectSignalRAsync();
    }

    private async Task ConnectSignalRAsync()
    {
        if (_apiService.Token != null)
        {
            await _signalRService.ConnectAsync(_apiService.BaseUrl, _apiService.Token);
        }
    }

    private void HandleNotification(string message)
    {
        NotificationBanner = $"Powiadomienie z systemu: {message}";
        HasNotification = true;

        // Auto-refresh active tab data
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await RefreshCurrentTabAsync();
        });
    }

    [RelayCommand]
    private void DismissNotification()
    {
        HasNotification = false;
        NotificationBanner = string.Empty;
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        _ = RefreshCurrentTabAsync();
    }

    public async Task RefreshCurrentTabAsync()
    {
        switch (SelectedTabIndex)
        {
            case 0:
                await BooksVM.LoadDataAsync();
                break;
            case 1:
                await LoansVM.LoadDataAsync();
                break;
            case 2:
                await UsersVM.LoadDataAsync();
                break;
            case 3:
                await ReservationsVM.LoadDataAsync();
                break;
            case 4:
                await OverdueVM.LoadDataAsync();
                break;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _signalRService.DisconnectAsync();
        _apiService.Logout();
        _onLogout();
    }
}
