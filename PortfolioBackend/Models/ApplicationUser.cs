using AspNetCore.Identity.Mongo.Model;

namespace PortfolioBackend.Models
{
    public class ApplicationUser : MongoUser<string>
    {
        public ApplicationUser()
        {
            Id = Guid.NewGuid().ToString(); // Ensure Id is never null
           
        }
    }
}
