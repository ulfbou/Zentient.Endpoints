# CHANGELOG

## 0.1.0 (2025-07-02)

### New Features

* **Initial Release of Zentient.Endpoints Library:**
    * Introduced core abstractions and concrete implementations for handling endpoint operation outcomes.
    * **`IEndpointOutcome` and `EndpointOutcome`**: Provides a non-generic contract and implementation for representing the result of an endpoint operation, including success/failure status, errors, messages, and transport-level metadata.
    * **`IEndpointOutcome<TValue>` and `EndpointOutcome<TValue>`**: Extends the non-generic outcome to support operations that return a specific value on success, maintaining the same rich status and metadata capabilities.
    * **`TransportMetadata`**: A new `sealed partial record` type designed for attaching transport-agnostic metadata (e.g., HTTP status codes, headers, loggers) to endpoint outcomes, enabling flexible and fluent modification.
    * **`EndpointOutcomeMetadataExtensions`**: Static extension methods to fluently add or transform `TransportMetadata` on both generic and non-generic endpoint outcomes.
    * **`IEndpointOutcomeInternal`**: An internal interface allowing in-assembly filters and adapters to access the underlying `IResult` from the `Zentient.Results` library.
    * **`Unit` struct**: A singleton struct representing the absence of a value, useful for generic endpoint outcomes that do not produce any specific data, similar to `void`.
    * **`UnitJsonConverter`**: A `JsonConverter` for the `Unit` struct, ensuring proper JSON serialization and deserialization.
    * **`MetadataKeys`**: A static class providing constant keys for commonly used `TransportMetadata` tags (e.g., `Logger`).

### Dependencies

* `Zentient.Results`: Core dependency for handling business logic results and error information.
