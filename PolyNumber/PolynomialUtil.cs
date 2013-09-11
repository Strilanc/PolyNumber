using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;
using Strilanc.Value;
using MoreLinq;

public static class PolynomialUtil {
    public static IReadOnlyList<BigInteger> RootsToCoefficients(this IEnumerable<int> roots) {
        return roots.Select(e => (BigInteger)e).RootsToCoefficients();
    }
    public static IReadOnlyList<BigInteger> RootsToCoefficients(this IEnumerable<BigInteger> roots) {
        var cached = roots.Select(e => -e).ToArray();
        return cached.Length.RangeInclusive()
            .Select(i =>
                cached.Choose(i)
                .Select(f => f.Product())
                .Sum())
            .ToArray();
    }
    public static IntegerPolynomial<OneVariablePolynomialTerm> M(IntegerPolynomial<OneVariablePolynomialTerm> poly1, IntegerPolynomial<OneVariablePolynomialTerm> poly2) {
        if (poly1 == 0 || poly2 == 0) return 0;

        var m1 = poly1.Coefficients.MaxBy(e => e.Key.Power);
        var em1 = m1 - poly1;

        var m2 = poly2.Coefficients.MaxBy(e => e.Key.Power);
        var em2 = m2 - poly2;




        //x^|poly1| = -(poly1 dot POLY)
        //x^|coefs2| = -(coefs2 dot POLY)
        throw new Exception();
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
    public static Dictionary<TKey, TVal> ToDictionary<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> sequence, Func<TVal, TVal, TVal> duplicateKeyValueReducer) {
        if (sequence == null) throw new ArgumentNullException("sequence");
        if (duplicateKeyValueReducer == null) throw new ArgumentNullException("duplicateKeyValueReducer");
        return sequence
            .GroupBy(e => e.Key)
            .ToDictionary(
                g => g.Key, 
                g => g.Select(e => e.Value).Aggregate(duplicateKeyValueReducer));
    }
    public static bool Contains<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dictionary, KeyValuePair<TKey, TVal> keyValuePair) {
        if (dictionary == null) throw new ArgumentNullException("dictionary");
        return dictionary.MayGetValue(keyValuePair.Key) == keyValuePair.Value.Maybe();
    }
    public static IEnumerable<KeyValuePair<TKey, TVal>> SelectValue<TKey, TVal>(this IEnumerable<TKey> keys, Func<TKey, TVal> valueSelector) {
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
