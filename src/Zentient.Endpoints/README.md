
# Zentient.Endpoints — Unified, Transport-Agnostic Result Handling for .NET

    


---

🧭 Overview

Zentient.Endpoints provides a transport-agnostic result boundary for .NET APIs, gRPC services, and event-driven systems. Built on top of Zentient.Results, it introduces standardized IEndpointOutcome abstractions, enabling clean layering, rich metadata, and effortless transport mapping.

Zentient.Endpoints.Http extends this to ASP.NET Core, turning your IResult<T> into structured IActionResult—automatically.

> 💡 Read the full 📚 Zentient.Endpoints Wiki for usage guides, API reference, and roadmap.




---

🎯 Key Features

✅ Transport-Agnostic Endpoint Contracts
Use IEndpointOutcome<T> to unify result handling across HTTP, gRPC, messaging, and more.

📡 Built-In ASP.NET Core Integration
Normalize results to IActionResult with filters, metadata mapping, and ProblemDetails enrichment.

🧱 Clean Architecture Compliance
Keeps your core logic decoupled from controllers, filters, or pipelines.

⚙️ Metadata-Driven Mapping
TransportMetadata enables context-aware transformations (status codes, error types, gRPC trailers).

🛡️ Exception Wrapping
All thrown exceptions are gracefully turned into structured ErrorInfo.Internal.



---

📦 Installation

dotnet add package Zentient.Endpoints
dotnet add package Zentient.Endpoints.Http


---

⚡ Quick Example

Program.cs

builder.Services.AddProblemDetails();
builder.Services.AddZentientEndpointsHttp();

Minimal API

app.UseEndpointFilter<NormalizeEndpointResultFilter>();

app.MapPost("/api/users", async (CreateUserRequest req, IUserService service) =>
{
    var result = await service.CreateUser(req);
    return EndpointOutcome<User>.From(result);
});

Application Layer

public Task<IResult<User>> CreateUser(CreateUserRequest req)
{
    if (string.IsNullOrWhiteSpace(req.Name))
        return Task.FromResult(Result<User>.Failure(ErrorInfo.BadRequest("NameRequired")));

    return Task.FromResult(Result<User>.Success(new User(Guid.NewGuid(), req.Name)));
}


---

📚 Documentation

📁 Section	📄 Wiki Pages

🔧 Core Library	IEndpointOutcome · TransportMetadata · Unit
🌐 HTTP Integration	Minimal API Integration · Controllers · Default Mappers



---

🛠️ Customization

🔄 Override IProblemDetailsMapper

public class MyProblemDetailsMapper : IProblemDetailsMapper
{
    public ProblemDetails Map(ErrorInfo error, HttpContext ctx)
    {
        return new()
        {
            Status = error.Category.ToHttpStatusCode(),
            Title = error.Message,
            Detail = error.Detail,
            Instance = ctx.Request.Path,
            Extensions = { ["code"] = error.Code, ["requestId"] = ctx.TraceIdentifier }
        };
    }
}

🔐 Exception-Aware Binding

return EndpointOutcome<User>
    .From(result)
    .Bind(user => user.IsActive ? Result.Success(user) : throw new InvalidOperationException());


---

🗺 Roadmap

Version	Highlights

0.1.0	✅ Core API + ASP.NET Core integration
0.2.0	🔜 gRPC support with trailer metadata
0.3.0	🔄 Messaging integration (Kafka, Azure Service Bus)
0.4.0+	📈 OpenTelemetry support, SDK generators



---

🤝 Contributing

We welcome ideas, bug fixes, extensions for other protocols, and ecosystem integrations.

Open an issue

Start a discussion

Fork and submit a PR



---

> Built with ❤️ by @ulfbou and the Zentient community.
For full API reference and integration guides, see the 📚 Zentient.Endpoints Wiki.
