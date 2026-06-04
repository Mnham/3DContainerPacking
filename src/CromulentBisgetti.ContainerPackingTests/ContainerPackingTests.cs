using System.Globalization;
using System.Reflection;
using CromulentBisgetti.ContainerPacking;
using CromulentBisgetti.ContainerPacking.Entities;

namespace CromulentBisgetti.ContainerPackingTests
{
    [TestClass]
    public class ContainerPackingReferenceTests
    {
        private const int ReferenceTestCount = 700;

        [TestMethod]
        public void EB_AFIT_improved_Packs_700_Reference_Cases_Consistently()
        {
            List<ReferenceCase> referenceCases = LoadReferenceCases(ReferenceTestCount);

            Parallel.ForEach(
                referenceCases,
                new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount) },
                AssertReferenceCase);
        }

        private static void AssertReferenceCase(ReferenceCase referenceCase)
        {
            List<ContainerPackingResult> result = PackingService.Pack(
                new List<Container> { referenceCase.Container },
                referenceCase.ItemsToPack);

            AlgorithmPackingResult packingResult = result[0].PackingResult
                ?? throw new AssertFailedException($"Case {referenceCase.Number}: packing result is missing.");

            // Assert that the number of items we tried to pack equals the number stated in the published reference.
            Assert.AreEqual(
                referenceCase.ExpectedTotalItems,
                packingResult.PackedItems.Count + packingResult.UnpackedItems.Count,
                $"Case {referenceCase.Number}: total item count mismatch.");

            decimal packedVolume = packingResult.PackedItems.Sum(item => item.Volume);
            Assert.IsTrue(
                packedVolume <= referenceCase.Container.Volume,
                $"Case {referenceCase.Number}: packed volume exceeds container volume.");

            Assert.IsTrue(
                packingResult.PercentContainerVolumePacked is >= 0 and <= 100,
                $"Case {referenceCase.Number}: packed container volume percentage is out of range.");

            Assert.IsTrue(
                packingResult.PercentItemVolumePacked is >= 0 and <= 100,
                $"Case {referenceCase.Number}: packed item volume percentage is out of range.");
        }

        private static List<ReferenceCase> LoadReferenceCases(int maxCases)
        {
            // ORLibrary.txt is an Embedded Resource in this project.
            const string resourceName = "CromulentBisgetti.ContainerPackingTests.DataFiles.ORLibrary.txt";
            Assembly assembly = Assembly.GetExecutingAssembly();
            var referenceCases = new List<ReferenceCase>();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                // Counter to control how many tests are run in dev.
                for (int counter = 1; reader.ReadLine() != null && counter <= maxCases; counter++)
                {
                    var itemsToPack = new List<Item>();

                    // First line in each test case is an ID. Skip it.

                    // Second line states the results of the test, as reported in the EB-AFIT master's thesis, appendix E.
                    string[] testResults = SplitLine(reader);

                    // Third line defines the container dimensions.
                    string[] containerDims = SplitLine(reader);

                    // Fourth line states how many distinct item types we are packing.
                    int itemTypeCount = ParseInt(reader.ReadLine());

                    for (int i = 0; i < itemTypeCount; i++)
                    {
                        string[] itemArray = SplitLine(reader);

                        var item = new Item(0, ParseDecimal(itemArray[1]), ParseDecimal(itemArray[3]), ParseDecimal(itemArray[5]), ParseInt(itemArray[7]));
                        itemsToPack.Add(item);
                    }

                    var container = new Container(0, ParseDecimal(containerDims[0]), ParseDecimal(containerDims[1]), ParseDecimal(containerDims[2]));

                    referenceCases.Add(new ReferenceCase(
                        counter,
                        container,
                        itemsToPack,
                        ParseInt(testResults[1])));
                }
            }

            return referenceCases;
        }

        private static string[] SplitLine(StreamReader reader) =>
            reader.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        private static decimal ParseDecimal(string value) =>
            decimal.Parse(value, CultureInfo.InvariantCulture);

        private static int ParseInt(string value) =>
            int.Parse(value, CultureInfo.InvariantCulture);

        private sealed class ReferenceCase
        {
            public int Number { get; }
            public Container Container { get; }
            public List<Item> ItemsToPack { get; }
            public int ExpectedTotalItems { get; }

            public ReferenceCase(
                int number,
                Container container,
                List<Item> itemsToPack,
                int expectedTotalItems)
            {
                Number = number;
                Container = container;
                ItemsToPack = itemsToPack;
                ExpectedTotalItems = expectedTotalItems;
            }
        }
    }
}
