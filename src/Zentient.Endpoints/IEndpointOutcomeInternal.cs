// <copyright file="IEndpointOutcomeInternal.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zentient.Results;

namespace Zentient.Endpoints
{
    /// <summary>
    /// For in-assembly filters/adapters that need to unwrap the raw business result.
    /// </summary>
    internal interface IEndpointOutcomeInternal
    {
        /// <summary>
        /// Gets the underlying business result for internal filters and adapters.
        /// </summary>
        /// <returns>The <see cref="IResult"/> encapsulating the raw operation outcome.</returns>
        IResult GetUnderlyingResult();
    }
}
