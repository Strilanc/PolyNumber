using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;
using Strilanc.Value;

public static class Polynomial {
    public static IntegerPolynomial<OneVariablePolynomialTerm> FromRoots(IEnumerable<BigInteger> roots) {
        if (roots == null) throw new ArgumentNullException("roots");
        return roots.Select(e => FromBigEndianCoefficients(1, -e)).Aggregate(FromBigEndianCoefficients(1), (a, e) => a * e);
    }
    public static IntegerPolynomial<OneVariablePolynomialTerm> FromRoots(params BigInteger[] roots) {
        if (roots == null) throw new ArgumentNullException("roots");
        return FromRoots(roots.AsEnumerable());
    }
    public static IntegerPolynomial<OneVariablePolynomialTerm> FromBigEndianCoefficients(params BigInteger[] coefs) {
        if (coefs == null) throw new ArgumentNullException("coefs");
        return new IntegerPolynomial<OneVariablePolynomialTerm>(coefs.Reverse().Select((e, i) => new OneVariablePolynomialTerm(i).KeyVal(e)));
    }
    public static IntegerPolynomial<TwoVariablePolynomialTerm> FromBigEndianCoefficientsOverVar1Of2(params BigInteger[] coefs) {
        if (coefs == null) throw new ArgumentNullException("coefs");
        return new IntegerPolynomial<TwoVariablePolynomialTerm>(coefs.Reverse().Select((e, i) => new TwoVariablePolynomialTerm(i, 0).KeyVal(e)));
    }
    public static IntegerPolynomial<TwoVariablePolynomialTerm> FromBigEndianCoefficientsOverVar2Of2(params BigInteger[] coefs) {
        if (coefs == null) throw new ArgumentNullException("coefs");
        return new IntegerPolynomial<TwoVariablePolynomialTerm>(coefs.Reverse().Select((e, i) => new TwoVariablePolynomialTerm(0, i).KeyVal(e)));
    }
    public static IntegerPolynomial<TTerm> ToPolynomial<TTerm>(this IEnumerable<KeyValuePair<TTerm, BigInteger>> terms) where TTerm : IPolynomialTerm<TTerm> {
        if (terms == null) throw new ArgumentNullException("terms");
        return new IntegerPolynomial<TTerm>(terms);
    }
    public static IntegerPolynomial<TwoVariablePolynomialTerm> ToPolynomialOverVariable1Of2(this IntegerPolynomial<OneVariablePolynomialTerm> polynomial) {
        return polynomial.Coefficients.Select(e => new TwoVariablePolynomialTerm(e.Key.Power, 0).KeyVal(e.Value)).ToPolynomial();
    }
    public static IntegerPolynomial<TwoVariablePolynomialTerm> ToPolynomialOverVariable2Of2(this IntegerPolynomial<OneVariablePolynomialTerm> polynomial) {
        return polynomial.Coefficients.Select(e => new TwoVariablePolynomialTerm(0, e.Key.Power).KeyVal(e.Value)).ToPolynomial();
    }
    public static IEnumerable<OneVariablePolynomialTerm> Basis { get { return CollectionUtil.Naturals.Select(XToThe); } }
    public static OneVariablePolynomialTerm XToThe(BigInteger power) {
        return new OneVariablePolynomialTerm(power);
    }
    public static IntegerPolynomial<OneVariablePolynomialTerm> TimesPolynomialBasis(this IEnumerable<BigInteger> coefficients) {
        return coefficients.Zip(Basis, (c, x) => x.KeyVal(c)).ToPolynomial();
    }
    public static BigInteger Degree(this IntegerPolynomial<OneVariablePolynomialTerm> polynomial) {
        return polynomial.Coefficients.Select(e => e.Key.Power).MayMax().ElseDefault();
    }

    public static bool DividesScaled(this IntegerPolynomial<OneVariablePolynomialTerm> denominator, IntegerPolynomial<OneVariablePolynomialTerm> numerator) {
        if (denominator == 0) return false;
        if (denominator.Degree() == 0) return true;

        var remainder = numerator;

        while (true) {
            var d1 = remainder.Degree();
            var d2 = denominator.Degree();
            if (d1 < d2) break;

            var nt = remainder.Coefficient(d1);
            var dt = denominator.Coefficient(d2);

            var gcd = BigInteger.GreatestCommonDivisor(nt, dt);
            remainder = remainder * (dt / gcd) - denominator * XToThe(d1 - d2) * (nt / gcd);

            if (remainder.Degree() == d1) throw new Exception();
        }

        return remainder == 0;
    }
}