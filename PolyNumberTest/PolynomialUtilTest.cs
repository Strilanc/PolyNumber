using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PolynomialUtilTest {
    [TestMethod]
    public void TestRootsToCoefficients() {
        new[] { 1, 2 }.RootsToCoefficients().AssertSequenceEquals(1, 3, 2);
        new[] { 3, 4 }.RootsToCoefficients().AssertSequenceEquals(1, 7, 12);
    }
}
