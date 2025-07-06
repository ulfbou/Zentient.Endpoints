// <copyright file="TestLogger.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.Extensions.Logging;

using Xunit.Sdk;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// A simple test logger that captures log messages for assertion.
    /// </summary>
    /// <typeparam name="T">The category type for the logger.</typeparam>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    [SuppressMessage("Design", "CA1515:Member names should begin with a capital letter", Justification = "Consistent with ASP.NET Core conventions for fluent builders.")]
    public sealed class TestLogger<T> : ILogger<T>
    {
        /// <summary>
        /// Gets the captured log entries.
        /// </summary>
        internal Collection<TestLogEntry> LogEntries { get; } = new();

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);

            LogEntries.Add(new TestLogEntry
            {
                LogLevel = logLevel,
                EventId = eventId,
                State = state,
                Exception = exception,
                FormattedMessage = formatter(state, exception),
                Category = typeof(T).FullName ?? string.Empty
            });
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc/>
        IDisposable ILogger.BeginScope<TState>(TState state) => NullScope.Instance;

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            private NullScope() { }
            public void Dispose() { }
        }

        /// <summary>
        /// Fluent assertion: Asserts that a log entry with the specified log level and message fragment was recorded.
        /// </summary>
        /// <param name="level">The log level to check for.</param>
        /// <param name="fragment">A substring to search for in the log message.</param>
        /// <returns>This logger instance for chaining.</returns>
        /// <exception cref="XunitException">Thrown if the expected log entry is not found.</exception>
        public TestLogger<T> ShouldHaveLogged(LogLevel level, string fragment)
        {
            if (!LogEntries.Any(e => e.LogLevel == level && e.FormattedMessage.Contains(fragment, StringComparison.Ordinal)))
                throw new XunitException($"Expected log message containing '{fragment}' with level '{level}' not found for category {typeof(T).FullName}.");
            return this;
        }

        /// <summary>
        /// Fluent assertion: Asserts that no log entry with the specified log level and message fragment was recorded.
        /// </summary>
        /// <param name="level">The log level to check for.</param>
        /// <param name="fragment">A substring to search for in the log message.</param>
        /// <returns>This logger instance for chaining.</returns>
        /// <exception cref="XunitException">Thrown if an unexpected log entry is found.</exception>
        public TestLogger<T> ShouldNotHaveLogged(LogLevel level, string fragment)
        {
            if (LogEntries.Any(e => e.LogLevel == level && e.FormattedMessage.Contains(fragment, StringComparison.Ordinal)))
                throw new XunitException($"Unexpected log message containing '{fragment}' with level '{level}' found for category {typeof(T).FullName}.");
            return this;
        }

        private string GetDebuggerDisplay()
            => $"TestLogger<{typeof(T).Name}> ({LogEntries.Count} entries)";
    }
}
