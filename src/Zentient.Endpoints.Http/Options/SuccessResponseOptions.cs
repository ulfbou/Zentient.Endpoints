// <copyright file="SuccessResponseOptions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Zentient.Results;

namespace Zentient.Endpoints.Http.Options
{
    /// <summary>
    /// Provides options for configuring the serialization and structure of
    /// successful API responses within Zentient.Endpoints.Http.
    /// </summary>
    public class SuccessResponseOptions
    {
        /// <summary>
        /// Gets or sets the default HTTP status code to use for successful outcomes
        /// that return data.
        /// <para>Defaults to <c>200 (OK)</c>.</para>
        /// </summary>
        public int DefaultOkStatusCode { get; set; } = ResultStatuses.Ok.Code;

        /// <summary>
        /// Gets or sets the default HTTP status code to use for successful outcomes
        /// that explicitly indicate no content (e.g., for <see cref="Zentient.Endpoints.Unit"/> results).
        /// <para>Defaults to <c>204 (No Content)</c>.</para>
        /// </summary>
        public int DefaultNoContentStatusCode { get; set; } = ResultStatuses.NoContent.Code;

        /// <summary>
        /// Gets or sets a value indicating whether to include the 'message' field in
        /// successful responses if only one message is present.
        /// <para>Defaults to <c>true</c>.</para>
        /// </summary>
        /// <remarks>
        /// If this is false, even a single message will only appear in the 'messages' array.
        /// </remarks>
        public bool IncludeSingleMessageField { get; set; } = true;
    }
}
