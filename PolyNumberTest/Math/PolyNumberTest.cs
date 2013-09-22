using System;
using System.Numerics;
using Math2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Numerics;

[TestClass]
public class PolyNumberTest {
    [TestMethod]
    public void TestConvert() {
        PolyNumber x = 2;
        x.HasValue(2).AssertTrue();
        Assert.AreEqual(x.Approximates().Single(), 2, 0.001);

        PolyNumber x2 = new BigRational(3, 5);
        x2.HasValue(new BigRational(3, 5)).AssertTrue();
        Assert.AreEqual(x2.Approximates().Single(), 0.6, 0.001);
    }
    [TestMethod]
    public void TestRoot() {
        for (var i = 1; i < 5; i++) {
            for (var n = 1; n < 3; n++) {
                var r = ((PolyNumber)n).Root(i);
                Assert.AreEqual(r.Approximates().Last(), System.Math.Pow(n, 1.0 / i), 0.00001);
                r.RaiseTo(i).HasValue(n).AssertTrue();
            }
        }

        ((PolyNumber)(-1)).Root(2).Approximates().Any().AssertFalse();
    }

    [TestMethod]
    public void TestNegate() {
        (-(PolyNumber)1).HasValue(-1).AssertTrue();
        (-(PolyNumber)0).HasValue(0).AssertTrue();

        var p = -new PolyNumber(Polynomial.FromRoots(1, 2, 3));
        p.HasValue(-1).AssertTrue();
        p.HasValue(-2).AssertTrue();
        p.HasValue(-3).AssertTrue();
        p.Constraint.Degree().AssertEquals((BigInteger)3);
    }
    [TestMethod]
    public void TestMultiplicativeInverse() {
        ((PolyNumber)1).MultiplicativeInverse().HasValue(1).AssertTrue();
        ((PolyNumber)2).MultiplicativeInverse().HasValue(new BigRational(1, 2)).AssertTrue();

        var p = new PolyNumber(Polynomial.FromRoots(1, 2, new BigRational(3, 5))).MultiplicativeInverse();
        p.HasValue(1).AssertTrue();
        p.HasValue(new BigRational(1, 2)).AssertTrue();
        p.HasValue(new BigRational(5, 3)).AssertTrue();
        p.Constraint.Degree().AssertEquals((BigInteger)3);
    }
    [TestMethod]
    public void TestAdd1() {
        PolyNumber x1 = 1;
        PolyNumber x2 = 2;
        var x3 = x1 + x2;
        x3.HasValue(3).AssertTrue();
    }
    [TestMethod]
    public void TestAdd2() {
        PolyNumber x1 = new BigRational(2, 3);
        PolyNumber x2 = new BigRational(5, 7);
        var x3 = x1 + x2;
        x3.HasValue(new BigRational(2 * 7 + 5 * 3, 3 * 7)).AssertTrue();
    }
    [TestMethod]
    public void TestAdd3() {
        var x1 = ((PolyNumber)3).Root(3);
        var x2 = ((PolyNumber)5).Root(5);
        var x3 = x1 + x2;
        var expected = Math.Pow(3, 1.0/3) + Math.Pow(5, 1/5.0);
        x3.HasValueNear(expected.HackFix_ToApproxBigRational()).AssertTrue();
    }
    [TestMethod]
    public void TestAdd4() {
        var x = new PolyNumber(Polynomial.FromRoots(1, 4, 7)) + new PolyNumber(Polynomial.FromRoots(1, 2, 3));
        x.Constraint.Degree().AssertEquals((BigInteger)9);
        x.HasValue(2).AssertTrue();
        x.HasValue(3).AssertTrue();
        x.HasValue(4).AssertTrue();
        x.HasValue(5).AssertTrue();
        x.HasValue(6).AssertTrue();
        x.HasValue(7).AssertTrue();
        x.HasValue(8).AssertTrue();
        x.HasValue(9).AssertTrue();
        x.HasValue(10).AssertTrue();
    }

    [TestMethod]
    public void TestSubtract1() {
        PolyNumber x1 = 1;
        PolyNumber x2 = 2;
        var x3 = x1 - x2;
        x3.HasValue(-1).AssertTrue();
    }
    [TestMethod]
    public void TestSubtract2() {
        PolyNumber x1 = new BigRational(2, 3);
        PolyNumber x2 = new BigRational(5, 7);
        var x3 = x1 - x2;
        x3.HasValue(new BigRational(2 * 7 - 5 * 3, 3 * 7)).AssertTrue();
    }
    [TestMethod]
    public void TestSubtract3() {
        var x1 = ((PolyNumber)3).Root(3);
        var x2 = ((PolyNumber)5).Root(5);
        var x3 = x1 - x2;
        var expected = Math.Pow(3, 1.0 / 3) - Math.Pow(5, 1 / 5.0);
        x3.HasValueNear(expected.HackFix_ToApproxBigRational()).AssertTrue();
    }

    [TestMethod]
    public void TestMultiply1() {
        PolyNumber x1 = new BigRational(2, 3);
        PolyNumber x2 = new BigRational(5, 7);
        var x3 = x1*x2;
        x3.HasValue(new BigRational(2*5, 3*7)).AssertTrue();
    }
    [TestMethod]
    public void TestMultiply2() {
        var x = new PolyNumber(Polynomial.FromRoots(1, 4, 7)) * new PolyNumber(Polynomial.FromRoots(1, 2, 3));
        x.Constraint.Degree().AssertEquals((BigInteger)9);
        x.HasValue(1).AssertTrue();
        x.HasValue(2).AssertTrue();
        x.HasValue(3).AssertTrue();
        x.HasValue(4).AssertTrue();
        x.HasValue(8).AssertTrue();
        x.HasValue(12).AssertTrue();
        x.HasValue(7).AssertTrue();
        x.HasValue(14).AssertTrue();
        x.HasValue(21).AssertTrue();
    }
    [TestMethod]
    public void TestOperations() {
        PolyNumber x = 5;
        var y = x.Root(3) + x.Root(2) - 1;
        var exp = Math.Pow(5, 1/3.0) + Math.Pow(5, 1/2.0) - 1;
        y.HasValueNear(exp.HackFix_ToApproxBigRational()).AssertTrue();
    }
}
