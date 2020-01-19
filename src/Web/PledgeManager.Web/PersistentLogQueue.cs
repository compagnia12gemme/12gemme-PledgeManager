using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PledgeManager.Web {

    public interface IPersistentLogQueue {

        public class LogItem {
            public EventId EventId { get; set; }
            public LogLevel Level { get; set; }
            public string Category { get; set; }
            public string Message { get; set; }
        }

        void Enqueue(LogItem item);

        Task<LogItem> DequeueAsync(CancellationToken cancellationToken);

    }

    public class PersistentLogQueue : IPersistentLogQueue {

        private readonly ConcurrentQueue<IPersistentLogQueue.LogItem> _logQueue = new ConcurrentQueue<IPersistentLogQueue.LogItem>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public async Task<IPersistentLogQueue.LogItem> DequeueAsync(CancellationToken cancellationToken) {
            await _signal.WaitAsync(cancellationToken);

            if(_logQueue.TryDequeue(out var message)) {
                return message;
            }
            else {
                return default;
            }
        }

        public void Enqueue(IPersistentLogQueue.LogItem item) {
            _logQueue.Enqueue(item);
            _signal.Release();
        }

    }

}
