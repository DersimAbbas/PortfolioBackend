using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PortfolioBackend.Models
{
    public class TechStack
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Technologies")]
        public string Technologies {  get; set; }

        [BsonElement("Tech-Experience")]
        public string TechExperience { get; set; }
        [BsonElement("skill-level")]
        public double SkillLevel { get; set; }

    }
}
