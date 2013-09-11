using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

[TestClass]
public class IntegerPolynomialTest {
    public static IntegerPolynomial<OneVariablePolynomialTerm> Poly(params BigInteger[] coefs) {
        return new IntegerPolynomial<OneVariablePolynomialTerm>(coefs.Reverse().Select((e, i) => new OneVariablePolynomialTerm(i).KeyVal(e)));
    }
    public static IntegerPolynomial<TwoVariablePolynomialTerm> Poly1(params BigInteger[] coefs) {
        return new IntegerPolynomial<TwoVariablePolynomialTerm>(coefs.Reverse().Select((e, i) => new TwoVariablePolynomialTerm(i, 0).KeyVal(e)));
    }
    public static IntegerPolynomial<TwoVariablePolynomialTerm> Poly2(params BigInteger[] coefs) {
        return new IntegerPolynomial<TwoVariablePolynomialTerm>(coefs.Reverse().Select((e, i) => new TwoVariablePolynomialTerm(0, i).KeyVal(e)));
    }

    [TestMethod]
    public void AdditionTest() {
        var p1 = Poly(1, 2, 3);
        var p2 = Poly(2, 3, 5, 7);
        (p1 + p2).AssertEquals(Poly(2, 4, 7, 10));
    }
    [TestMethod]
    public void MultiplicationTest() {
        var p1 = Poly(1, 2);
        var p2 = Poly(2, 3, 5);
        (p1 * p2).AssertEquals(Poly(2, 7, 11, 10));
    }
    [TestMethod]
    public void RaisedToTest() {
        Poly(1).RaisedTo(10000000).AssertEquals(Poly(1));
        Poly(1, 0).RaisedTo(10000000).AssertSimilar(new OneVariablePolynomialTerm(10000000).KeyVal(BigInteger.One));
        Poly(2, 0).RaisedTo(50).AssertSimilar(new OneVariablePolynomialTerm(50).KeyVal(BigInteger.Pow(2, 50)));
        Poly(1, 1).RaisedTo(5).AssertSimilar(Poly(1, 5, 10, 10, 5, 1));

        Poly2(1, 0).RaisedTo(10000000).AssertSimilar(new TwoVariablePolynomialTerm(0, 10000000).KeyVal(BigInteger.One));
    }
    [TestMethod]
    public void MultiplicationTest2() {
        var p1 = Poly1(1, 2);
        var p2 = Poly2(2, 3, 5);
        (p1 * p2).AssertEquals(new IntegerPolynomial<TwoVariablePolynomialTerm>(new Dictionary<TwoVariablePolynomialTerm, BigInteger> {
            {new TwoVariablePolynomialTerm(0, 0), 10},
            {new TwoVariablePolynomialTerm(0, 1), 6},
            {new TwoVariablePolynomialTerm(0, 2), 4},
            {new TwoVariablePolynomialTerm(1, 0), 5},
            {new TwoVariablePolynomialTerm(1, 1), 3},
            {new TwoVariablePolynomialTerm(1, 2), 2}
        }));
    }
}
