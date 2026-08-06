using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZMPdesktop.Models;
using ZMPdesktop.Services;

namespace ZMPdesktop.ViewModels;

public partial class BooksViewModel : ViewModelBase
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<BookModel> _books = new();

    [ObservableProperty]
    private ObservableCollection<BookModel> _filteredBooks = new();

    [ObservableProperty]
    private BookModel? _selectedBook;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Add / Edit Form Fields
    [ObservableProperty]
    private bool _isFormVisible;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _formTitle = "Dodaj Nową Książkę";

    [ObservableProperty]
    private string _bookTitle = string.Empty;

    [ObservableProperty]
    private string _bookAuthor = string.Empty;

    [ObservableProperty]
    private string _bookISBN = string.Empty;

    [ObservableProperty]
    private bool _bookIsAvailable = true;

    // Issue Loan Form Fields
    [ObservableProperty]
    private bool _isCheckoutFormVisible;

    [ObservableProperty]
    private ObservableCollection<UserModel> _users = new();

    [ObservableProperty]
    private UserModel? _selectedUserForCheckout;

    [ObservableProperty]
    private int _checkoutDays = 14;

    // Add Offline Reader Quick Dialog Fields
    [ObservableProperty]
    private bool _isOfflineUserDialogVisible;

    [ObservableProperty]
    private string _offlineFullName = string.Empty;

    public BooksViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterBooks();
    }

    private void FilterBooks()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredBooks = new ObservableCollection<BookModel>(Books);
        }
        else
        {
            var query = SearchText.Trim().ToLower();
            var filtered = Books.Where(b => 
                b.Title.ToLower().Contains(query) || 
                b.Author.ToLower().Contains(query) || 
                b.ISBN.ToLower().Contains(query));
            FilteredBooks = new ObservableCollection<BookModel>(filtered);
        }
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsBusy = true;
        StatusMessage = "Wczytywanie katalogu...";
        try
        {
            var booksList = await _apiService.GetBooksAsync();
            Books = new ObservableCollection<BookModel>(booksList);
            FilterBooks();

            var usersList = await _apiService.GetUsersAsync();
            Users = new ObservableCollection<UserModel>(usersList);

            StatusMessage = $"Wczytano {Books.Count} pozycji.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Błąd podczas ładowania danych: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenAddForm()
    {
        FormTitle = "Dodaj Nową Książkę";
        IsEditMode = false;
        BookTitle = string.Empty;
        BookAuthor = string.Empty;
        BookISBN = string.Empty;
        BookIsAvailable = true;
        IsFormVisible = true;
        IsCheckoutFormVisible = false;
    }

    [RelayCommand]
    private void OpenEditForm()
    {
        if (SelectedBook == null) return;

        FormTitle = $"Edycja: {SelectedBook.Title}";
        IsEditMode = true;
        BookTitle = SelectedBook.Title;
        BookAuthor = SelectedBook.Author;
        BookISBN = SelectedBook.ISBN;
        BookIsAvailable = SelectedBook.IsAvailable;
        IsFormVisible = true;
        IsCheckoutFormVisible = false;
    }

    [RelayCommand]
    private void CancelForm()
    {
        IsFormVisible = false;
    }

    [RelayCommand]
    private async Task SaveBookAsync()
    {
        if (string.IsNullOrWhiteSpace(BookTitle) || string.IsNullOrWhiteSpace(BookAuthor))
        {
            StatusMessage = "Tytuł i Autor są wymagani.";
            return;
        }

        IsBusy = true;
        try
        {
            var req = new BookCreateOrUpdateRequest
            {
                Title = BookTitle.Trim(),
                Author = BookAuthor.Trim(),
                ISBN = BookISBN.Trim(),
                IsAvailable = BookIsAvailable
            };

            if (IsEditMode && SelectedBook != null)
            {
                var (success, err) = await _apiService.UpdateBookAsync(SelectedBook.Id, req);
                if (success)
                {
                    StatusMessage = $"Zaktualizowano książkę '{req.Title}'.";
                    IsFormVisible = false;
                    await LoadDataAsync();
                }
                else
                {
                    StatusMessage = $"Błąd edycji: {err}";
                }
            }
            else
            {
                var (success, err) = await _apiService.AddBookAsync(req);
                if (success)
                {
                    StatusMessage = $"Pomyślnie dodano książkę '{req.Title}'.";
                    IsFormVisible = false;
                    await LoadDataAsync();
                }
                else
                {
                    StatusMessage = $"Błąd dodawania: {err}";
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteBookAsync()
    {
        if (SelectedBook == null) return;

        IsBusy = true;
        try
        {
            var (success, err) = await _apiService.DeleteBookAsync(SelectedBook.Id);
            if (success)
            {
                StatusMessage = $"Usunięto pozycję '{SelectedBook.Title}'.";
                SelectedBook = null;
                await LoadDataAsync();
            }
            else
            {
                StatusMessage = $"Nie można usunąć pozycji: {err}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Checkout / Loan Registration
    [RelayCommand]
    private void OpenCheckoutForm()
    {
        if (SelectedBook == null || !SelectedBook.IsAvailable) return;
        IsCheckoutFormVisible = true;
        IsFormVisible = false;
        CheckoutDays = 14;
    }

    [RelayCommand]
    private void CancelCheckoutForm()
    {
        IsCheckoutFormVisible = false;
    }

    [RelayCommand]
    private async Task ConfirmCheckoutAsync()
    {
        if (SelectedBook == null || SelectedUserForCheckout == null)
        {
            StatusMessage = "Wybierz czytelnika do wypożyczenia.";
            return;
        }

        IsBusy = true;
        try
        {
            var (success, err) = await _apiService.CheckoutBookAdminAsync(SelectedBook.Id, SelectedUserForCheckout.Id, CheckoutDays);
            if (success)
            {
                StatusMessage = $"Wypożyczono '{SelectedBook.Title}' dla {SelectedUserForCheckout.DisplayName}.";
                IsCheckoutFormVisible = false;
                await LoadDataAsync();
            }
            else
            {
                StatusMessage = $"Błąd wypożyczenia: {err}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReturnBookAsync()
    {
        if (SelectedBook == null || SelectedBook.IsAvailable) return;

        IsBusy = true;
        try
        {
            var (success, err) = await _apiService.ReturnBookAsync(SelectedBook.Id);
            if (success)
            {
                StatusMessage = $"Zarejestrowano zwrot książki '{SelectedBook.Title}'.";
                await LoadDataAsync();
            }
            else
            {
                StatusMessage = $"Błąd zwrotu: {err}";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Quick Add Offline Reader
    [RelayCommand]
    private void OpenOfflineUserDialog()
    {
        OfflineFullName = string.Empty;
        IsOfflineUserDialogVisible = true;
    }

    [RelayCommand]
    private void CancelOfflineUserDialog()
    {
        IsOfflineUserDialogVisible = false;
    }

    [RelayCommand]
    private async Task CreateOfflineUserAsync()
    {
        if (string.IsNullOrWhiteSpace(OfflineFullName))
        {
            StatusMessage = "Podaj imię i nazwisko czytelnika.";
            return;
        }

        IsBusy = true;
        try
        {
            var (success, user, err) = await _apiService.CreateOfflineUserAsync(OfflineFullName.Trim());
            if (success && user != null)
            {
                StatusMessage = $"Utworzono czytelnika off-line: {user.FullName}";
                IsOfflineUserDialogVisible = false;
                
                var usersList = await _apiService.GetUsersAsync();
                Users = new ObservableCollection<UserModel>(usersList);
                SelectedUserForCheckout = Users.FirstOrDefault(u => u.Id == user.Id);
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
