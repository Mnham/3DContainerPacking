using System.Runtime.Serialization;

namespace CromulentBisgetti.ContainerPacking.Entities
{
    /// <summary>
    /// The container packing result.
    /// </summary>
    [DataContract]
    public sealed class ContainerPackingResult
    {
        [DataMember]
        public AlgorithmPackingResult PackingResult { get; set; } = new AlgorithmPackingResult();

        /// <summary>
        /// Gets or sets the container ID.
        /// </summary>
        /// <value>
        /// The container ID.
        /// </value>
        [DataMember]
        public int ContainerID { get; set; }
    }
}
