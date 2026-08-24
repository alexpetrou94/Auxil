namespace Auxil.CodeGeneration.Common.Builders.Scopes;

/// <summary>
/// Marker interface for typed scope handles. Every scope's parent is
/// carried as a generic type argument, so the compiler knows the full
/// chain statically and only valid next steps are offered.
/// </summary>
[PublicAPI]
public interface IScope;
