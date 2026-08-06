using System;
using System.Text.Json.Serialization;

namespace ZMPdesktop.Models;

public class UserModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("isOffline")]
    public bool IsOffline { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    public string DisplayName => !string.IsNullOrWhiteSpace(FullName) 
        ? $"{FullName} ({(IsOffline ? "Off-line" : Email)})" 
        : Email;

    public string UserTypeBadge => IsOffline ? "Czytelnik Off-line" : "Konto On-line";
}
