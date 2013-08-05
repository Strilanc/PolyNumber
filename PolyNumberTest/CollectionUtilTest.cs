using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strilanc.LinqToCollections;

[TestClass]
public class CollectionUtilTest {
    [TestMethod]
    public void TestCombinationsOfSize() {
        0.Range().CombinationsOfSize(0)
         .AssertSequenceSimilar(new[] { new int[0] });
        0.Range().CombinationsOfSize(1)
         .AssertSequenceSimilar(new int[0][]);

        1.Range().CombinationsOfSize(0)
         .AssertSequenceSimilar(new[] { new int[0] });
        1.Range().CombinationsOfSize(1)
         .AssertSequenceSimilar(new[] { new[] { 0 } });
        1.Range().CombinationsOfSize(2)
         .AssertSequenceSimilar(new int[0][]);

        3.Range().CombinationsOfSize(0)
         .AssertSequenceSimilar(new[] { new int[0] });
        3.Range().CombinationsOfSize(1)
         .AssertSequenceSimilar(new[] { new[] { 0 }, new[] { 1 }, new[] { 2 } });
        3.Range().CombinationsOfSize(2)
         .AssertSequenceSimilar(new[] { new[] { 0, 1 }, new[] { 0, 2 }, new[] { 1, 2 } });
        3.Range().CombinationsOfSize(3)
         .AssertSequenceSimilar(new[] { new[] { 0, 1, 2 } });
        3.Range().CombinationsOfSize(4)
         .AssertSequenceSimilar(new int[0][]);
    }
}
