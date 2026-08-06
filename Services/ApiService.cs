using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ZMPdesktop.Models;

namespace ZMPdesktop.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string _baseUrl = "http://localhost:5228";
    private string? _token;

    public string BaseUrl
    {
        get => _baseUrl;
        set => _baseUrl = value.TrimEnd('/');
    }

    public string? Token => _token;
    public bool IsLoggedIn => !string.IsNullOrEmpty(_token);
    public LoginResponse? CurrentUser { get; private set; }

    public ApiService()
    {
        _httpClient = new HttpClient();
    }

    public void SetAuthToken(string? token)
    {
        _token = token;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/auth/login", new LoginRequest
            {
                Email = email,
                Password = password
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null)
                {
                    CurrentUser = result;
                    SetAuthToken(result.Token);
                    return (true, null);
                }
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, "Nieprawidłowy e-mail lub hasło.");
            }

            return (false, $"Błąd logowania: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return (false, $"Nie można połączyć z API ({BaseUrl}): {ex.Message}");
        }
    }

    public void Logout()
    {
        _token = null;
        CurrentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // Books API
    public async Task<List<BookModel>> GetBooksAsync()
    {
        try
        {
            var books = await _httpClient.GetFromJsonAsync<List<BookModel>>($"{BaseUrl}/api/books");
            return books ?? new List<BookModel>();
        }
        catch
        {
            return new List<BookModel>();
        }
    }

    public async Task<BookModel?> GetBookDetailsAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BookModel>($"{BaseUrl}/api/books/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> AddBookAsync(BookCreateOrUpdateRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/books", request);
            if (response.IsSuccessStatusCode) return (true, null);
            var err = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(err) ? response.ReasonPhrase : err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateBookAsync(int id, BookCreateOrUpdateRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/api/books/{id}", request);
            if (response.IsSuccessStatusCode) return (true, null);
            var err = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(err) ? response.ReasonPhrase : err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteBookAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/books/{id}");
            if (response.IsSuccessStatusCode) return (true, null);
            var err = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(err) ? response.ReasonPhrase : err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> CheckoutBookAdminAsync(int bookId, int userId, int days = 14)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/books/{bookId}/checkout-admin", new AdminCheckoutRequest
            {
                UserId = userId,
                Days = days
            });

            if (response.IsSuccessStatusCode) return (true, null);
            var err = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(err) ? response.ReasonPhrase : err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ReturnBookAsync(int bookId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{BaseUrl}/api/books/{bookId}/return", null);
            if (response.IsSuccessStatusCode) return (true, null);
            var err = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(err) ? response.ReasonPhrase : err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // Users API
    public async Task<List<UserModel>> GetUsersAsync(bool? isOffline = null)
    {
        try
        {
            var url = $"{BaseUrl}/api/users";
            if (isOffline.HasValue) url += $"?isOffline={isOffline.Value}";
            var users = await _httpClient.GetFromJsonAsync<List<UserModel>>(url);
            return users ?? new List<UserModel>();
        }
        catch
        {
            return new List<UserModel>();
        }
    }

    public async Task<(bool Success, UserModel? User, string? ErrorMessage)> CreateOfflineUserAsync(string fullName, string? email = null)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/users/offline", new CreateOfflineUserRequest
            {
                FullName = fullName,
                Email = email
            });

            if (response.IsSuccessStatusCode)
            {
                var createdUser = await response.Content.ReadFromJsonAsync<UserModel>();
                return (true, createdUser, null);
            }

            var err = await response.Content.ReadAsStringAsync();
            return (false, null, string.IsNullOrWhiteSpace(err) ? response.ReasonPhrase : err);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    // Loans API
    public async Task<List<LoanModel>> GetLoansAsync(bool? active = null, bool? overdue = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (active.HasValue) queryParams.Add($"active={active.Value}");
            if (overdue.HasValue) queryParams.Add($"overdue={overdue.Value}");
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

            var loans = await _httpClient.GetFromJsonAsync<List<LoanModel>>($"{BaseUrl}/api/loans{queryString}");
            return loans ?? new List<LoanModel>();
        }
        catch
        {
            return new List<LoanModel>();
        }
    }

    public async Task<List<LoanModel>> GetOverdueLoansAsync()
    {
        try
        {
            var loans = await _httpClient.GetFromJsonAsync<List<LoanModel>>($"{BaseUrl}/api/loans/overdue");
            return loans ?? new List<LoanModel>();
        }
        catch
        {
            return new List<LoanModel>();
        }
    }

    // Reservations API
    public async Task<List<ReservationModel>> GetAdminReservationsAsync()
    {
        try
        {
            var res = await _httpClient.GetFromJsonAsync<List<ReservationModel>>($"{BaseUrl}/api/reservations/admin");
            return res ?? new List<ReservationModel>();
        }
        catch
        {
            return new List<ReservationModel>();
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> CancelReservationAdminAsync(int reservationId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/reservations/admin/{reservationId}");
            if (response.IsSuccessStatusCode) return (true, null);
            var err = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(err) ? response.ReasonPhrase : err);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
