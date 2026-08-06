using System;
using System.Text.Json.Serialization;

namespace ZMPdesktop.Models;

public class ReservationModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("bookId")]
    public int BookId { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("reservedAt")]
    public DateTime ReservedAt { get; set; }

    [JsonPropertyName("notifiedAt")]
    public DateTime? NotifiedAt { get; set; }

    [JsonPropertyName("book")]
    public BookModel? Book { get; set; }

    [JsonPropertyName("borrower")]
    public UserModel? Borrower { get; set; }

    public string StatusText => NotifiedAt.HasValue ? "Książka gotowa do odbioru!" : $"Pozycja w kolejce: {Position}";
}
