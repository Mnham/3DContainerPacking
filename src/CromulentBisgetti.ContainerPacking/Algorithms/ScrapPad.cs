namespace CromulentBisgetti.ContainerPacking.Algorithms
{
    /// <summary>
    /// From the master's thesis:
    /// "The double linked list we use keeps the topology of the edge of the
    /// current layer under construction. We keep the x and z coordinates of
    /// each gap's right corner. The program looks at those gaps and tries to
    /// fill them with boxes one at a time while trying to keep the edge of the
    /// layer even" (p. 3-7).
    /// </summary>
    internal sealed class ScrapPad
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the x coordinate of the gap's right corner.
        /// </summary>
        /// <value>
        /// The x coordinate of the gap's right corner.
        /// </value>
        public decimal CumX { get; set; }

        /// <summary>
        /// Gets or sets the z coordinate of the gap's right corner.
        /// </summary>
        /// <value>
        /// The z coordinate of the gap's right corner.
        /// </value>
        public decimal CumZ { get; set; }

        /// <summary>
        /// Gets or sets the following entry.
        /// </summary>
        /// <value>
        /// The following entry.
        /// </value>
        public ScrapPad Post { get; set; }

        /// <summary>
        /// Gets or sets the previous entry.
        /// </summary>
        /// <value>
        /// The previous entry.
        /// </value>
        public ScrapPad Pre { get; set; }

        #endregion Public Properties
    }
}