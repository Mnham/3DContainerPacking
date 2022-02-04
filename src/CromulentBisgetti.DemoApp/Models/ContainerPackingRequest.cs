using CromulentBisgetti.ContainerPacking.Entities;

using System.Collections.Generic;

namespace CromulentBisgetti.DemoApp.Models
{
    public sealed class ContainerPackingRequest
    {
        #region Public Properties

        public List<int> AlgorithmTypeIDs { get; set; }
        public List<Container> Containers { get; set; }

        public List<Item> ItemsToPack { get; set; }

        #endregion Public Properties
    }
}