// <copyright file="TestLogEntry.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// Represents a captured log entry.
    /// </summary>
    internal class TestLogEntry
    {
        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        public LogLevel LogLevel { get; set; }
        /// <summary>
        /// Gets or sets the event id.
        /// </summary>
        public EventId EventId { get; set; }
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public object? State { get; set; }
        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        public Exception? Exception { get; set; }
        /// <summary>
        /// Gets or sets the formatted message.
        /// </summary>
        public string FormattedMessage { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the log category.
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }
}
