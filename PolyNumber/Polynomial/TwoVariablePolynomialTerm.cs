using System;
using System.Diagnostics;
using System.Numerics;

[DebuggerDisplay("{ToString()}")]
public struct TwoVariablePolynomialTerm : IPolynomialTerm<TwoVariablePolynomialTerm>, IComparable<TwoVariablePolynomialTerm> {
    public readonly BigInteger Power1;
    public readonly BigInteger Power2;
    public TwoVariablePolynomialTerm(BigInteger power1, BigInteger power2) {
        if (power1 < 0) throw new ArgumentOutOfRangeException();
        if (power2 < 0) throw new ArgumentOutOfRangeException();
        Power1 = power1;
        Power2 = power2;
    }
    public TwoVariablePolynomialTerm Times(TwoVariablePolynomialTerm other) {
        return new TwoVariablePolynomialTerm(Power1 + other.Power1, Power2 + other.Power2);
    }
    private static string PowerFactorString(string var, BigInteger power) {
        if (power == 0) return "";
        if (power == 1) return var;
        if (power == 2) return var + "²";
        if (power == 3) return var + "³";
        return var + "^" + power;
    }
    public int CompareTo(TwoVariablePolynomialTerm other) {
        if (Power1 != other.Power1) return Power1.CompareTo(other.Power1);
        return Power2.CompareTo(other.Power2);
    }
    public override string ToString() {
        return PowerFactorString("x", Power1) + PowerFactorString("y", Power2);
    }
}