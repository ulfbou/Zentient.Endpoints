using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Test for a mapper inherently involves coupling to multiple types/namespaces it maps between.")]
[assembly: SuppressMessage("Documentation", "CA1591:Missing XML comment for publicly visible type or member", Justification = "Test code does not require XML documentation comments.")]
[assembly: SuppressMessage("Reliability", "CA2007:Do not directly await a Task without calling ConfigureAwait", Justification = "ConfigureAwait is not required in test or ASP.NET code.")]
