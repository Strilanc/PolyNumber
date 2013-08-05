using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;

public static class PolynomialUtil {
    public static IReadOnlyList<BigInteger> RootsToCoefficients(this IReadOnlyList<int> roots) {
        return roots.Select(e => (BigInteger)e).RootsToCoefficients();
    }
    public static IReadOnlyList<BigInteger> RootsToCoefficients(this IReadOnlyList<BigInteger> roots) {
        return roots.Count.UpTo()
            .Select(i => 
                roots.CombinationsOfSize(i)
                .Select(f => f.Product())
                .Sum())
            .ToArray();
    }
}
