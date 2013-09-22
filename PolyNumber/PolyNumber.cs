using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Numerics;
using Math;
using Strilanc.LinqToCollections;
using Int = System.Numerics.BigInteger;

public struct PolyNumber {
    public static PolyNumber Zero { get { return 0; } }

    public readonly IntPolynomial<XTerm> Constraint;
    public PolyNumber(IntPolynomial<XTerm> constraint) {
        if (constraint.Degree() < 1) throw new ArgumentOutOfRangeException("constraint", "No solutions.");
        Constraint = constraint;
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

    public static PolyNumber operator +(PolyNumber value1, PolyNumber value2) {
        return new PolyNumber(value1.Constraint.AddRoots(value2.Constraint));
    }
    public static PolyNumber operator *(PolyNumber value1, PolyNumber value2) {
        return new PolyNumber(value1.Constraint.MultiplyRoots(value2.Constraint));
    }

    public override string ToString() {
        return Constraint.ToString();
    }
}
