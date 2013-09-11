using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;
using Strilanc.Value;
using MoreLinq;

public static class PolynomialUtil {
    public static IReadOnlyList<BigInteger> RootsToCoefficients(this IEnumerable<BigInteger> roots) {
        var cached = roots.Select(e => -e).ToArray();
        return cached.Length.RangeInclusive()
            .Select(i =>
                cached.Choose(i)
                .Select(f => f.Product())
                .Sum())
            .ToArray();
    }

    public static IEnumerable<IntegerPolynomial<OneVariablePolynomialTerm>> RewritePolyBasisUsing(OneVariablePolynomialTerm termToRewrite,
                                                                                                  IntegerPolynomial<OneVariablePolynomialTerm> rewrittenTerm) {

        var X = new OneVariablePolynomialTerm(1);
        var r = rewrittenTerm - termToRewrite;

        var cur = (IntegerPolynomial<OneVariablePolynomialTerm>)1;
        yield return cur;

        for (var i = 1; i < termToRewrite.Power; i++) {
            cur *= X;
            yield return cur;
        }
        while (true) {
            cur = cur*X;
            var factor = cur.Coefficient(termToRewrite);
            cur += r*factor;
            yield return cur;
        }
    }
    public static IEnumerable<IntegerPolynomial<OneVariablePolynomialTerm>> RewritePolyBasisUsing(IntegerPolynomial<OneVariablePolynomialTerm> poly) {
        var termToRewrite = poly.Coefficients.MaxBy(e => e.Key.Power);
        if (termToRewrite.Value != 1) throw new NotImplementedException();
        var rewrittenTerm = termToRewrite - poly;
        return RewritePolyBasisUsing(termToRewrite.Key, rewrittenTerm);
    }

    public static IntegerPolynomial<OneVariablePolynomialTerm> M(IntegerPolynomial<OneVariablePolynomialTerm> poly1, IntegerPolynomial<OneVariablePolynomialTerm> poly2) {
        var deg1 = poly1.Degree();
        var deg2 = poly2.Degree();
        
        var maxDegreeOfResult = deg1 * deg2;
        if (maxDegreeOfResult == 0) return 1;

        var rewrittenPowers1 = RewritePolyBasisUsing(poly1).Select(p => p.ToPolynomialOverVariable1Of2());
        var rewrittenPowers2 = RewritePolyBasisUsing(poly2).Select(p => p.ToPolynomialOverVariable2Of2());
        var rewrittenPowerPairs = 
            rewrittenPowers1.Zip(rewrittenPowers2,
                IntegerPolynomial<TwoVariablePolynomialTerm>.Multiply)
            .Take(maxDegreeOfResult + 1);

        var linearSystem = IntegerMatrix.FromColumns(
            from col in rewrittenPowerPairs
            select from row in maxDegreeOfResult.Range()
                   let exponentForX = row/deg2
                   let exponentForY = row%deg2
                   select col.Coefficient(new TwoVariablePolynomialTerm(exponentForX, exponentForY)));

        var solvedSystem = linearSystem.Reduced();

        var degreeOfSolution = solvedSystem.Rows.Count(row => row.Any(cell => cell != 0));

        var lowCoefficients = solvedSystem.Columns[degreeOfSolution].Take(degreeOfSolution);

        return Polynomial.XToThe(degreeOfSolution) - lowCoefficients.TimesPolynomialBasis();
    }
}
