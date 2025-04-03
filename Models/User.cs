using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Subman.Models;

public class User {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required]
    [BsonElement("username")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long."), MaxLength(20, ErrorMessage = "Username cannot exceed 20 characters")]
    public string? Username { get; set; }


    [EmailAddress(ErrorMessage = "Invalid email address")]
    [BsonElement("email")]
    public string? Email { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("createdAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}