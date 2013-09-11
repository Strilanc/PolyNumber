using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Strilanc.LinqToCollections;
using Strilanc.Value;
using Int = System.Numerics.BigInteger;

namespace Math {
    public static class Polynomial {
        public static Int EvaluateAt(this IntPolynomial<XTerm> polynomial, Int x) {
            return polynomial.Coefficients.Select(e => Int.Pow(x, (int)e.Key.XPower)*e.Value).Sum();
        }

        public static IntPolynomial<XTerm> FromRoots(IEnumerable<Int> roots) {
            if (roots == null) throw new ArgumentNullException("roots");
            return roots.Select(e => FromBigEndianCoefficients(1, -e)).Aggregate(FromBigEndianCoefficients(1), (a, e) => a*e);
        }
        public static IntPolynomial<XTerm> FromRoots(params Int[] roots) {
            if (roots == null) throw new ArgumentNullException("roots");
            return FromRoots(roots.AsEnumerable());
        }
        public static IntPolynomial<XTerm> FromBigEndianCoefficients(params Int[] coefs) {
            if (coefs == null) throw new ArgumentNullException("coefs");
            return new IntPolynomial<XTerm>(coefs.Reverse().Select((e, i) => new XTerm(i).KeyVal(e)));
        }
        public static IntPolynomial<XYTerm> FromBigEndianCoefficientsOverVar1Of2(params Int[] coefs) {
            if (coefs == null) throw new ArgumentNullException("coefs");
            return new IntPolynomial<XYTerm>(coefs.Reverse().Select((e, i) => new XYTerm(i, 0).KeyVal(e)));
        }
        public static IntPolynomial<XYTerm> FromBigEndianCoefficientsOverVar2Of2(params Int[] coefs) {
            if (coefs == null) throw new ArgumentNullException("coefs");
            return new IntPolynomial<XYTerm>(coefs.Reverse().Select((e, i) => new XYTerm(0, i).KeyVal(e)));
        }
        public static IntPolynomial<TTerm> ToPolynomial<TTerm>(this IEnumerable<KeyValuePair<TTerm, Int>> terms) where TTerm : ITerm<TTerm> {
            if (terms == null) throw new ArgumentNullException("terms");
            return new IntPolynomial<TTerm>(terms);
        }
        public static IntPolynomial<XYTerm> ToPolynomialOverVariable1Of2(this IntPolynomial<XTerm> polynomial) {
            return polynomial.Coefficients.Select(e => new XYTerm(e.Key.XPower, 0).KeyVal(e.Value)).ToPolynomial();
        }
        public static IntPolynomial<XYTerm> ToPolynomialOverVariable2Of2(this IntPolynomial<XTerm> polynomial) {
            return polynomial.Coefficients.Select(e => new XYTerm(0, e.Key.XPower).KeyVal(e.Value)).ToPolynomial();
        }
        public static IEnumerable<XTerm> Basis { get { return CollectionUtil.Naturals.Select(XToThe); } }
        public static XTerm XToThe(Int power) {
            return new XTerm(power);
        }
        public static IntPolynomial<XTerm> TimesPolynomialBasis(this IEnumerable<Int> coefficients) {
            return coefficients.Zip(Basis, (c, x) => x.KeyVal(c)).ToPolynomial();
        }
        public static Int Degree(this IntPolynomial<XTerm> polynomial) {
            return polynomial.Coefficients.Select(e => e.Key.XPower).MayMax().ElseDefault();
        }

        public static bool DividesScaled(this IntPolynomial<XTerm> denominator, IntPolynomial<XTerm> numerator) {
            if (denominator == 0) return false;
            if (denominator.Degree() == 0) return true;

            var remainder = numerator;

            while (true) {
                var d1 = remainder.Degree();
                var d2 = denominator.Degree();
                if (d1 < d2) break;

                var nt = remainder.Coefficient(d1);
                var dt = denominator.Coefficient(d2);

                var gcd = Int.GreatestCommonDivisor(nt, dt);
                remainder = remainder*(dt/gcd) - denominator*XToThe(d1 - d2)*(nt/gcd);

                if (remainder.Degree() == d1) throw new Exception();
            }

            return remainder == 0;
        }
        private static IEnumerable<IntPolynomial<XTerm>> RewritePolyBasisUsing(XTerm termToRewrite,
                                                                               IntPolynomial<XTerm> rewrittenTerm) {
            var X = new XTerm(1);
            var r = rewrittenTerm - termToRewrite;

            var cur = (IntPolynomial<XTerm>)1;
            yield return cur;

            for (var i = 1; i < termToRewrite.XPower; i++) {
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
        private static IEnumerable<IntPolynomial<XTerm>> RewritePolyBasisUsing(IntPolynomial<XTerm> poly) {
            var termToRewrite = poly.Coefficients.MaxBy(e => e.Key.XPower);
            if (termToRewrite.Value != 1) throw new NotImplementedException();
            var rewrittenTerm = termToRewrite - poly;
            return RewritePolyBasisUsing(termToRewrite.Key, rewrittenTerm);
        }

        public static IntPolynomial<XTerm> MultiplyRoots(this IntPolynomial<XTerm> poly1,
                                                         IntPolynomial<XTerm> poly2) {
            var deg1 = poly1.Degree();
            var deg2 = poly2.Degree();

            var maxDegreeOfResult = deg1*deg2;
            if (maxDegreeOfResult == 0) return 1;

            var rewrittenPowers1 = RewritePolyBasisUsing(poly1).Select(p => p.ToPolynomialOverVariable1Of2());
            var rewrittenPowers2 = RewritePolyBasisUsing(poly2).Select(p => p.ToPolynomialOverVariable2Of2());
            var rewrittenPowerPairs =
                rewrittenPowers1.Zip(rewrittenPowers2,
                                     IntPolynomial<XYTerm>.Multiply)
                                .Take(maxDegreeOfResult + 1);

            var linearSystem = IntMatrix.FromColumns(
                from col in rewrittenPowerPairs
                select from row in maxDegreeOfResult.Range()
                       let exponentForX = row/deg2
                       let exponentForY = row%deg2
                       select col.Coefficient(new XYTerm(exponentForX, exponentForY)));

            var solvedSystem = linearSystem.Reduced();

            var degreeOfSolution = solvedSystem.Rows.Count(row => row.Any(cell => cell != 0));

            var lowCoefficients = solvedSystem.Columns[degreeOfSolution].Take(degreeOfSolution);

            return Polynomial.XToThe(degreeOfSolution) - lowCoefficients.TimesPolynomialBasis();
        }
    }
}

