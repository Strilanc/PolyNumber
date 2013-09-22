using System;
using System.Numerics;
using Numerics;
using Math2;
using Int = System.Numerics.BigInteger;
using System.Linq;

public struct PolyNumber {
    public static PolyNumber Zero { get { return 0; } }

    public readonly RationalPolynomial<XTerm> Constraint;
    public PolyNumber(RationalPolynomial<XTerm> constraint) {
        if (constraint.Degree() < 1) throw new ArgumentOutOfRangeException("constraint", "No solutions.");
        Constraint = constraint.ToIntegerForm();
    }

    public static implicit operator PolyNumber(BigRational value) {
        return new PolyNumber(Polynomial.FromBigEndianCoefficients(value.Denominator, -value.Numerator));
    }
    public static implicit operator PolyNumber(Int value) {
        return new PolyNumber(Polynomial.FromBigEndianCoefficients(1, -value));
    }
    public static implicit operator PolyNumber(int value) {
        return (BigInteger)value;
    }

    public PolyNumber MultiplicativeInverse() {
        var degree = Constraint.Degree();
        var rev = Constraint.Coefficients.SelectKeyValue(e => new XTerm(degree - e.Key.XPower), e => e.Value).ToPolynomial();
        return new PolyNumber(rev);
    }
    public PolyNumber Root(int nth) {
        if (nth < 1) throw new ArgumentOutOfRangeException("nth");
        var exp = Constraint.Coefficients.SelectKeyValue(e => new XTerm(e.Key.XPower * nth), e => e.Value).ToPolynomial();
        return new PolyNumber(exp);
    }
    public PolyNumber RaiseTo(int power) {
        if (power == 0) return 1;
        if (power < 0) return MultiplicativeInverse().RaiseTo(-power);
        var r = this;
        while (power > 1) {
            r *= this;
            power -= 1;
        }
        return r;
    }
    public static PolyNumber operator -(PolyNumber value) {
        return new PolyNumber(value.Constraint.Coefficients.SelectKeyValue(e => e.Key, e => e.Value * (e.Key.XPower % 2 == 0 ? +1 : -1)).ToPolynomial());
    }
    public static PolyNumber operator -(PolyNumber value1, PolyNumber value2) {
        return value1 + -value2;
    }
    public static PolyNumber operator +(PolyNumber value1, PolyNumber value2) {
        return new PolyNumber(value1.Constraint.AddRoots(value2.Constraint));
    }
    public static PolyNumber operator *(PolyNumber value1, PolyNumber value2) {
        return new PolyNumber(value1.Constraint.MultiplyRoots(value2.Constraint));
    }
    public static PolyNumber operator /(PolyNumber value1, PolyNumber value2) {
        return value1*value2.MultiplicativeInverse();
    }

    public bool HasValue(BigRational root) {
        return Constraint.EvaluateAt(root) == 0;
    }
    public bool HasValueNear(BigRational value, BigRational? epsilon = null) {
        var y = BigRational.Abs(Constraint.EvaluateAt(value));
        return y < (epsilon ?? new BigRational(1, 1000000));
    }
    public double[] Approximates(BigRational? epsilon = null) {
        return Constraint.ApproximateRoots(epsilon ?? new BigRational(1, 1000000)).Select(e => (double)(e.Min + e.Max)/2).ToArray();
    }

    public override string ToString() {
        return Constraint.ToString();
    }
}
