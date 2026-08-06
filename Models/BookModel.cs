using System.Text.Json.Serialization;

namespace ZMPdesktop.Models;

public class BookModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("isbn")]
    public string ISBN { get; set; } = string.Empty;

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonPropertyName("activeLoan")]
    public ActiveLoanInfo? ActiveLoan { get; set; }

    public string StatusText => IsAvailable ? "Dostępna" : "Wypożyczona";
}

public class ActiveLoanInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("borrowedAt")]
    public System.DateTime BorrowedAt { get; set; }

    [JsonPropertyName("dueDate")]
    public System.DateTime DueDate { get; set; }

    [JsonPropertyName("isOverdue")]
    public bool IsOverdue { get; set; }

    [JsonPropertyName("borrower")]
    public UserModel? Borrower { get; set; }
}
