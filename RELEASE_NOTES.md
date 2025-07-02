# 📦 Zentient.Endpoints v0.1.0 – Initial Release

📅 **Release Date:** July 2, 2025  
🏷️ **Version:** 0.1.0  
📄 **Status:** ✅ Released  
🔗 **Repository:** [Zentient.Endpoints](https://github.com/ulfbou/Zentient.Endpoints)

---

## ✨ Overview

We are thrilled to announce the inaugural release of **Zentient.Endpoints**, a foundational library for cleanly modeling **transport-agnostic operation outcomes** at the boundaries of your .NET applications.

This release establishes a consistent, extensible, and testable way to communicate operation results—whether success, failure, or exceptions—along with relevant metadata. Built to integrate seamlessly with [`Zentient.Results`](https://github.com/ulfbou/Zentient.Results), this package forms the baseline abstraction for upcoming support in transports such as **HTTP**, **gRPC**, and **messaging systems**.

---

## 🧱 Key Features

### 1. `IEndpointOutcome` and `EndpointOutcome`

> For non-generic outcomes without return values.

- Encapsulates operation result semantics (`IsSuccess`, `IsFailure`, `Status`).
- Supports rich error modeling via `ErrorInfo` and descriptive messages.
- Provides immutable construction using static factory methods:
  - `Success()`, `From(IResult result)`, `FromError(ErrorInfo)`, `NotFound()`, etc.
  - Semantic convenience overloads for common scenarios like `Unauthorized()`, `Forbidden()`, `FromException(Exception)`.

### 2. `IEndpointOutcome<TValue>` and `EndpointOutcome<TValue>`

> For generic outcomes that return a value.

- Extends the non-generic interface with a strongly-typed `Value` property.
- Supports fluent construction using overloads like:
  - `Success(TValue value)`, `From(IResult<TValue> result)`, `NoContent()`.

### 3. `TransportMetadata` (sealed record)

> Flexible metadata carrier for transport-agnostic hints and tagging.

- Immutable with `with`-expression support.
- Backed by an internal `ImmutableDictionary<string, object?>` for extensibility.
- Fluent API for metadata construction and access:
  - `WithTag(key, value)`, `TryGetTag<T>`, `WithLogger`, `From(IDictionary<...>)`.

### 4. `EndpointOutcomeMetadataExtensions`

> Extension methods to fluently attach or modify `TransportMetadata`.

- Apply metadata fluently to any outcome:
  ```csharp
  outcome.WithMetadata(m => m.WithTag("http.status_code", 200));
````

* Includes `OutcomeEquals<T>` for deep equality checks.

### 5. `Unit` and `UnitJsonConverter`

> Marker type for representing “no value” in generic scenarios.

* `Unit`: Lightweight singleton `struct` (commonly used in `EndpointOutcome<Unit>`).
* `UnitJsonConverter`: Serializes to `{}` in JSON, preserving consistency in API responses.

### 6. `MetadataKeys`

> Static class defining commonly used metadata tag keys.

* Prevents magic strings and ensures metadata consistency.
* Examples: `MetadataKeys.Logger`, `MetadataKeys.HttpStatusCode`.

---

## 🎯 Motivation

Zentient.Endpoints addresses a recurring challenge in layered application design: **cleanly separating domain logic outcomes from transport-specific behavior**. It introduces:

* ✅ **Transport-agnostic boundaries** for endpoint handlers.
* 🧪 **Improved testability** through composable, inspectable outcomes.
* 🔁 **Consistency across transports**, enabling adapters for HTTP, gRPC, and messaging.
* 🔐 **Immutable and fluent APIs** for safe and expressive usage.
* 🧩 **Rich extensibility** through metadata without tight coupling.

---

## 🚀 Getting Started

Install the package:

```bash
dotnet add package Zentient.Endpoints
```

Start returning `IEndpointOutcome` or `IEndpointOutcome<T>` from your endpoint handlers or services. Use `EndpointOutcome.[From|Success|Failure]` static methods to construct results. Attach metadata as needed using `.WithMetadata(...)`.

---

## 📦 Dependencies

* ✅ [Zentient.Results](https://github.com/ulfbou/Zentient.Results) – Provides the underlying `IResult` and `ErrorInfo` primitives used throughout.

---

## 📅 Roadmap Highlights

> For full roadmap details, refer to the [project roadmap](https://github.com/ulfbou/Zentient.Endpoints/wiki/Roadmap).

| Version | Focus Area                                 | Status         |
| ------- | ------------------------------------------ | -------------- |
| 0.1.0   | Core abstractions (`IEndpointOutcome`)     | ✅ Complete     |
| 0.2.0   | HTTP integration (ProblemDetails, filters) | 🔄 In Progress |
| 0.3.0   | gRPC outcome mapping & metadata trailers   | 🗓️ Planned    |
| 0.4.0+  | Messaging (RabbitMQ, Kafka), WebSockets    | 🗓️ Planned    |

---

## 🙌 Acknowledgements

A heartfelt thank you to the **Zentient Framework Team** and early collaborators for shaping this vision and executing it with clarity, discipline, and purpose.

---

## 🪪 License

Zentient.Endpoints is distributed under the [MIT License](https://github.com/ulfbou/Zentient.Endpoints/blob/main/LICENSE).
