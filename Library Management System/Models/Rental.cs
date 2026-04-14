using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManagementSystem.Models
{
    [BsonIgnoreExtraElements] // This prevents the crash if the DB has extra fields
    public class Rental
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public uint UserId { get; set; }
        public uint BookId { get; set; }
        public float Label { get; set; }
    }
}