using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PledgeManager.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web {

    public class MongoDatabase {

        private readonly IConfiguration _configuration;
        private readonly ILogger<MongoDatabase> _logger;

        public MongoDatabase(
            IConfiguration configuration,
            ILogger<MongoDatabase> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private readonly object _lockRoot = new object();
        private MongoClient _client = null;

        private MongoClient Client {
            get {
                if(_client == null) {
                    lock (_lockRoot) {
                        if (_client == null) {
                            var connStr = _configuration.GetSection("MongoDb")["ConnectionString"];

                            _logger.LogInformation("Creating new Mongo client");
                            _client = new MongoClient(connStr);
                        }
                    }
                }

                return _client;
            }
        }

        private IMongoDatabase MainDatabase {
            get {
                return Client.GetDatabase("PledgeManager");
            }
        }

        private IMongoCollection<Campaign> CampaignCollection {
            get {
                return MainDatabase.GetCollection<Campaign>("Campaigns");
            }
        }

        private IMongoCollection<Pledge> PledgeCollection {
            get {
                return MainDatabase.GetCollection<Pledge>("Pledges");
            }
        }

        public async Task<Campaign> GetCampaign(string code) {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Code, code);
            return (await CampaignCollection.FindAsync(filter)).SingleOrDefault();
        }

        public async Task<Pledge> GetPledge(string campaignId, int userId) {
            var filter = Builders<Pledge>.Filter
                .And(
                    Builders<Pledge>.Filter.Eq(p => p.CampaignId, campaignId),
                    Builders<Pledge>.Filter.Eq(p => p.UserId, userId)
                );
            return (await PledgeCollection.FindAsync(filter)).SingleOrDefault();
        }

        public async Task UpdatePledge(Pledge pledge) {
            var idFilter = Builders<Pledge>.Filter.Eq(p => p.Id, pledge.Id);
            var result = await PledgeCollection.ReplaceOneAsync(idFilter, pledge);
            if(result.ModifiedCount != 1) {
                _logger.LogError("Modified count on UpdatePledge not 1");
                throw new Exception();
            }
        }

    }

}
