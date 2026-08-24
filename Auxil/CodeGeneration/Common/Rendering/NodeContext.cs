namespace Auxil.CodeGeneration.Common.Rendering;

/// <summary>
/// Where a node appears in the model; context policies (file-level,
/// class-member, interface-member restrictions) are applied per context.
/// </summary>
public enum NodeContext
{
    /// <summary>A top-level child of the file.</summary>
    FileLevel,

    /// <summary>A member of a class.</summary>
    ClassMember,

    /// <summary>A member of an interface.</summary>
    InterfaceMember,

    /// <summary>A statement inside a body.</summary>
    Statement
}
