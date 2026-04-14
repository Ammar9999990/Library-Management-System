namespace LibraryManagementSystem.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Book
{
    public MongoDB.Bson.ObjectId Id { get; set; }
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? InternalId { get; set; }
    public uint BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Summary { get; set; } = "No summary available.";
}