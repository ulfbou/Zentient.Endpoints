// <copyright file="EndpointHttpOptionsBuilderExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options.Validation;

namespace Zentient.Endpoints.Http.Options
{
    /// <summary>
    /// Provides common convention extension methods for <see cref="OptionsBuilder{TOptions}"/>
    /// within the Zentient Framework, centralizing validation and startup behaviors.
    /// </summary>
    /// <remarks>
    /// This static class encapsulates standard configuration practices for Zentient options,
    /// ensuring consistency across modules.
    /// </remarks>
    public static class EndpointHttpOptionsBuilderExtensions
    {
        /// <summary>
        /// Applies Zentient's default conventions to the options builder.
        /// This includes Data Annotations validation and ensures validation runs on application startup.
        /// </summary>
        /// <typeparam name="TOptions">The type of options to configure, which must be a reference type.</typeparam>
        /// <param name="optionsBuilder">The <see cref="OptionsBuilder{TOptions}"/> to extend. Must not be <see langword="null"/>.</param>
        /// <returns>The <see cref="OptionsBuilder{TOptions}"/> for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsBuilder"/> is <see langword="null"/>.</exception>
        public static OptionsBuilder<TOptions> ApplyZentientConventions<TOptions>(
            this OptionsBuilder<TOptions> optionsBuilder)
            where TOptions : class
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

            return optionsBuilder
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
    }
}
