using System.Diagnostics;
using CromulentBisgetti.ContainerPacking.Algorithms;
using CromulentBisgetti.ContainerPacking.Entities;

namespace CromulentBisgetti.ContainerPacking
{
    /// <summary>
    /// The container packing service.
    /// </summary>
    public static class PackingService
    {
        /// <summary>
        /// Attempts to pack the specified containers with the specified items.
        /// </summary>
        /// <param name="containers">The list of containers to pack.</param>
        /// <param name="itemsToPack">The items to pack.</param>
        /// <returns>A container packing result with lists of the packed and unpacked items.</returns>
        public static List<ContainerPackingResult> Pack(List<Container> containers, List<Item> itemsToPack)
        {
            object sync = new();
            var result = new List<ContainerPackingResult>();

            Parallel.ForEach(containers, container =>
            {
                var containerPackingResult = new ContainerPackingResult
                {
                    ContainerID = container.ID
                };

                var algorithm = new EB_AFIT_improved();

                // Until I rewrite the algorithm with no side effects, we need to clone the item list
                // so the parallel updates don't interfere with each other.
                var items = new List<Item>();

                itemsToPack.ForEach(item => items.Add(new Item(item.ID, item.Dim1, item.Dim2, item.Dim3, item.Quantity)));

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                AlgorithmPackingResult algorithmResult = algorithm.Run(container, items);
                stopwatch.Stop();

                algorithmResult.PackTimeInMilliseconds = stopwatch.ElapsedMilliseconds;

                decimal containerVolume = container.Length * container.Width * container.Height;
                decimal itemVolumePacked = algorithmResult.PackedItems.Sum(i => i.Volume);
                decimal itemVolumeUnpacked = algorithmResult.UnpackedItems.Sum(i => i.Volume);

                algorithmResult.PercentContainerVolumePacked = Math.Round(itemVolumePacked / containerVolume * 100, 2);
                algorithmResult.PercentItemVolumePacked = Math.Round(itemVolumePacked / (itemVolumePacked + itemVolumeUnpacked) * 100, 2);

                containerPackingResult.PackingResult = algorithmResult;

                lock (sync)
                {
                    result.Add(containerPackingResult);
                }
            });

            return result;
        }
    }
}
