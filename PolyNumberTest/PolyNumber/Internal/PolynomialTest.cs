using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strilanc.PolyNumber.Internal;
using Numerics;
using System.Linq;

[TestClass]
public class PolynomialTest {
    [TestMethod]
    public void TestEvaluateAt_X() {
        var p = Polynomial.FromBigEndianCoefficients(1, 2, 3);
        p.EvaluateAt(0).AssertEquals((BigRational)3);
        p.EvaluateAt(1).AssertEquals((BigRational)6);
        p.EvaluateAt(2).AssertEquals((BigRational)11);
        p.EvaluateAt(3).AssertEquals((BigRational)18);

        p.EvaluateAt(new BigRational(3, 2)).AssertEquals(new BigRational(33, 4));
    }
    [TestMethod]
    public void TestDerivative() {
        var p = Polynomial.FromBigEndianCoefficients(1, 2, 3, 0, 5).Derivative();
        p.AssertEquals(Polynomial.FromBigEndianCoefficients(4, 6, 6, 0));
    }
    [TestMethod]
    public void TestApproximateRoots_1() {
        var r = Polynomial.FromRoots(1).ApproximateRoots(0.01).ToArray();
        r.Count().AssertEquals(1);
        r[0].Contains(1).AssertTrue();
    }
    [TestMethod]
    public void TestApproximateRoots_2() {
        var r = Polynomial.FromRoots(3, 3).ApproximateRoots(0.01).ToArray();
        r.Count().AssertEquals(1);
        r[0].Contains(3).AssertTrue();
    }
    [TestMethod]
    public void TestApproximateRoots_3() {
        var r = Polynomial.FromRoots(1, 2, 3).ApproximateRoots(0.01).ToArray();
        r.Count().AssertEquals(3);
        r[0].Contains(1).AssertTrue();
        r[1].Contains(2).AssertTrue();
        r[2].Contains(3).AssertTrue();
    }
}
