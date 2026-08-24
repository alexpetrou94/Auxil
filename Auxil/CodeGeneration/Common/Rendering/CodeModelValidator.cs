using Auxil.CodeGeneration.Common.Builders;

namespace Auxil.CodeGeneration.Common.Rendering;

internal static class CodeModelValidator
{
    public static void Validate(CodeFile file)
    {
        foreach (CodeNode child in file.Children)
        {
            ValidateNode(child);
        }
    }

    private static void ValidateNode(CodeNode node)
    {
        switch (node)
        {
            case CodeClass cls:
                foreach (CodeNode member in cls.Members)
                {
                    ValidateNode(member);
                }

                break;
            case CodeInterface iface:
                foreach (CodeNode member in iface.Members)
                {
                    ValidateNode(member);
                }

                break;
            case CodeNamespace namespaceNode:
                foreach (CodeNode child in namespaceNode.Children)
                {
                    ValidateNode(child);
                }

                break;
            case CodeMethod { Body: not null } method:
                ValidateStatements(method.Body.Statements);
                break;
            case CodeConstructor { Body: not null } constructor:
                ValidateStatements(constructor.Body.Statements);
                break;
            case CodeFunction { Body: not null } function:
                ValidateStatements(function.Body.Statements);
                break;
            case CodeProperty property:
                if (property.GetterBody is not null)
                {
                    ValidateStatements(property.GetterBody.Statements);
                }

                if (property.SetterBody is not null)
                {
                    ValidateStatements(property.SetterBody.Statements);
                }

                break;
        }
    }

    private static void ValidateStatements(IEnumerable<CodeStatement> statements)
    {
        foreach (CodeStatement statement in statements)
        {
            switch (statement)
            {
                case CodeIf ifStatement:
                    ValidateStatements(ifStatement.Then);
                    foreach (CodeElseIf branch in ifStatement.ElseIfs)
                    {
                        ValidateStatements(branch.Body);
                    }

                    ValidateStatements(ifStatement.Else);
                    break;
                case CodeForEach forEach:
                    ValidateStatements(forEach.Body);
                    break;
                case CodeWhile whileStatement:
                    ValidateStatements(whileStatement.Body);
                    break;
                case CodeDoWhile doWhile:
                    ValidateStatements(doWhile.Body);
                    break;
                case CodeFor forStatement:
                    ValidateStatements(forStatement.Body);
                    break;
                case CodeRawBlock rawBlock:
                    ValidateStatements(rawBlock.Body);
                    break;
                case CodeMatch match:
                    foreach (CodeMatchArm arm in match.Arms)
                    {
                        ValidateStatements(arm.Body);
                    }

                    break;
                case CodeTry tryStatement:
                    if (tryStatement.Catches.Count == 0 && tryStatement.Finally.Count == 0)
                    {
                        throw new CodeBuilderException("A try statement requires at least one catch or finally block.");
                    }

                    ValidateStatements(tryStatement.Body);
                    foreach (CodeCatch catchBlock in tryStatement.Catches)
                    {
                        ValidateStatements(catchBlock.Body);
                    }

                    ValidateStatements(tryStatement.Finally);
                    break;
                case CodeNestedFunction nestedFunction when nestedFunction.Body is not null:
                    ValidateStatements(nestedFunction.Body.Statements);
                    break;
                case CodeObjectLiteral objectLiteral:
                    foreach (CodeObjectMember member in objectLiteral.Members)
                    {
                        if (member.Object is not null)
                        {
                            ValidateStatements([member.Object]);
                        }

                        if (member.Function?.Body is not null)
                        {
                            ValidateStatements(member.Function.Body.Statements);
                        }
                    }

                    break;
            }
        }
    }
}
