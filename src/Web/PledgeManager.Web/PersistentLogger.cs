using Microsoft.Extensions.Logging;
using System;

namespace PledgeManager.Web {

    public class PersistentLogger : ILogger {

        private readonly string _categoryName;
        private readonly IPersistentLogQueue _queue;

        private readonly LogLevel _minLevel;

        public PersistentLogger(IPersistentLogQueue queue, string categoryName) {
            _queue = queue;
            _categoryName = categoryName;

            if(_categoryName.StartsWith("PledgeManager.Web")) {
                _minLevel = LogLevel.Information;
            }
            else if(_categoryName.StartsWith("Microsoft.AspNetCore.Antiforgery.DefaultAntiforgery")) {
                _minLevel = LogLevel.None;
            }
            else {
                _minLevel = LogLevel.Warning;
            }
        }
        
        public IDisposable BeginScope<TState>(TState state) {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) {
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            if(logLevel < _minLevel) {
                return;
            }

            Console.WriteLine("State type {0}: {1}", typeof(TState), System.Text.Json.JsonSerializer.Serialize(state, typeof(TState)));

            _queue.Enqueue(new IPersistentLogQueue.LogItem {
                EventId = eventId,
                Level = logLevel,
                Category = _categoryName,
                Message = formatter(state, exception)
            });
        }

    }

}
