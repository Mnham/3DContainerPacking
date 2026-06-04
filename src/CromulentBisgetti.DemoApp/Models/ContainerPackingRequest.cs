using CromulentBisgetti.ContainerPacking.Entities;

using System.Collections.Generic;

namespace CromulentBisgetti.DemoApp.Models
{
    public sealed class ContainerPackingRequest
    {
        #region Public Properties

        public List<int> AlgorithmTypeIDs { get; set; } = new List<int>();
        public List<Container> Containers { get; set; } = new List<Container>();

        public List<Item> ItemsToPack { get; set; } = new List<Item>();

        #endregion Public Properties
    }
}