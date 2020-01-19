using Microsoft.Extensions.Hosting;
using PledgeManager.Web.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PledgeManager.Web {

    public class PersistentLoggerService : BackgroundService {

        private readonly IPersistentLogQueue _queue;
        private readonly MongoDatabase _database;

        public PersistentLoggerService(
            IPersistentLogQueue queue,
            MongoDatabase database
        ) {
            _queue = queue;
            _database = database;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while(!stoppingToken.IsCancellationRequested) {
                var item = await _queue.DequeueAsync(stoppingToken);
                await _database.InsertLogItem(new ServerLogItem {
                    EventId = item.EventId.Id,
                    Level = item.Level,
                    Category = item.Category,
                    Message = item.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

    }

}
