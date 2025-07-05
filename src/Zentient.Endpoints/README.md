# Zentient.Endpoints — Unified, Transport-Agnostic Result Handling for .NET

[![Zentient.Endpoints on NuGet](https://img.shields.io/nuget/v/Zentient.Endpoints?label=Zentient.Endpoints)](https://www.nuget.org/packages/Zentient.Endpoints)
[![Zentient.Endpoints.Http (Coming Soon)](https://img.shields.io/badge/Zentient.Endpoints.Http-in%20development-yellow)](https://github.com/ulfbou/Zentient.Endpoints.Http)
[![Build Status](https://img.shields.io/github/actions/workflow/status/ulfbou/Zentient.Endpoints/build.yml)](https://github.com/ulfbou/Zentient.Endpoints/actions)

---

## Table of Contents

- [🚀 Overview](#-overview)
- [❓ Why Zentient.Endpoints?](#-why-zentientendpoints)
- [🏛️ Architecture Overview](#-architecture-overview)
- [💻 Quick Start](#-quick-start)
- [🔧 Advanced Usage](#-advanced-usage)
- [📡 gRPC Support](#-grpc-support)
- [📊 Observability](#-observability)
- [🗺️ Vision & Roadmap](#-vision--roadmap)
- [🤝 Contributing](#-contributing)

---

## 🚀 Overview

**Zentient.Endpoints** is a modular, protocol-agnostic result adapter for .NET services. It bridges clean, transport-neutral application logic—powered by [`Zentient.Results`](https://www.nuget.org/packages/Zentient.Results)—with modern transports like HTTP, gRPC, and messaging.

```csharp
// ✅ With Zentient.Endpoints
var result = await _service.CreateUser(req);
return EndpointResult<User>.From(result);
```

No more scattered status codes. No brittle exception filters. Just **structured, consistent, observable outcomes—every time**.

---

## ❓ Why Zentient.Endpoints?

Modern .NET applications often suffer from transport leakage and duplicated logic. Common symptoms:

- 🔁 Repeated error-to-response mapping across services
- ⚠️ Inconsistent error formats between transports
- 🙈 Opaque try-catch blocks inside core logic
- 🔍 Weak observability and tracing metadata

Zentient.Endpoints introduces a consistent boundary abstraction:

```text
IResult<T> → IEndpointResult<T> → Transport-specific response
```

### ✨ Key Differentiators

- 🚛 **Protocol-Agnostic Outcome Flow**  
  Return `IResult<T>` from core logic, adapt it to any transport—HTTP, gRPC, Messaging.

- 🧱 **Clean Architecture Friendly**  
  Wrap results at the presentation boundary with no transport coupling in your domain layer.

- 📦 **Modular & Extensible Design**  
  - `Zentient.Endpoints.Http` (🚧 _in active development_)
  - `Zentient.Endpoints.Grpc` (planned)
  - Messaging, SignalR adapters coming soon

- 🛡️ **Exception Resilience**  
  `Bind(...)` operations catch exceptions and convert them to structured `ErrorInfo`.

- 🔍 **Built-In Observability**  
  Metadata tagging + rich error models = end-to-end traceability

---

## ⚠️ About HTTP Integration

> The `Zentient.Endpoints.Http` package is currently under **active development** and will be released soon.  
> APIs and configuration may evolve slightly prior to its stable release.

You can preview integration patterns now and start planning adoption into ASP.NET Core Minimal APIs or MVC controllers.

---

## 🏛️ Architecture Overview

![Zentient Architecture](./docs/assets/diagram.svg)

---

## 💻 Quick Start

### 1. Install Zentient.Endpoints Core

```bash
dotnet add package Zentient.Endpoints --version 0.1.0
```

### 2. Return `IResult<T>` from your application layer

```csharp
public class UserService : IUserService
{
    public async Task<IResult<User>> CreateUser(CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return Result<User>.Failure(ErrorInfo.BadRequest("EmptyName", "Name is required"));

        return Result<User>.Success(new User(Guid.NewGuid(), req.Name));
    }
}
```

### 3. Adapt using `EndpointResult<T>`

```csharp
var appResult = await userService.CreateUser(req);
return EndpointResult<User>.From(appResult); // transport adapter applies this result
```

---

## 🔧 Advanced Usage

### Custom `IProblemDetailsMapper` (HTTP Preview)

```csharp
public class MyProblemDetailsMapper : IProblemDetailsMapper
{
    public ProblemDetails Map(ErrorInfo error, HttpContext ctx)
    {
        return new ProblemDetails
        {
            Status = error.Category.ToHttpStatusCode(),
            Title = error.Message,
            Type = $"https://errors.myapi.com/{error.Code}",
            Instance = ctx.Request.Path,
            Detail = error.Detail,
            Extensions = {
                ["requestId"] = ctx.TraceIdentifier,
                ["errorCode"] = error.Code
            }
        };
    }
}
```

Register with:

```csharp
builder.Services.AddScoped<IProblemDetailsMapper, MyProblemDetailsMapper>();
```

### Exception-Safe Binding

```csharp
return EndpointResult<User>
    .From(serviceResult)
    .Bind(user => user.IsActive ? Result.Success(user) : throw new InvalidOperationException());
```

Exceptions are safely converted into structured internal errors.

---

## 📡 gRPC Support (Planned)

Expected pattern with `Zentient.Endpoints.Grpc`:

```csharp
public override Task<UserResponse> GetUser(UserRequest req, ServerCallContext ctx)
{
    var result = await _service.GetUser(req.Id);
    return result
        .ToEndpointResult()
        .ToRpcResult(mapper: MyGrpcMapper);
}
```

Maps structured errors to trailers and typed `RpcException`.

---

## 📊 Observability

- 🧩 `TransportMetadata`: Immutable context tags like logger, request ID, headers
- 📄 `ErrorInfo`: Rich, structured, loggable errors—perfect for Serilog, OpenTelemetry

---

## 🗺️ Roadmap

| Feature                  | Status                  |
|--------------------------|-------------------------|
| Core Outcome APIs        | ✅ Stable               |
| Zentient.Endpoints.Http  | 🚧 In Development       |
| Zentient.Endpoints.Grpc  | 🧪 Planned              |
| Messaging Adapter        | 🔭 Upcoming             |
| SignalR/WebSocket        | 🔭 Exploring            |
| SDK Code Generation      | 🧰 Planned              |
| Better ProblemDetails UX | ✨ Planned              |

---

## 🤝 Contributing

We welcome contributions from developers who believe boundaries should be elegant, not repetitive.

- Open an [Issue](https://github.com/ulfbou/Zentient.Endpoints/issues)
- Join [Discussions](https://github.com/ulfbou/Zentient.Endpoints/discussions)
- Submit a [Pull Request](https://github.com/ulfbou/Zentient.Endpoints/pulls)

> Built with ❤️ by [@ulfbou](https://github.com/ulfbou) and Zentient contributors.
