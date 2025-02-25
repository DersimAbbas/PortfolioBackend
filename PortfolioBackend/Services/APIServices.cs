using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PortfolioBackend.Models;

namespace PortfolioBackend.Services
{
    public class APIServices
    {
        private readonly IMongoCollection<TechStack> _collection;
        private readonly IMongoCollection<PipeLineStage> _pipecollection;
        public APIServices(IOptions<DatabaseSettings> dbSettings)
        {
            var mongoClient = new MongoClient(
                dbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(
                dbSettings.Value.DatabaseName);
            _collection = mongoDatabase.GetCollection<TechStack>(
                dbSettings.Value.CollectionName);
            _pipecollection = mongoDatabase.GetCollection<PipeLineStage>(
              dbSettings.Value.CollectionName);

        }
  
        public async Task<List<TechStack>> GetAsync()
        {

          var technology = await _collection.Find(tech => tech.project == null).ToListAsync();
            return technology;
        }

        public async Task<List<TechStack>> GetProjectsAsync()
        {

            var filter = Builders<TechStack>.Filter.And(
                Builders<TechStack>.Filter.Where(p => !string.IsNullOrEmpty(p.project)),
                Builders<TechStack>.Filter.Where(p => !string.IsNullOrEmpty(p.Description)),
                Builders<TechStack>.Filter.Where(p => !string.IsNullOrEmpty(p.Technologies)));
            
            return await _collection.Find(filter).ToListAsync();
        }




        public async Task<TechStack?> GetByIdAsync(string id) =>
            await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();
      
        public async Task CreateAsync(TechStack tech) =>
            await _collection.InsertOneAsync(tech);

        public async Task UpdateAsync(string id, TechStack updatedTech) =>
            await _collection.ReplaceOneAsync(m => m.Id == id, updatedTech);

        public async Task RemoveAsync(string id) =>
            await _collection.DeleteOneAsync(m => m.Id == id);

        public async Task<List<PipeLineStage>> GetAllStagesAsync()
        {
            var filter = Builders<PipeLineStage>.Filter.And(
            Builders<PipeLineStage>.Filter.Where(stage => !string.IsNullOrEmpty(stage.Project)),
            Builders<PipeLineStage>.Filter.Where(stage => !string.IsNullOrEmpty(stage.Description)),
            Builders<PipeLineStage>.Filter.Where(stage => !string.IsNullOrEmpty(stage.StageType))
        );
            var sort = Builders<PipeLineStage>.Sort.Ascending(stage => stage.Order);
            return await _pipecollection.Find(filter).Sort(sort).ToListAsync();
        }

        // Insert a new pipeline stage
        public async Task<PipeLineStage> CreateStageAsync(PipeLineStage stage)
        {
            await _pipecollection.InsertOneAsync(stage);
            return stage;
        }

        //insert bulk pipeline stages

        public async Task CreateManyStagesAsync(List<PipeLineStage> stage)
        {
          await _pipecollection.InsertManyAsync(stage);
        }

    }
}

