using CromulentBisgetti.ContainerPacking.Entities;

namespace CromulentBisgetti.DemoApp.Models
{
    public sealed class ContainerPackingRequest
    {
        public List<int> AlgorithmTypeIDs { get; set; } = new List<int>();
        public List<Container> Containers { get; set; } = new List<Container>();

        public List<Item> ItemsToPack { get; set; } = new List<Item>();
    }
}
