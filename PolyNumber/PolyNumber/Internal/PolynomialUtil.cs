using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Strilanc.LinqToCollections;
using Strilanc.Value;
using Frac = Numerics.BigRational;
using Int = System.Numerics.BigInteger;

namespace Strilanc.PolyNumber.Internal {
    internal static class Polynomial {
        public static Frac EvaluateAt(this Polynomial<XTerm> polynomial, Frac x) {
            return polynomial.Coefficients.Select(e => Frac.Pow(x, (int)e.Key.XPower) * e.Value).Sum();
        }
        public static Frac EvaluateAt(this Polynomial<XYTerm> polynomial, Frac x, Frac y) {
            return polynomial.Coefficients.Select(e => Frac.Pow(x, (int)e.Key.XPower) * Frac.Pow(y, (int)e.Key.YPower) * e.Value).Sum();
        }

        public static Polynomial<XTerm> FromRoots(IEnumerable<Frac> roots) {
            if (roots == null) throw new ArgumentNullException("roots");
            return roots.Select(e => FromBigEndianCoefficients(1, -e)).Aggregate(FromBigEndianCoefficients(1), (a, e) => a*e);
        }
        public static Polynomial<XTerm> FromRoots(params Frac[] roots) {
            if (roots == null) throw new ArgumentNullException("roots");
            return FromRoots(roots.AsEnumerable());
        }
        public static Polynomial<XTerm> FromBigEndianCoefficients(params Frac[] coefs) {
            if (coefs == null) throw new ArgumentNullException("coefs");
            return new Polynomial<XTerm>(coefs.Reverse().Select((e, i) => new XTerm(i).KeyVal(e)));
        }
        public static Polynomial<XYTerm> FromBigEndianCoefficientsOverVar1Of2(params Frac[] coefs) {
            if (coefs == null) throw new ArgumentNullException("coefs");
            return new Polynomial<XYTerm>(coefs.Reverse().Select((e, i) => new XYTerm(i, 0).KeyVal(e)));
        }
        public static Polynomial<XYTerm> FromBigEndianCoefficientsOverVar2Of2(params Frac[] coefs) {
            if (coefs == null) throw new ArgumentNullException("coefs");
            return new Polynomial<XYTerm>(coefs.Reverse().Select((e, i) => new XYTerm(0, i).KeyVal(e)));
        }
        public static Polynomial<TTerm> ToPolynomial<TTerm>(this IEnumerable<KeyValuePair<TTerm, Frac>> terms) where TTerm : ITerm<TTerm> {
            if (terms == null) throw new ArgumentNullException("terms");
            return new Polynomial<TTerm>(terms);
        }
        public static Polynomial<XYTerm> ToPolynomialOverVariable1Of2(this Polynomial<XTerm> polynomial) {
            return polynomial.Coefficients.Select(e => new XYTerm(e.Key.XPower, 0).KeyVal(e.Value)).ToPolynomial();
        }
        public static Polynomial<XYTerm> ToPolynomialOverVariable2Of2(this Polynomial<XTerm> polynomial) {
            return polynomial.Coefficients.Select(e => new XYTerm(0, e.Key.XPower).KeyVal(e.Value)).ToPolynomial();
        }
        public static IEnumerable<XTerm> Basis { get { return CollectionUtil.Naturals.Select(XToThe); } }
        public static XTerm XToThe(Int power) {
            return new XTerm(power);
        }
        public static Polynomial<XTerm> TimesPolynomialBasis(this IEnumerable<Frac> coefficients) {
            return coefficients.Zip(Basis, (c, x) => x.KeyVal(c)).ToPolynomial();
        }
        public static Int Degree(this Polynomial<XTerm> polynomial) {
            return polynomial.Coefficients.Select(e => e.Key.XPower).MayMax().ElseDefault();
        }

