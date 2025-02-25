using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PortfolioBackend.Models
{
    public class TechStack
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("project")]
        public string? project { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("GitHubUrl")]
        public string? githubUrl {  get; set; }

        [BsonElement("Technologies")]
        public string Technologies { get; set; } = null!;

        [BsonElement("Tech-Experience")]
        public string TechExperience { get; set; } = null!;
        [BsonElement("skill-level")]
        public double SkillLevel { get; set; }

        [BsonElement("image")]
        public string? Image { get; set; }

        
        
    }
}
