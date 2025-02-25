using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace PortfolioBackend.Models
{
    [BsonIgnoreExtraElements]
    public class PipeLineStage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Basic stage details
        [BsonElement("project")]
        public string Project { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        // New field for personal anecdotes and detailed insights
        [BsonElement("details")]
        public string Details { get; set; }

        // Additional metadata
        [BsonElement("order")]
        public int Order { get; set; }

        [BsonElement("stageType")]
        public string StageType { get; set; }

        // Optional: You can also include existing fields if needed
       

    }
}
