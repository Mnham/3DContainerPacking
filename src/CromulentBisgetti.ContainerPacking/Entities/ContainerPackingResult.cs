﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CromulentBisgetti.ContainerPacking.Entities
{
    /// <summary>
    /// The container packing result.
    /// </summary>
    [DataContract]
    public sealed class ContainerPackingResult
    {
        #region Public Properties

        [DataMember]
        public List<AlgorithmPackingResult> AlgorithmPackingResults { get; set; } = new List<AlgorithmPackingResult>();

        /// <summary>
        /// Gets or sets the container ID.
        /// </summary>
        /// <value>
        /// The container ID.
        /// </value>
        [DataMember]
        public int ContainerID { get; set; }

        #endregion Public Properties
    }
}