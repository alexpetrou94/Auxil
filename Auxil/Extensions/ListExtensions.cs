using System.Collections;
using System.Text;

namespace Auxil.Extensions;

/// <summary>Provides convenience members for working with lists, collections, and other enumerables.</summary>
[PublicAPI]
public static class ListExtensions
{
    /// <summary>Appends all arguments to the list.</summary>
    /// <param name="list">The list to modify.</param>
    /// <param name="args">The items to append.</param>
    public static void Add<T>(this List<T> list, params T[] args)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(args);

        list.AddRange(args);
    }

    /// <summary>Joins the values with the given separator.</summary>
    /// <param name="values">The values to join.</param>
    /// <param name="separator">The separator between values.</param>
    public static string Join(this IEnumerable<string> values, string separator = " ")
        => string.Join(separator, values);

    /// <summary>Joins the values with the given separator character.</summary>
    /// <param name="values">The values to join.</param>
    /// <param name="separator">The separator character.</param>
    public static string Join(this IEnumerable<string> values, char separator)
        => string.Join(separator.ToString(), values);

    /// <summary>Whether the index is within the list bounds.</summary>
    /// <param name="list">The list to inspect.</param>
    /// <param name="index">The index to check.</param>
    public static bool IsIndexValid(this IList list, int index)
    {
        ArgumentNullException.ThrowIfNull(list);

        return index >= 0 && index < list.Count;
    }

    /// <summary>Replaces each element with the result of the transform function.</summary>
    /// <param name="list">The list to modify in place.</param>
    /// <param name="modifyFunc">The transform applied to each element.</param>
    public static void ModifyElements<T>(this IList<T> list, Func<T, T> modifyFunc)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(modifyFunc);

        for (int i = 0; i < list.Count; i++)
        {
            list[i] = modifyFunc(list[i]);
        }
    }

    /// <summary>Renders the collection as a readable string, one item per line when requested.</summary>
    /// <param name="collection">The collection to render.</param>
    /// <param name="verticalPrint">Whether items are laid out vertically with indentation.</param>
    public static string PrintContents(this IEnumerable collection, bool verticalPrint = false)
        => collection.PrintContents(2, verticalPrint);

    /// <summary>Renders the collection as a readable string with the given indentation.</summary>
    /// <param name="collection">The collection to render.</param>
    /// <param name="indent">The indentation width used for nested collections.</param>
    /// <param name="verticalPrint">Whether items are laid out vertically with indentation.</param>
    public static string PrintContents(this IEnumerable collection, int indent, bool verticalPrint = false)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection is IDictionary dictionary)
        {
            return PrintDictionary(dictionary, indent, verticalPrint);
        }

        if (collection is IList list)
        {
            return PrintList(list, indent, verticalPrint);
        }

        return collection.ToString() ?? string.Empty;
    }

    /// <summary>Whether the collection is <c>null</c> or contains no elements.</summary>
    /// <param name="collection">The collection to check.</param>
    public static bool IsNullOrEmpty<T>(this ICollection<T>? collection)
        => collection is null || collection.Count == 0;

    /// <summary>Removes the element at the index when it is in bounds; otherwise does nothing.</summary>
    /// <param name="list">The list to modify.</param>
    /// <param name="index">The index to remove.</param>
    public static void RemoveIfExists(this IList list, int index)
    {
        if (index >= 0 && index < list.Count)
        {
            list.RemoveAt(index);
        }
    }

    /// <summary>Appends the item when the condition is true.</summary>
    /// <param name="list">The list to modify.</param>
    /// <param name="item">The item to append.</param>
    /// <param name="condition">Whether to append.</param>
    public static void AddIf<T>(this IList list, T item, bool condition)
    {
        if (condition)
        {
            list.Add(item);
        }
    }

    /// <summary>Removes the element at the index and returns it.</summary>
    /// <param name="list">The list to modify.</param>
    /// <param name="index">The index of the element to remove.</param>
    public static T GetAndRemove<T>(this IList<T> list, int index)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, list.Count);

        T item = list[index];
        list.RemoveAt(index);

        return item;
    }

    /// <summary>Replaces all elements whose exact runtime type matches the given type.</summary>
    /// <param name="list">The list to modify.</param>
    /// <param name="type">The exact runtime type to match.</param>
    /// <param name="newItem">The replacement item.</param>
    public static void ReplaceAllIfType<T>(this IList list, Type type, T newItem)
    {
        if (list.Count == 0)
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            object? item = list[i];

            if (item?.GetType() == type)
            {
                list[i] = newItem;
            }
        }
    }

    /// <summary>Returns a random element from the list.</summary>
    /// <param name="list">The list to draw from.</param>
    /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
    public static T RandomItem<T>(this IList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);

        if (list.Count == 0)
        {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        return list[Random.Shared.Next(list.Count)];
    }

    extension(ICollection collection)
    {
        /// <summary>Whether the collection contains no elements.</summary>
        public bool IsEmpty => collection.Count == 0;
    }

    private static string PrintList(IList list, int indent, bool verticalPrint)
    {
        if (list.Count == 0)
        {
            return "[ ]";
        }

        if (list.Count == 1)
        {
            return $"[ {GetItemString(list[0], indent, verticalPrint)} ]";
        }

        StringBuilder sb = new();
        string indentText = " ".Repeat(indent);
        string separator = verticalPrint ? ",\n" : ", ";

        sb.Append(verticalPrint ? "[\n" : "[");

        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(separator);
            }

            if (verticalPrint)
            {
                sb.Append(indentText);
            }

            sb.Append(GetItemString(list[i], indent, verticalPrint));
        }

        sb.Append(verticalPrint ? $"\n{" ".Repeat(Math.Max(0, indent - 2))}]" : "]");

        return sb.ToString();
    }

    private static string PrintDictionary(IDictionary dictionary, int indent, bool verticalPrint)
    {
        if (dictionary.IsEmpty)
        {
            return "{ }";
        }

        StringBuilder sb = new();
        string indentText = " ".Repeat(indent);
        string separator = verticalPrint ? ",\n" : ", ";

        sb.Append(verticalPrint ? "{\n" : "{ ");

        bool isFirst = true;

        foreach (object key in dictionary.Keys)
        {
            if (!isFirst)
            {
                sb.Append(separator);
            }

            isFirst = false;

            if (verticalPrint)
            {
                sb.Append(indentText);
            }

            object? value = dictionary[key];

            sb.Append(GetItemString(key, indent, verticalPrint))
                .Append(": ")
                .Append(GetItemString(value, indent, verticalPrint));
        }

        if (verticalPrint)
        {
            sb.Append($"\n{" ".Repeat(Math.Max(0, indent - 2))}}}");
        }

        else
        {
            sb.Append(" }");
        }

        return sb.ToString();
    }

    private static string GetItemString(object? item, int indent, bool verticalPrint)
    {
        int nestedIndentText = indent + 2;

        return item switch
        {
            IDictionary dictionary => PrintDictionary(dictionary, nestedIndentText, verticalPrint),
            IList list => PrintList(list, nestedIndentText, verticalPrint),
            _ => item?.ToString() ?? "NULL"
        };
    }
}
