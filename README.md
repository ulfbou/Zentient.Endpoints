# Zentient.Endpoints

## 🚀 The Universal Bridge for Zentient.Results and Transport Protocols

`Zentient.Endpoints` is a .NET library designed to streamline how `Zentient.Results` are consumed and adapted at the "endpoint" or "transport" layer of your application. Whether you're building a Web API, a gRPC service, or integrating with messaging queues, `Zentient.Endpoints` provides a unified, clean, and extensible way to translate the explicit outcomes of your business logic (represented by `Zentient.Results`) into appropriate responses for various communication protocols.

---

## ✨ Why `Zentient.Endpoints`?

In modern, layered architectures like Clean Architecture, your application's core logic should produce clear, explicit outcomes using a pattern like `Zentient.Results`. However, mapping these internal results to external transport-specific responses (like HTTP status codes and Problem Details, or gRPC error codes and metadata) often introduces boilerplate and inconsistency.

`Zentient.Endpoints` solves this by offering:

* **Unified Abstraction:** A common interface (`IEndpointResult`) for all endpoint outcomes, regardless of the underlying transport.
* **Protocol-Specific Adapters:** Dedicated, opt-in packages (`.Http`, `.Grpc`, etc.) to cleanly translate `IEndpointResult` into standard protocol responses.
* **Robust Error Handling:** Seamless integration with `Zentient.Results` ensures detailed error information is consistently captured and exposed.
* **Extensibility:** Easily customize how errors are mapped (e.g., Problem Details, gRPC metadata) or integrate new transport protocols.
* **Enhanced Developer Experience:** Reduce boilerplate, improve readability, and ensure consistent error handling across your entire system.

---

## 📦 Packaging Strategy

`Zentient.Endpoints` follows a modular packaging approach, allowing you to only include the components relevant to your specific application's needs.

* **`Zentient.Endpoints` (Core):**
    * **Purpose:** Contains the foundational, transport-agnostic interfaces (`IEndpointResult`), base types (`EndpointResult<T>`, `Unit`), and core functional extensions (`Bind`, `Map`).
    * **Dependencies:** Primarily `Zentient.Results`.
    * **Installation:**
        ```bash
        dotnet add package Zentient.Endpoints
        ```

* **`Zentient.Endpoints.Http`:**
    * **Purpose:** Provides specific functionality for mapping `IEndpointResult` to ASP.NET Core HTTP responses, including Minimal APIs and traditional MVC controllers. This includes extensions for `IResult` (ASP.NET Core's HTTP result type), `IEndpointFilter` for global response normalization, and configurable Problem Details mapping.
    * **Dependencies:** `Zentient.Endpoints`, `Microsoft.AspNetCore.Http.Abstractions`, `Microsoft.AspNetCore.Mvc.Core`.
    * **Installation:**
        ```bash
        dotnet add package Zentient.Endpoints.Http
        ```

* **`Zentient.Endpoints.Grpc`:**
    * **Purpose:** Offers utilities and extension methods for integrating `IEndpointResult` with gRPC services, including mapping `Zentient.Results` failures to `RpcException` with structured metadata.
    * **Dependencies:** `Zentient.Endpoints`, `Grpc.AspNetCore`, `Grpc.Net.Client`.
    * **Installation:**
        ```bash
        dotnet add package Zentient.Endpoints.Grpc
        ```

---

## 🛠️ When to Use `EndpointResult<T>` over Raw `Result<T>`

Use `EndpointResult<T>` when your `Zentient.Results` outcome needs to be exposed at a public application boundary (an "endpoint") and potentially adapted for a specific transport protocol.

* **`Zentient.Results.IResult<T>` / `Result<T>`:** Ideal for internal application layers (e.g., domain services, application services, command/query handlers) where the focus is purely on the business outcome and error details.
* **`Zentient.Endpoints.IEndpointResult<T>` / `EndpointResult<T>`:** Use this as the return type for your Minimal API handlers, MVC controller actions, gRPC service methods, or message consumers. It wraps the `Zentient.Results` outcome and can carry additional `TransportMetadata` relevant to how it will be presented to the outside world.

---

## 💻 Quick Usage Example (HTTP)

```csharp
// In your Minimal API endpoint handler (e.g., Program.cs or a dedicated module)

// 1. Add services:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails(); // Ensure ProblemDetails is configured
builder.Services.AddZentientEndpointsHttp(); // Extension method to add necessary services

// 2. Use the endpoint filter for consistent response mapping
app.UseEndpointFilter<NormalizeEndpointResultFilter>();

// 3. Define an endpoint that returns EndpointResult<T>
app.MapPost("/api/users", async (CreateUserRequest request, IUserService userService) =>
{
    // userService method returns Zentient.Results.IResult<User>
    var userResult = await userService.CreateUser(request);

    // Wrap it in EndpointResult to prepare for transport mapping
    var endpointResult = EndpointResult<User>.From(userResult);

    // Endpoint filter will intercept this and convert it to Microsoft.AspNetCore.Http.IResult
    return endpointResult;
})
.WithName("CreateUser")
.Produces<UserResponse>((int)HttpStatusCode.Created)
.ProducesProblem((int)HttpStatusCode.BadRequest) // Indicates ProblemDetails will be returned
.ProducesProblem((int)HttpStatusCode.InternalServerError);

// Inside your IUserService (Application Layer)
public class UserService : IUserService
{
    // ...
    public async Task<Zentient.Results.IResult<User>> CreateUser(CreateUserRequest request)
    {
        // Your business logic returns Zentient.Results.IResult<User>
        // e.g., return Result<User>.Success(newUser);
        // e.g., return Result<User>.Failure(new ErrorInfo(...), ResultStatuses.BadRequest);
        return await Task.FromResult(Zentient.Results.Result<User>.Success(new User(Guid.NewGuid(), request.Name)));
    }
}
