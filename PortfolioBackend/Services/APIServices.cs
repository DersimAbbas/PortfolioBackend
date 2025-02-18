using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PortfolioBackend.Models;

namespace PortfolioBackend.Services
{
    public class APIServices
    {
        private readonly IMongoCollection<TechStack> _collection;

        public APIServices(IOptions<DatabaseSettings> dbSettings)
        {
            var mongoClient = new MongoClient(
                dbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(
                dbSettings.Value.DatabaseName);
            _collection = mongoDatabase.GetCollection<TechStack>(
                dbSettings.Value.CollectionName);

        }
  
        public async Task<List<TechStack>> GetAsync() =>
            await _collection.Find(_ => true).ToListAsync();
 
        public async Task<TechStack?> GetByIdAsync(string id) =>
            await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();
      
        public async Task CreateAsync(TechStack tech) =>
            await _collection.InsertOneAsync(tech);

        public async Task UpdateAsync(string id, TechStack updatedTech) =>
            await _collection.ReplaceOneAsync(m => m.Id == id, updatedTech);

        public async Task RemoveAsync(string id) =>
            await _collection.DeleteOneAsync(m => m.Id == id);

    }
}

