using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CromulentBisgetti.ContainerPacking;
using CromulentBisgetti.ContainerPacking.Algorithms;
using CromulentBisgetti.ContainerPacking.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CromulentBisgetti.ContainerPackingTests
{
    [TestClass]
    public class ContainerPackingTests
    {
        private const int ReferenceTestCount = 700;

        [TestMethod]
        public void EB_AFIT_Passes_700_Standard_Reference_Tests()
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
                referenceCase.ItemsToPack,
                new List<int> { (int)AlgorithmType.EB_AFIT });

            AlgorithmPackingResult packingResult = result[0].AlgorithmPackingResults[0];

            // Assert that the number of items we tried to pack equals the number stated in the published reference.
            Assert.AreEqual(
                referenceCase.ExpectedTotalItems,
                packingResult.PackedItems.Count + packingResult.UnpackedItems.Count,
                $"Case {referenceCase.Number}: total item count mismatch.");

            // Assert that the number of items successfully packed equals the number stated in the published reference.
            Assert.AreEqual(
                referenceCase.ExpectedPackedItems,
                packingResult.PackedItems.Count,
                $"Case {referenceCase.Number}: packed item count mismatch.");

            // Assert that the packed container volume percentage is equal to the published reference result.
            // Make an exception for a couple of tests where this algorithm yields 87.20% and the published result
            // was 87.21% (acceptable rounding error).
            Assert.IsTrue(
                packingResult.PercentContainerVolumePacked == referenceCase.ExpectedContainerVolumePacked
                    || (packingResult.PercentContainerVolumePacked == 87.20M && referenceCase.ExpectedContainerVolumePacked == 87.21M),
                $"Case {referenceCase.Number}: packed container volume percentage mismatch.");

            // Assert that the packed item volume percentage is equal to the published reference result.
            Assert.AreEqual(
                referenceCase.ExpectedItemVolumePacked,
                packingResult.PercentItemVolumePacked,
                $"Case {referenceCase.Number}: packed item volume percentage mismatch.");
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
                for (
                // Counter to control how many tests are run in dev.
                int counter = 1; reader.ReadLine() != null && counter <= maxCases; counter++)
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
                        ParseInt(testResults[1]),
                        ParseInt(testResults[2]),
                        ParseDecimal(testResults[3]),
                        ParseDecimal(testResults[4])));
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
            public ReferenceCase(
                int number,
                Container container,
                List<Item> itemsToPack,
                int expectedTotalItems,
                int expectedPackedItems,
                decimal expectedContainerVolumePacked,
                decimal expectedItemVolumePacked)
            {
                Number = number;
                Container = container;
                ItemsToPack = itemsToPack;
                ExpectedTotalItems = expectedTotalItems;
                ExpectedPackedItems = expectedPackedItems;
                ExpectedContainerVolumePacked = expectedContainerVolumePacked;
                ExpectedItemVolumePacked = expectedItemVolumePacked;
            }

            public int Number { get; }

            public Container Container { get; }

            public List<Item> ItemsToPack { get; }

            public int ExpectedTotalItems { get; }

            public int ExpectedPackedItems { get; }

            public decimal ExpectedContainerVolumePacked { get; }

            public decimal ExpectedItemVolumePacked { get; }
        }
    }
}