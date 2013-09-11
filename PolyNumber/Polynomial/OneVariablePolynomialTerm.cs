using System;
using System.Diagnostics;
using System.Numerics;

[DebuggerDisplay("{ToString()}")]
public struct OneVariablePolynomialTerm : IPolynomialTerm<OneVariablePolynomialTerm>, IComparable<OneVariablePolynomialTerm> {
    public readonly BigInteger Power;
    public OneVariablePolynomialTerm(BigInteger power) {
        if (power < 0) throw new ArgumentOutOfRangeException();
        Power = power;
    }
    public OneVariablePolynomialTerm Times(OneVariablePolynomialTerm other) {
        return new OneVariablePolynomialTerm(Power + other.Power);
    }
    public static implicit operator OneVariablePolynomialTerm(BigInteger power) {
        return new OneVariablePolynomialTerm(power);
    }
    private static string PowerFactorString(string var, BigInteger power) {
        if (power == 0) return "";
        if (power == 1) return var;
        if (power == 2) return var + "²";
        if (power == 3) return var + "³";
        return var + "^" + power;
    }
    public int CompareTo(OneVariablePolynomialTerm other) {
        return Power.CompareTo(other.Power);
    }
    public override string ToString() {
        return PowerFactorString("x", Power);
    }
}