        public static bool Divides(this Polynomial<XTerm> denominator, Polynomial<XTerm> numerator) {
            if (denominator == 0) return false;
            if (denominator.Degree() == 0) return true;

            var remainder = numerator;

            while (true) {
                var d1 = remainder.Degree();
                var d2 = denominator.Degree();
                if (d1 < d2) break;

                var nt = remainder.Coefficient(d1);
                var dt = denominator.Coefficient(d2);

                remainder -= denominator * XToThe(d1 - d2) * (nt / dt);

                if (remainder.Degree() == d1) throw new Exception();
            }

            return remainder == 0;
        }
        private static IEnumerable<Polynomial<XTerm>> RewritePolyBasisUsing(XTerm termToRewrite,
                                                                                    Polynomial<XTerm> rewrittenTerm) {
            var X = new XTerm(1);
            var r = rewrittenTerm - termToRewrite;

            var cur = (Polynomial<XTerm>)1;
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
        public static Polynomial<XTerm> ToMonicForm(this Polynomial<XTerm> polynomial) {
            var highCoef = polynomial.Coefficients.MayMaxBy(e => e.Key.XPower).Select(e => e.Value).Else(1);
            return polynomial/highCoef;
        }
        public static Polynomial<TTerm> ToIntegerForm<TTerm>(this Polynomial<TTerm> polynomial) where TTerm : ITerm<TTerm> {
            var lcm = polynomial.Coefficients.Select(e => e.Value).LeastCommonDenominator();
            return polynomial * lcm;
        }
        private static IEnumerable<Polynomial<XTerm>> RewritePolyBasisUsing(Polynomial<XTerm> poly) {
            var termToRewrite = poly.Coefficients.MaxBy(e => e.Key.XPower);
            var rewrittenTerm = (termToRewrite - poly) / termToRewrite.Value;
            return RewritePolyBasisUsing(termToRewrite.Key, rewrittenTerm);
        }

        public static Polynomial<XTerm> MultiplyRoots(this Polynomial<XTerm> poly1,
                                                              Polynomial<XTerm> poly2) {
            var deg1 = poly1.Degree();
            var deg2 = poly2.Degree();

            var maxDegreeOfResult = deg1*deg2;
            if (maxDegreeOfResult == 0) return 1;

            var rewrittenPowers1 = RewritePolyBasisUsing(poly1).Select(p => p.ToPolynomialOverVariable1Of2());
            var rewrittenPowers2 = RewritePolyBasisUsing(poly2).Select(p => p.ToPolynomialOverVariable2Of2());
            var rewrittenPowerPairs =
                rewrittenPowers1.Zip(rewrittenPowers2,
                                     Polynomial<XYTerm>.Multiply)
                                .Take(maxDegreeOfResult + 1);

            var linearSystem = Matrix.FromColumns(
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
        public static Polynomial<TTerm> Sum<TTerm>(this IEnumerable<Polynomial<TTerm>> polys) where TTerm : ITerm<TTerm> {
            return polys.Aggregate(Polynomial<TTerm>.Zero, (a, e) => a + e);
        }  
        public static Polynomial<XTerm> AddRoots(this Polynomial<XTerm> poly1,
                                                          Polynomial<XTerm> poly2) {
            var deg1 = poly1.Degree();
            var deg2 = poly2.Degree();

            var maxDegreeOfResult = deg1*deg2;
            if (maxDegreeOfResult == 0) return 1;

            var rewrittenPowers1 = RewritePolyBasisUsing(poly1).Select(p => p.ToPolynomialOverVariable1Of2()).Take(maxDegreeOfResult + 1).ToArray();
            var rewrittenPowers2 = RewritePolyBasisUsing(poly2).Select(p => p.ToPolynomialOverVariable2Of2()).Take(maxDegreeOfResult + 1).ToArray();

            Polynomial<XYTerm> X = new XYTerm(1, 0);
            Polynomial<XYTerm> Y = new XYTerm(0, 1);
            var raiseds = 
                CollectionUtil.Naturals
                .Select(e => (X + Y).RaisedTo(e))
                .Take(maxDegreeOfResult + 1)
                .ToArray();

            var rewritten = 
                raiseds
                .Select(e => 
                    e.Coefficients
                    .Select(r => rewrittenPowers1[(int)r.Key.XPower]*rewrittenPowers2[(int)r.Key.YPower]*r.Value)
                    .Sum())
                .ToArray();

            var linearSystem = Matrix.FromColumns(
                from col in rewritten
                select from row in maxDegreeOfResult.Range()
                       let exponentForX = row/deg2
                       let exponentForY = row%deg2
                       select col.Coefficient(new XYTerm(exponentForX, exponentForY)));

            var solvedSystem = linearSystem.Reduced();

            var degreeOfSolution = solvedSystem.Rows.Count(row => row.Any(cell => cell != 0));

            var lowCoefficients = solvedSystem.Columns[degreeOfSolution].Take(degreeOfSolution);

            return XToThe(degreeOfSolution) - lowCoefficients.TimesPolynomialBasis();
        }
        public static Polynomial<XTerm> Derivative(this Polynomial<XTerm> polynomial) {
            return new Polynomial<XTerm>(
                polynomial.Coefficients
                .Where(e => e.Key.XPower > 0)
                .Select(e => new XTerm(e.Key.XPower - 1).KeyVal(e.Value*e.Key.XPower)));
        }
        public static IEnumerable<Range<Frac>> ApproximateRoots(this Polynomial<XTerm> polynomial, Frac epsilon) {
            return ApproximateRootsHelper(polynomial, epsilon).OrderBy(e => e.Min).Distinct();
        }
        private static IEnumerable<Range<Frac>> ApproximateRootsHelper(this Polynomial<XTerm> polynomial, Frac epsilon) {
            if (epsilon <= 0) throw new ArgumentOutOfRangeException("epsilon");
            if (!polynomial.Coefficients.Any()) throw new InvalidOperationException("Everything is a root...");

            var degree = polynomial.Degree();
            if (degree == 0) return new Range<Frac>[0];

            if (degree == 1)
                return new Range<Frac>[] {-polynomial.Coefficient(XToThe(0))/polynomial.Coefficient(XToThe(1))};

            var criticalRegions =
                polynomial
                .Derivative()
                .ApproximateRoots(epsilon)
                .ToArray();

            var lowerRoot = polynomial.BisectLower(criticalRegions.First().Min, epsilon);
            var upperRoot = polynomial.BisectUpper(criticalRegions.Last().Max, epsilon);
            var r0 = new[] {lowerRoot, upperRoot}.WhereHasValue();

            var r1 = criticalRegions.Where(r => Frac.Abs(polynomial.EvaluateAt((r.Min + r.Max)/2)) < epsilon);

            var interRegions =
                criticalRegions
                .Window(2)
                .Where(e => e.Count() == 2)
                .Select(e => new Range<Frac>(e.First().Max, e.Last().Min, false, false))
                .ToArray();

            var r2 = interRegions.Select(e => polynomial.Bisect(e.Min, e.Max, epsilon)).WhereHasValue();

            return r0.Concat(r1).Concat(r2);
        }
        private static May<Range<Frac>> BisectLower(this Polynomial<XTerm> polynomial, Frac maxX, Frac epsilon) {
            var maxCoef = polynomial.Coefficients.MaxBy(e => e.Key.XPower);
            var decreasingLimitSign = maxCoef.Value.Sign * (maxCoef.Key.XPower % 2 == 0 ? +1 : -1);

            var maxS = polynomial.EvaluateAt(maxX).Sign;
            if (maxS == decreasingLimitSign) return May.NoValue;
            if (maxS == 0) return (Range<Frac>)maxX;

            var d = Frac.One;
            while (true) {
                d *= 2;
                var minX = maxX - d;
                if (polynomial.EvaluateAt(minX).Sign != decreasingLimitSign) continue;
                return polynomial.Bisect(minX, maxX, epsilon);
            }
        }
        private static May<Range<Frac>> BisectUpper(this Polynomial<XTerm> polynomial, Frac minX, Frac epsilon) {
            var increasingLimitSign = polynomial.Coefficients.MaxBy(e => e.Key.XPower).Value.Sign;

            var minS = polynomial.EvaluateAt(minX).Sign;
            if (minS == increasingLimitSign) return May.NoValue;
            if (minS == 0) return (Range<Frac>)minX;

            var d = Frac.One;
            while (true) {
                d *= 2;
                var maxX = minX + d;
                if (polynomial.EvaluateAt(maxX).Sign != increasingLimitSign) continue;
                return polynomial.Bisect(minX, maxX, epsilon);
            }
        }
        private static May<Range<Frac>> Bisect(this Polynomial<XTerm> polynomial, Frac minX, Frac maxX, Frac epsilon) {
            var minS = polynomial.EvaluateAt(minX).Sign;
            var maxS = polynomial.EvaluateAt(maxX).Sign;
            if (minS == 0) return (Range<Frac>)minX;
            if (maxS == 0) return (Range<Frac>)maxX;
            if (minS == maxS) return May.NoValue;

            while (maxX - minX > epsilon) {
                var x = (minX + maxX)/2;
                var y = polynomial.EvaluateAt(x);
                if (y == 0) return (Range<Frac>)x;
                if (y.Sign == minS) {
                    minX = x;
                } else {
                    maxX = x;
                }
            }

            return new Range<Frac>(minX, maxX, false, false);
        }
    }
}

