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
using Frac = Numerics.BigRational;
using Int = System.Numerics.BigInteger;

public static class NumberUtil {
    public static Int Lcm(this Int value1, Int value2) {
        return value1 * value2 / Int.GreatestCommonDivisor(value1, value2);
    }
    public static Int Lcm(this IEnumerable<Int> values) {
        return values.Aggregate(Int.One, Lcm);
    }
    public static Int LeastCommonDenominator(this IEnumerable<Frac> values) {
        return values.Where(e => e != 0).Select(e => e.Denominator).Lcm();
    }
}
