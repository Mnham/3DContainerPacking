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
        EB_AFIT = 1,

        /// <summary>
        /// Улучшенный EB-AFIT алгоритм.
        /// </summary>
        EB_AFIT_improved = 2,

        /// <summary>
        /// Только вертикальная упаковка.
        /// </summary>
        XYZRotationVertical = 3,

        /// <summary>
        /// Упаковка с вращением только по оси Z.
        /// </summary>
        ZRotation = 4,

        /// <summary>
        /// Без вращения.
        /// </summary>
        WithoutRotation = 5,
    }
}