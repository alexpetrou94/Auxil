using Auxil.CodeGeneration.Common.Builders;
using Auxil.CodeGeneration.Common.Builders.Scopes;
using Auxil.CodeGeneration.Common.Languages;
using Auxil.CodeGeneration.Common.Model;

CodeBuilder<GDScript> code = new();

FileScope<GDScript> file = code.File("player.gd");
file
    .Doc("The player character.")
    .ClassName("Player")
    .Extends("Node")
    .Field("health", CodeTypes.Int, "100").Annotate("export").EndField()
    .Signal("hit").Parameter("damage", CodeTypes.Int).EndSignal()
    .Method("take_damage")
    .Parameter("amount", CodeTypes.Int)
    .Returns(CodeTypes.Bool)
    .Doc("Reduces health; returns true if still alive.")
    .Body(b => b
        .If("health <= 0", i => i.Return("false"))
        .Line("health -= amount")
        .Return("health > 0"))
    .EndMethod()
    .EndFile();

Console.WriteLine("=== GDScript ===");
Console.WriteLine(code.Render());
Console.WriteLine();

CodeBuilder<CSharp> cs = new();
FileScope<CSharp> csFile = cs.File("repository.cs");
csFile
    .Import("System.Collections.Generic")
    .Namespace("UPhoricStudio.Game")
    .Class("Repository")
    .Public()
    .Generic("T", "class")
    .Implements("IRepository<T>")
    .Property("Items", "List<T>").Public().EndProperty()
    .Method("Add")
    .Public()
    .Parameter("item", "T")
    .Body(b => b
        .Line("_items.Add(item);")
        .If("_items.Count > 100", i => i.Throw("new InvalidOperationException(\"too many\")")))
    .EndMethod()
    .EndClass()
    .EndNamespace()
    .EndFile();

Console.WriteLine("=== Native C# with namespace, property, generics ===");
Console.WriteLine(cs.Render());
