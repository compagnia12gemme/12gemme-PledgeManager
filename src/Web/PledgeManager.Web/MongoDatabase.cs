using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PledgeManager.Web.Models;
using System;
using System.Collections.Generic;
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
            return await CampaignCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task<Pledge> GetPledge(string campaignId, int userId) {
            var filter = Builders<Pledge>.Filter
                .And(
                    Builders<Pledge>.Filter.Eq(p => p.CampaignId, campaignId),
                    Builders<Pledge>.Filter.Eq(p => p.UserId, userId)
                );
            return await PledgeCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task UpdatePledge(Pledge pledge) {
            var idFilter = Builders<Pledge>.Filter.Eq(p => p.Id, pledge.Id);
            var result = await PledgeCollection.ReplaceOneAsync(idFilter, pledge);
            if(result.ModifiedCount != 1) {
                _logger.LogError("Modified count on UpdatePledge not 1");
                throw new Exception();
            }
        }

        public Task InsertPledge(Pledge pledge) {
            return PledgeCollection.InsertOneAsync(pledge);
        }

        public async Task<(IList<Pledge> Pledges, long Closed)> GetPledges(string campaignId) {
            var filter = Builders<Pledge>.Filter.Eq(p => p.CampaignId, campaignId);
            
            var pledges = await PledgeCollection.Find(filter)
                .SortBy(p => p.UserId)
                .ToListAsync();

            var closedCount = await PledgeCollection
                .CountDocumentsAsync(p => p.CampaignId == campaignId && p.IsClosed == true);

            return (pledges, closedCount);
        }

        private IMongoDatabase LogsDatabase {
            get {
                return Client.GetDatabase("Logs");
            }
        }

        private IMongoCollection<ServerLogItem> ServerLogCollection {
            get {
                return LogsDatabase.GetCollection<ServerLogItem>("ServerLog");
            }
        }

        public Task InsertLogItem(ServerLogItem item) {
            return ServerLogCollection.InsertOneAsync(item);
        }

    }

}
