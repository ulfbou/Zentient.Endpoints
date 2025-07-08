// <copyright file="TestLoggerFactory.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// A simple test logger factory that creates <see cref="TestLogger{T}"/> instances.
    /// </summary>
    internal sealed class TestLoggerFactory : ILoggerFactory
    {
        private readonly List<ILogger> _loggers = new();

        public static TestLoggerFactory Instance { get; } = new TestLoggerFactory();

        /// <summary>
        /// Creates a logger for the given category name.
        /// </summary>
        public ILogger CreateLogger(string categoryName)
        {
            // We need to create a specific generic TestLogger<T> based on the category name
            // For simplicity in a test helper, we'll use object as the generic type arg
            // if we can't infer a more specific one.
            // If more precision is needed, a dictionary mapping category names to concrete ILogger<T> types
            // could be used, or a more sophisticated reflection-based instantiation.
            // For now, TestLogger<object> will capture all logs.
            var logger = new TestLogger<object>();
            _loggers.Add(logger);
            return logger;
        }

        /// <summary>
        /// Not needed for test logger.
        /// </summary>
        public void AddProvider(ILoggerProvider provider) { /* Not implemented */ }

        /// <summary>
        /// Nothing to dispose.
        /// </summary>
        public void Dispose() { /* Nothing to dispose */ }

        /// <summary>
        /// Retrieves all <see cref="TestLogger{T}"/> instances created by this factory, cast to their base ILogger.
        /// Note: To access LogEntries, you would need to cast them back to TestLogger&lt;object&gt; or TestLogger&lt;TCategory&gt;.
        /// </summary>
        /// <returns>A read-only list of all created loggers.</returns>
        public IReadOnlyList<ILogger> GetLoggers() => _loggers.AsReadOnly();

        /// <summary>
        /// Retrieves a specific <see cref="TestLogger{TCategory}"/> instance by its generic type.
        /// </summary>
        /// <typeparam name="TCategory">The category type of the logger to retrieve.</typeparam>
        /// <returns>The <see cref="TestLogger{TCategory}"/> instance, or null if not found.</returns>
        public TestLogger<TCategory>? GetLogger<TCategory>()
        {
            // This assumes the factory creates a new TestLogger<object> for each category.
            // If you need specific TCategory loggers, you would need to adjust CreateLogger
            // to store them by type or categoryName and retrieve them here.
            // For the current setup (TestLogger<object> for all), this will return the first TestLogger<object>
            // which handles all logs. For truly specific TCategory logs, you'd need a map:
            // private readonly Dictionary<Type, ILogger> _specificLoggers = new();
            // ... in CreateLogger:
            // if (!_specificLoggers.TryGetValue(typeof(T), out var logger))
            // { logger = new TestLogger<T>(); _specificLoggers[typeof(T)] = logger; }
            // return logger;
            // And then here:
            // return (TestLogger<TCategory>)_specificLoggers.GetValueOrDefault(typeof(TCategory));

            // Given the current factory implementation (creating TestLogger<object> for all categories),
            // this will find and return the *first* TestLogger<object> instance.
            // If precise generic category logging is desired, the TestLoggerFactory needs a more sophisticated internal map.
            return _loggers.OfType<TestLogger<TCategory>>().FirstOrDefault();
        }
    }
}
