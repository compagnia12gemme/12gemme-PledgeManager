using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
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

                            BsonClassMap.RegisterClassMap<Campaign>();

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

        public async Task<Campaign> GetCampaign(string code) {
            var filter = Builders<Campaign>.Filter.Eq(c => c.Code, code);
            return await CampaignCollection.Find(filter).SingleOrDefaultAsync();
        }

    }

}
