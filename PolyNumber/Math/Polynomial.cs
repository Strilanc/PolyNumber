using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Numerics;
using Strilanc.LinqToCollections;
using Strilanc.Value;
using Int = System.Numerics.BigInteger;

namespace Math {
    public static class Polynomial {
        public static Int EvaluateAt(this IntPolynomial<XTerm> polynomial, Int x) {
            return polynomial.Coefficients.Select(e => Int.Pow(x, (int)e.Key.XPower)*e.Value).Sum();
        }
        public static BigRational EvaluateAt(this IntPolynomial<XTerm> polynomial, BigRational x) {
            return polynomial.Coefficients.Select(e => BigRational.Pow(x, (int)e.Key.XPower) * e.Value).Sum();
        }
        public static Int EvaluateAt(this IntPolynomial<XYTerm> polynomial, Int x, Int y) {
            return polynomial.Coefficients.Select(e => Int.Pow(x, (int)e.Key.XPower) * Int.Pow(y, (int)e.Key.YPower) * e.Value).Sum();
        }
        public static BigRational EvaluateAt(this IntPolynomial<XYTerm> polynomial, BigRational x, BigRational y) {
            return polynomial.Coefficients.Select(e => BigRational.Pow(x, (int)e.Key.XPower) * BigRational.Pow(y, (int)e.Key.YPower) * e.Value).Sum();
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

            return XToThe(degreeOfSolution) - lowCoefficients.TimesPolynomialBasis();
        }
        public static IntPolynomial<XTerm> Derivative(this IntPolynomial<XTerm> polynomial) {
            return new IntPolynomial<XTerm>(
                polynomial.Coefficients
                .Where(e => e.Key.XPower > 0)
                .Select(e => new XTerm(e.Key.XPower - 1).KeyVal(e.Value*e.Key.XPower)));
        }
        public static IEnumerable<Range<BigRational>> ApproximateRoots(this IntPolynomial<XTerm> polynomial, BigRational epsilon) {
            return ApproximateRootsHelper(polynomial, epsilon).OrderBy(e => e.Min).Distinct();
        }
        private static IEnumerable<Range<BigRational>> ApproximateRootsHelper(this IntPolynomial<XTerm> polynomial, BigRational epsilon) {
            if (epsilon <= 0) throw new ArgumentOutOfRangeException("epsilon");
            if (!polynomial.Coefficients.Any()) throw new InvalidOperationException("Everything is a root...");

            var degree = polynomial.Degree();
            if (degree == 0) return new Range<BigRational>[0];

            if (degree == 1)
                return new Range<BigRational>[] {-polynomial.Coefficient(XToThe(0))/(BigRational)polynomial.Coefficient(XToThe(1))};

            var criticalRegions =
                polynomial
                .Derivative()
                .ApproximateRoots(epsilon)
                .ToArray();

            var lowerRoot = polynomial.BisectLower(criticalRegions.First().Min, epsilon);
            var upperRoot = polynomial.BisectUpper(criticalRegions.Last().Max, epsilon);
            var r0 = new[] {lowerRoot, upperRoot}.WhereHasValue();

            var r1 = criticalRegions.Where(r => BigRational.Abs(polynomial.EvaluateAt((r.Min + r.Max)/2)) < epsilon);

            var interRegions =
                criticalRegions
                .Window(2)
                .Select(e => new Range<BigRational>(e.First().Max, e.Last().Min, false, false))
                .ToArray();

            var r2 = interRegions.Select(e => polynomial.Bisect(e.Min, e.Max, epsilon)).WhereHasValue();

            return r0.Concat(r1).Concat(r2);
        }
        private static May<Range<BigRational>> BisectLower(this IntPolynomial<XTerm> polynomial, BigRational maxX, BigRational epsilon) {
            var maxCoef = polynomial.Coefficients.MaxBy(e => e.Key.XPower);
            var decreasingLimitSign = maxCoef.Value.Sign * (maxCoef.Key.XPower % 2 == 0 ? +1 : -1);

            var maxS = polynomial.EvaluateAt(maxX).Sign;
            if (maxS == decreasingLimitSign) return May.NoValue;
            if (maxS == 0) return (Range<BigRational>)maxX;

            var d = BigRational.One;
            while (true) {
                d *= 2;
                var minX = maxX - d;
                if (polynomial.EvaluateAt(minX).Sign != decreasingLimitSign) continue;
                return polynomial.Bisect(minX, maxX, epsilon);
            }
        }
        private static May<Range<BigRational>> BisectUpper(this IntPolynomial<XTerm> polynomial, BigRational minX, BigRational epsilon) {
            var increasingLimitSign = polynomial.Coefficients.MaxBy(e => e.Key.XPower).Value.Sign;

            var minS = polynomial.EvaluateAt(minX).Sign;
            if (minS == increasingLimitSign) return May.NoValue;
            if (minS == 0) return (Range<BigRational>)minX;

            var d = BigRational.One;
            while (true) {
                d *= 2;
                var maxX = minX + d;
                if (polynomial.EvaluateAt(maxX).Sign != increasingLimitSign) continue;
                return polynomial.Bisect(minX, maxX, epsilon);
            }
        }
        public static IntPolynomial<XTerm> AddRoots(this IntPolynomial<XTerm> value1, IntPolynomial<XTerm> value2) {
            var degree = value1.Degree() + value2.Degree();
            return new IntPolynomial<XTerm>(
                from d in degree.RangeInclusive()
                let p = XToThe(d)
                let s = (from d2 in d.RangeInclusive()
                         select value1.Coefficient(XToThe(d2))*value2.Coefficient(XToThe(d - d2))
                        ).Sum()
                select p.KeyVal(s));
        }
        private static May<Range<BigRational>> Bisect(this IntPolynomial<XTerm> polynomial, BigRational minX, BigRational maxX, BigRational epsilon) {
            var minS = polynomial.EvaluateAt(minX).Sign;
            var maxS = polynomial.EvaluateAt(maxX).Sign;
            if (minS == 0) return (Range<BigRational>)minX;
            if (maxS == 0) return (Range<BigRational>)maxX;
            
            while (maxX - minX > epsilon) {
                var x = (minX + maxX)/2;
                var y = polynomial.EvaluateAt(x);
                if (y == 0) return (Range<BigRational>)x;
                if (BigRational.Abs(y) < epsilon) return new Range<BigRational>(minX, maxX, false, false);
                if (y.Sign == minS) {
                    minX = x;
                } else {
                    maxX = x;
                }
            }

            return May.NoValue;
        }
    }
}

