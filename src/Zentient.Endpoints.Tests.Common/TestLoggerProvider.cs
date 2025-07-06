// <copyright file="TestLoggerProvider.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// An <see cref="ILoggerProvider"/> that registers a <see cref="TestLogger{T}"/> for the given category.
    /// </summary>
    internal sealed class TestLoggerProvider<T> : ILoggerProvider
    {
        /// <summary>
        /// Gets the test logger instance.
        /// </summary>
        public TestLogger<T> Logger { get; } = new();

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => Logger;

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
