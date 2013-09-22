using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using MoreLinq;
using Numerics;
using Strilanc.Exceptions;
using Strilanc.LinqToCollections;
using System.Linq;
using Strilanc.Value;

public static class CollectionUtil {
    public static IEnumerable<BigInteger> Range(this BigInteger count) {
        return Naturals.Take(count);
    }
    public static T Max<T>(this T value1, T value2) where T : IComparable<T> {
        return value1.CompareTo(value2) >= 0 ? value1 : value2;
    }
    public static T Min<T>(this T value1, T value2) where T : IComparable<T> {
        return value1.CompareTo(value2) <= 0 ? value1 : value2;
    }
    public static IEnumerable<T> Take<T>(this IEnumerable<T> sequence, BigInteger count) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (count <= 0) yield break;
        foreach (var e in sequence) {
            yield return e;
            count -= 1;
            if (count == 0) yield break;
        }
    }
    public static IEnumerable<BigInteger> Naturals {
        get {
            for (var i = BigInteger.Zero;; i++) {
                yield return i;
            }
        }
    }

    public static IReadOnlyList<IReadOnlyList<T>> Transpose<T>(this IReadOnlyList<IReadOnlyList<T>> rectangularListOfLists) {
        if (rectangularListOfLists == null) throw new ArgumentNullException("rectangularListOfLists");

        return new AnonymousReadOnlyList<IReadOnlyList<T>>(
            () => rectangularListOfLists.MayFirst().Select(e => e.Count).ElseDefault(),
            i => new AnonymousReadOnlyList<T>(
                () => rectangularListOfLists.Count,
                j => rectangularListOfLists[j][i]));
    }

    public static BigRational Sum(this IEnumerable<BigRational> sequence) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        return sequence.Aggregate(BigRational.Zero, (a, e) => a + e);
    }

    public static May<TVal> MayGetValue<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dictionary, TKey key) {
        TVal val;
        if (!dictionary.TryGetValue(key, out val)) return May.NoValue;
        return val;
    }
    public static Dictionary<TKey, TVal> ToDictionary<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> sequence) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        return sequence.ToDictionary(e => e.Key, e => e.Value);
    }
    public static KeyValuePair<TKey, TVal> KeyVal<TKey, TVal>(this TKey key, TVal val) {
        return new KeyValuePair<TKey, TVal>(key, val);
    }
    public static IEnumerable<KeyValuePair<TKey, TVal>> KeyReduce<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> sequence, Func<TVal, TVal, TVal> duplicateKeyValueReducer) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (duplicateKeyValueReducer == null) throw new ArgumentNullException("duplicateKeyValueReducer");
        return sequence
            .GroupBy(e => e.Key)
            .SelectKeyValue(
                g => g.Key,
                g => g.Select(e => e.Value).Aggregate(duplicateKeyValueReducer));
    }

    public static bool Contains<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dictionary, KeyValuePair<TKey, TVal> keyValuePair) {
        if (dictionary == null) throw new ArgumentNullException("dictionary");
        return dictionary.MayGetValue(keyValuePair.Key) == keyValuePair.Value.Maybe();
    }
    public static IEnumerable<KeyValuePair<TKey, TVal>> SelectKeyValue<TKey, TVal>(this IEnumerable<TKey> keys, Func<TKey, TVal> valueSelector) {
        return keys.Select(e => e.KeyVal(valueSelector(e)));
    }
    public static IEnumerable<KeyValuePair<TKey, TVal>> SelectKeyValue<TItem, TKey, TVal>(this IEnumerable<TItem> sequence, Func<TItem, TKey> keySelector, Func<TItem, TVal> valueSelector) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (keySelector == null) throw new ArgumentNullException("keySelector");
        if (valueSelector == null) throw new ArgumentNullException("valueSelector");
        return sequence.Select(e => keySelector(e).KeyVal(valueSelector(e)));
    }
    public static bool DictionaryEqual<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dictionary1, IReadOnlyDictionary<TKey, TVal> dictionary2) {
        if (dictionary1 == null) throw new ArgumentNullException("dictionary1");
        if (dictionary2 == null) throw new ArgumentNullException("dictionary2");
        return dictionary1.Count == dictionary2.Count
               && dictionary1.All(dictionary2.Contains);
    }
}