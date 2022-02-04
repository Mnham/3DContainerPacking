using System.Runtime.Serialization;

namespace CromulentBisgetti.ContainerPacking.Algorithms
{
    [DataContract]
    public enum AlgorithmType
    {
        /// <summary>
        /// The EB-AFIT packing algorithm type.
        /// </summary>
        [DataMember]
        EB_AFIT = 1
    }
}