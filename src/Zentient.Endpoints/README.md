# 🏠 Zentient.Endpoints

📅 Last Updated: 2025-06-19
📄 Status: ✅ Active
📦 Repository: `Zentient.Endpoints`
🏷️ Version: `0.1.0`

-----

\<\!-- Badges --\>

[](https://www.nuget.org/packages/Zentient.Endpoints)
[](https://github.com/ulfbou/Zentient.Endpoints/actions)

-----

## Table of Contents

  * [🎯 Purpose](%23purpose)
  * [✨ Key Features](%23key-features)
  * [📦 Installation](%23installation)
  * [⚡ Quick Example](%23quick-example)
  * [📚 Documentation](%23documentation)
  * [🗺️ Roadmap Highlights](%23roadmap-highlights)
  * [🤝 Contributing](%23contributing)
  * [📄 License](%23license)
  * [📚 See Also](%23see-also)

-----

\<a name="purpose"\>\</a\>
🎯 **Purpose**

**Zentient.Endpoints** provides a **protocol-agnostic abstraction layer** for representing operation outcomes at the boundaries of your .NET applications. It introduces standardized `IEndpointOutcome` contracts to enable clean layering, rich metadata, and a unified return model for any application logic, decoupled from specific transport protocols like HTTP or gRPC.

Built on top of [`Zentient.Results`](%5Bhttps://github.com/ulfbou/Zentient.Results%5D\(https://github.com/ulfbou/Zentient.Results\)), it serves as the foundational integration point for `Zentient.Results` into various endpoint types.

**Who is it for?** Developers building clean-architecture, layered systems, or distributed applications who want to:

  * Decouple business logic from transport-specific details.
  * Enforce consistent API response patterns.
  * Improve the testability and maintainability of their endpoint handlers.

-----

\<a name="key-features"\>\</a\>
✨ **Key Features**

  * ✅ **Protocol-Agnostic Outcome Contracts**: Define endpoint handler return types using `IEndpointOutcome` and `IEndpointOutcome<T>`, ensuring your business logic remains independent of transport concerns.
  * 🧱 **Clean Architecture Alignment**: Promotes strict separation of concerns by providing a dedicated layer for transport-facing outcomes, keeping your core domain logic clean and highly testable.
  * ⚙️ **Extensible Transport Metadata**: Attach arbitrary, transport-agnostic metadata using `TransportMetadata`, a sealed record with a tag-based metadata system, allowing for flexible contextual information that can be consumed by future transport adapters.
  * 🔄 **Immutable Outcome Representation**: All core outcome types (`EndpointOutcome`, `EndpointOutcome<T>`, `TransportMetadata`) are immutable, ensuring predictability, thread safety, and functional composition.
  * 🔗 **Seamless `Zentient.Results` Integration**: Easily convert `Zentient.Results.IResult` instances into `IEndpointOutcome` using dedicated factory methods, preserving all success/failure states, errors, and messages.
  * 🧪 **Enhanced Testability**: By decoupling business logic from transport specifics, `IEndpointOutcome` makes endpoint handlers inherently more testable without requiring ASP.NET Core or other transport framework dependencies.
  * 🚫 **No Transport-Specific Dependencies (v0.1.0)**: This initial release of `Zentient.Endpoints` contains no direct dependencies on ASP.NET Core, gRPC, or messaging libraries. It provides the core abstractions only. Transport-specific integration will be provided by separate `Zentient.Endpoints.*` packages in future releases.

-----

\<a name="installation"\>\</a\>
📦 **Installation**

Install the core `Zentient.Endpoints` NuGet package:

```bash
dotnet add package Zentient.Endpoints
```

-----

\<a name="quick-example"\>\</a\>
⚡ **Quick Example**

This example demonstrates how to create and inspect `IEndpointOutcome` instances, which would then be consumed by transport-specific adapters (e.g., for HTTP, gRPC, or messaging) in later `Zentient.Endpoints.*` packages.

```csharp
using Zentient.Endpoints;
using Zentient.Results;
using System;
using System.Collections.Generic;
using System.Linq; // For .Any()

// --- 1. Define your Domain Result (using Zentient.Results) ---
public record User(Guid Id, string Name);

public class UserService
{
    public IResult<User> CreateUser(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<User>.Failure(
                new ErrorInfo("NameRequired", "User name is required.", ErrorCategory.Validation),
                ResultStatus.BadRequest);
        }
        return Result<User>.Success(new User(Guid.NewGuid(), name)); // Guid.NewGuid() will generate a unique ID
    }
}

// --- 2. Create Endpoint Outcomes from Domain Results ---
public class EndpointHandler
{
    private readonly UserService _userService = new UserService(); // In a real app, this would be injected

    public IEndpointOutcome<User> HandleCreateUser(string userName)
    {
        // Call your domain service, which returns Zentient.Results.IResult<T>
        IResult<User> domainResult = _userService.CreateUser(userName);

        // Convert the domain result into an IEndpointOutcome<T>
        // This is the common return type for your endpoint handlers.
        return EndpointOutcome<User>.From(domainResult);
    }

    public IEndpointOutcome HandleDeleteUser(Guid userId)
    {
        // Example: a non-generic domain result
        IResult deleteResult = Result.Success(ResultStatus.NoContent);
        // Or Result.NotFound(new ErrorInfo("UserNotFound", "User not found."));

        return EndpointOutcome.From(deleteResult);
    }
}

// --- 3. Inspecting Endpoint Outcomes (e.g., in a test or a future adapter) ---
public static class OutcomeInspector
{
    public static void InspectOutcome(IEndpointOutcome outcome)
    {
        Console.WriteLine($"Outcome Status: {outcome.Status}");
        Console.WriteLine($"Is Success: {outcome.IsSuccess}");
        Console.WriteLine($"Is Failure: {outcome.IsFailure}");

        if (outcome.IsFailure)
        {
            // Iterate through all errors for detailed output
            foreach (var error in outcome.Errors)
            {
                Console.WriteLine($"  - Error: {error.Code} - {error.Message} ({error.Category})");
                if (error.Metadata != null && error.Metadata.Any())
                {
                    Console.WriteLine($"    Metadata: {string.Join(", ", error.Metadata.Select(kv => $"{kv.Key}={kv.Value}"))}");
                }
            }
        }

        if (outcome is IEndpointOutcome<User> userOutcome && userOutcome.IsSuccess)
        {
            Console.WriteLine($"User Value: {userOutcome.Value}");
        }

        // Accessing metadata (e.g., for future HTTP status code mapping)
        Console.WriteLine($"Metadata Tags Count: {outcome.Metadata.Tags.Count}");
        if (outcome.Metadata.TryGetTag("custom.tag", out string? customTag))
        {
            Console.WriteLine($"Custom Tag: {customTag}");
        }
    }

    public static void Main(string[] args)
    {
        var handler = new EndpointHandler();

        Console.WriteLine("--- Successful User Creation ---");
        var successOutcome = handler.HandleCreateUser("Alice");
        InspectOutcome(successOutcome);

        Console.WriteLine("\n--- Failed User Creation (Validation) ---");
        var failureOutcome = handler.HandleCreateUser("");
        InspectOutcome(failureOutcome);

        Console.WriteLine("\n--- Successful User Deletion ---");
        var deleteSuccessOutcome = handler.HandleDeleteUser(Guid.NewGuid());
        InspectOutcome(deleteSuccessOutcome);
    }
}

/* Expected Console Output for Quick Example:
--- Successful User Creation ---
Outcome Status: Ok
Is Success: True
Is Failure: False
User Value: User { Id = <GUID>, Name = Alice } // <GUID> will be a random GUID
Metadata Tags Count: 0

--- Failed User Creation (Validation) ---
Outcome Status: BadRequest
Is Success: False
Is Failure: True
  - Error: NameRequired - User name is required. (Validation)
Metadata Tags Count: 0

--- Successful User Deletion ---
Outcome Status: NoContent
Is Success: True
Is Failure: False
Metadata Tags Count: 0
*/
```

-----

\<a name="documentation"\>\</a\>
📚 **Documentation**

Explore the full capabilities of Zentient.Endpoints through our comprehensive documentation:

  * [Getting Started: How to set up Zentient.Endpoints in your project](/docs/guides/getting-started-endpoints.md)
  * [Core Concepts: Understanding IEndpointOutcome and TransportMetadata](/docs/guides/core-concepts-endpoints.md)
  * [API Reference: Detailed API documentation for all types and members](/docs/api-reference/zentient-endpoints.md)
  * [Roadmap: Future plans and development milestones](/docs/roadmap/endpoints.md)

-----

\<a name="roadmap-highlights"\>\</a\>
🗺️ **Roadmap Highlights**

This section provides a high-level overview of the strategic milestones for Zentient.Endpoints. For a detailed plan, please refer to the [full roadmap document](/docs/roadmap/endpoints.md).

### 🧱 v0.1.0 – Core Abstractions (Completed)

**Goal**: Establish the foundational `IEndpointOutcome` abstraction and its core components, enabling protocol-agnostic outcome representation. This includes `IEndpointOutcome`, `EndpointOutcome` (classes), `TransportMetadata` (sealed record), and factory methods for converting `Zentient.Results.IResult` instances.

### 🌐 v0.2.0 – HTTP Integration (In Progress)

**Goal**: Introduce first-class support for HTTP APIs, enabling consistent outcome handling in ASP.NET Core applications. This will involve the `Zentient.Endpoints.Http` package with HTTP-specific mappers, filters, and Problem Details (RFC 9457) support.

### 📦 v0.3.0 – gRPC Deep Dive (Planned)

**Goal**: Introduce first-class support for gRPC services, enabling consistent outcome handling in a binary RPC context. This will involve the `Zentient.Endpoints.Grpc` package with gRPC-specific mappers and filters.

### 📈 v0.4.0 – Initial Transport Expansion (Planned)

**Goal**: Begin expanding Zentient.Endpoints to cover more diverse transport protocols, demonstrating its true protocol-agnostic nature. This will explore patterns for messaging (e.g., RabbitMQ, Kafka) and WebSockets/SignalR integration.

### 🛠️ v0.5.0 – Enhanced Developer Experience & Options (Planned)

**Goal**: Further enhance developer experience, introduce foundational observability features, and refine existing components based on accumulated feedback. This includes refined error-to-ProblemDetails mapping, logging integration, and initial OpenTelemetry integration.

### 📊 v0.6.0 – Advanced Error Handling & Tooling (Planned)

**Goal**: Provide more sophisticated error handling capabilities and tooling to streamline development workflows. This will cover comprehensive `ErrorInfo` helpers (e.g., validation library integration) and custom error serialization.

### 🧪 v0.7.0 – Comprehensive Observability & Performance (Planned)

**Goal**: Deepen observability integration and begin focusing on performance optimizations. This includes full OpenTelemetry integration, performance benchmarking, and error reporting integration.

### 📚 v0.8.0 – Cross-Cutting Refinements (Planned)

**Goal**: Address common cross-cutting concerns and refine existing APIs based on accumulated feedback. This will involve authorization integration, caching support, and more advanced middleware/interceptor examples.

### 🔒 v0.9.0 – Documentation & Community Readiness (Release Candidate)

**Goal**: Focus heavily on documentation completeness, community resources, and preparing for a release candidate phase. This includes a comprehensive documentation site, migration guides, and community engagement.

### 🚀 v1.0.0 – General Availability (Stable Release)

**Goal**: Announce the stable release of Zentient.Endpoints for widespread production adoption. This marks the official release of stable NuGet packages and continuous improvement.

-----

\<a name="contributing"\>\</a\>
🤝 **Contributing**

We welcome ideas, bug fixes, extensions for other protocols, and ecosystem integrations. Your contributions help shape the future of Zentient.Endpoints\!

  * [Open an issue](https://github.com/ulfbou/Zentient.Endpoints/issues)
  * [Start a discussion](https://github.com/ulfbou/Zentient.Endpoints/discussions)
  * [Fork and submit a PR](https://github.com/ulfbou/Zentient.Endpoints/pulls)

-----

\<a name="license"\>\</a\>
📄 **License**

Zentient.Endpoints is licensed under the [MIT License](https://github.com/ulfbou/Zentient.Endpoints/blob/main/LICENSE).

-----

\<a name="see-also"\>\</a\>
📚 **See Also**

  * [Zentient.Results Repository ↗](https://github.com/ulfbou/Zentient.Results)
  * [Zentient.Endpoints Wiki ↗](https://github.com/ulfbou/Zentient.Endpoints/wiki)
  * [Zentient.Endpoints Roadmap ↗](/docs/roadmap/endpoints.md)
  * [Zentient Framework Documentation Standards ↗](/docs/conventions/documentation-standards.md)
  * [RFC 9457: Problem Details for HTTP APIs ↗](https://www.rfc-editor.org/rfc/rfc9457.html)

-----

> Built with ❤️ by @ulfbou and the Zentient community.
