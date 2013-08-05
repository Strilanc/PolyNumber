using System;
using System.Collections.Generic;
using Strilanc.LinqToCollections;
using System.Linq;

public static class CollectionUtil {
    public static IEnumerable<T[]> CombinationsOfSize<T>(this IReadOnlyList<T> items, int size) {
        if (items == null) throw new ArgumentNullException("items");
        if (size < 0) throw new ArgumentOutOfRangeException("size", "size < 0");
        if (size == 0) return new[] {new T[0]};
        if (size > items.Count) return new T[0][];
        return from i in items.Count.Range()
               let head = items[i]
               from tail in items.Skip(i + 1).CombinationsOfSize(size - 1)
               select new[] {head}.Concat(tail).ToArray();
    }
}