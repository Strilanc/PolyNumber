using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

[TestClass]
public class PolynomialUtilTest {
    [TestMethod]
    public void TestRootsToCoefficients() {
        new[] { 1, 2 }.RootsToCoefficients().AssertSequenceEquals(1, 3, 2);
        new[] { 3, 4 }.RootsToCoefficients().AssertSequenceEquals(1, 7, 12);
    }

    [TestMethod]
    public void TempTest() {
        var p1 = PolyNumber.FromRoots(1, 2);
        var p2 = PolyNumber.FromRoots(3, 4);
        var p3 = p1*p2;
    }
    [TestMethod]
    public void TempTest2() {
        var p1 = PolyNumber.FromRoots(1, 2);
        var p2 = PolyNumber.FromRoots(3, 4);
        var p3 = p1 + p2;
        var zz = p3.ComputeRoots(0.0001).ToArray();
        var p4 = PolyNumber.FromRoots(4, 5, 6);
    }
    [TestMethod]
    public void TempTest3() {
        PolyNumber.FromRoots(1).ComputeRoots(0.1).AssertSequenceEquals(1);
        PolyNumber.FromRoots(1,2).ComputeRoots(0.1).AssertSequenceEquals(1,2);
    }
}
