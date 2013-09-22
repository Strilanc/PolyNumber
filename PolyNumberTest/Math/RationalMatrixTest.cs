using Microsoft.VisualStudio.TestTools.UnitTesting;
using Numerics;

[TestClass]
public class RationalMatrixTest {
    [TestMethod]
    public void ReduceTest1() {
        var m = RationalMatrix.FromRows(
            new[] { 4, 6 });

        var r = m.Reduced();
        r.Width.AssertEquals(m.Width);
        r.Height.AssertEquals(m.Height);
        r.Rows[0].AssertSequenceEquals(1, new BigRational(3, 2));
    }

    [TestMethod]
    public void ReduceTest2() {
        var m = RationalMatrix.FromRows(
            new[] { -4, 6 });

        var r = m.Reduced();
        r.Width.AssertEquals(m.Width);
        r.Height.AssertEquals(m.Height);
        r.Rows[0].AssertSequenceEquals(1, new BigRational(-3, 2));
    }

    [TestMethod]
    public void ReduceTest3() {
        var m = RationalMatrix.FromRows(
            new[] {1, 0, -2, 0, -14},
            new[] {0, 0, -3, 0, -15},
            new[] {0, 0, 0, 6, 0},
            new[] {0, 1, 0, 7, 0});

        var r = m.Reduced();
        r.Width.AssertEquals(m.Width);
        r.Height.AssertEquals(m.Height);
        r.Rows[0].AssertSequenceEquals(1, 0, 0, 0, -4);
        r.Rows[1].AssertSequenceEquals(0, 1, 0, 0, 0);
        r.Rows[2].AssertSequenceEquals(0, 0, 1, 0, 5);
        r.Rows[3].AssertSequenceEquals(0, 0, 0, 1, 0);
    }
}
