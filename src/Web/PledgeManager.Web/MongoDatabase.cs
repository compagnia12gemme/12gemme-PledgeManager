using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using PledgeManager.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PledgeManager.Web {
    
    public class MongoDatabase {

        private readonly ILogger<MongoDatabase> _logger;

        public MongoDatabase(ILogger<MongoDatabase> logger) {
            _logger = logger;
        }

        private static readonly object _lockRoot = new object();
        private static MongoClient _client = null;

        private MongoClient Client {
            get {
                _logger.LogTrace("Accessing Mongo client");

                if(_client == null) {
                    lock (_lockRoot) {
                        if (_client == null) {
                            _logger.LogInformation("Creating new Mongo client");

                            _client = new MongoClient(
                                "mongodb://mongo"
                            );
                        }
                    }
                }

                return _client;
            }
        }

        private IMongoDatabase PledgeDatabase {
            get {
                return Client.GetDatabase("Pledges");
            }
        }

        private IMongoCollection<Campaign> CampaignCollection {
            get {
                return PledgeDatabase.GetCollection<Campaign>("Campaigns");
            }
        }

        private IMongoCollection<Pledge> PledgeCollection {
            get {
                return PledgeDatabase.GetCollection<Pledge>("Pledges");
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
