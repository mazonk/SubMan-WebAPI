using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Subman.Models;

public class Subscription {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set;}

    [BsonElement("userId")]
    public string? UserId { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("price")]
    public double Price { get; set; }

    [BsonElement("currency")]
    public string? Currency { get; set; }

    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }

    [BsonElement("interval")]
    public int Interval { get; set; } // in days
}
