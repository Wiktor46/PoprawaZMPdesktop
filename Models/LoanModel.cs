using System;
using System.Text.Json.Serialization;

namespace ZMPdesktop.Models;

public class LoanModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("borrowedAt")]
    public DateTime BorrowedAt { get; set; }

    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; set; }

    [JsonPropertyName("returnedAt")]
    public DateTime? ReturnedAt { get; set; }

    [JsonPropertyName("isOverdue")]
    public bool IsOverdue { get; set; }

    [JsonPropertyName("daysOverdue")]
    public int DaysOverdue { get; set; }

    [JsonPropertyName("book")]
    public BookModel? Book { get; set; }

    [JsonPropertyName("borrower")]
    public UserModel? Borrower { get; set; }

    public string StatusText => ReturnedAt.HasValue 
        ? "Zwrócona" 
        : (IsOverdue ? $"Zaległa ({DaysOverdue} dni)" : "Wypożyczona");
}
