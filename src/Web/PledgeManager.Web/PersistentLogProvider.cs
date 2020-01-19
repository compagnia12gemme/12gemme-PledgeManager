using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PledgeManager.Web {

    public class PersistentLogProvider : ILoggerProvider {

        private IPersistentLogQueue _queue;

        public PersistentLogProvider(
            IPersistentLogQueue queue
        ) {
            _queue = queue;
        }

        public ILogger CreateLogger(string categoryName) {
            return new PersistentLogger(_queue, categoryName);
        }

        public void Dispose() {
            
        }

    }

    public static class PersistentLogProviderExtensions {

        public static ILoggingBuilder AddPersistentLogger(this ILoggingBuilder builder) {
            builder.Services.AddHostedService<PersistentLoggerService>();
            builder.Services.AddSingleton<IPersistentLogQueue, PersistentLogQueue>();
            builder.Services.AddSingleton<ILoggerProvider, PersistentLogProvider>();
            return builder;
        }

    }

}
