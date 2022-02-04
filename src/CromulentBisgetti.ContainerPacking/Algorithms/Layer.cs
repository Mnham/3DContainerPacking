namespace CromulentBisgetti.ContainerPacking.Algorithms
{
    /// <summary>
    /// A list that stores all the different lengths of all item dimensions.
    /// From the master's thesis:
    /// "Each Layerdim value in this array represents a different layer thickness
    /// value with which each iteration can start packing. Before starting iterations,
    /// all different lengths of all box dimensions along with evaluation values are
    /// stored in this array" (p. 3-6).
    /// </summary>
    internal struct Layer
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the layer dimension value, representing a layer thickness.
        /// </summary>
        /// <value>
        /// The layer dimension value.
        /// </value>
        public decimal LayerDim { get; }

        /// <summary>
        /// Gets or sets the layer eval value, representing an evaluation weight
        /// value for the corresponding LayerDim value.
        /// </summary>
        /// <value>
        /// The layer eval value.
        /// </value>
        public decimal LayerEval { get; }

        #endregion Public Properties

        #region Public Constructors

        public Layer(decimal layerDim = 0, decimal layerEval = -1)
        {
            LayerDim = layerDim;
            LayerEval = layerEval;
        }

        #endregion Public Constructors
    }
}