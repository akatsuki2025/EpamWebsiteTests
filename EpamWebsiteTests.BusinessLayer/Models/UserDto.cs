using System.Text.Json;
using System.Text.Json.Serialization;

namespace EpamWebsite.Business.Models;

public sealed class UserDto
{
    [JsonPropertyName("id")] 
    public int Id { get; init; }

    [JsonPropertyName("name")] 
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("username")] 
    public string Username { get; init; } = string.Empty;

    [JsonPropertyName("email")] 
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("address")] 
    public JsonElement Address { get; init; }

    [JsonPropertyName("phone")] 
    public string Phone { get; init; } = string.Empty;

    [JsonPropertyName("website")] 
    public string Website { get; init; } = string.Empty;

    [JsonPropertyName("company")] 
    public JsonElement Company { get; init; }
}
