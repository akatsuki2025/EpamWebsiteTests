using System.Text.Json.Serialization;

namespace EpamWebsite.Business.Models;

public sealed class CreateUserResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;
}