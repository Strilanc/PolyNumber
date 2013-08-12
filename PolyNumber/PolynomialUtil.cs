using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;

public static class PolynomialUtil {
    public static IReadOnlyList<BigInteger> RootsToCoefficients(this IEnumerable<int> roots) {
        return roots.Select(e => (BigInteger)e).RootsToCoefficients();
    }
    public static IReadOnlyList<BigInteger> RootsToCoefficients(this IEnumerable<BigInteger> roots) {
        var cached = roots.Select(e => -e).ToArray();
        return cached.Length.UpTo()
            .Select(i =>
                cached.CombinationsOfSize(i)
                .Select(f => f.Product())
                .Sum())
            .ToArray();
    }
}
