using MongoDB.Bson.Serialization.Attributes;

namespace KanbamApi.Models;

public class Donation
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("DonorEmail")]
    public string? DonorEmail { get; set; }

    [BsonElement("Amount")]
    public decimal Amount { get; set; }

    [BsonElement("Currency")]
    public string? Currency { get; set; }

    [BsonElement("PaymentIntentId")]
    public string? PaymentIntentId { get; set; }

    [BsonElement("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
