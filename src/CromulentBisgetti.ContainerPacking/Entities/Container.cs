namespace CromulentBisgetti.ContainerPacking.Entities
{
    /// <summary>
    /// The container to pack items into.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the Container class.
    /// </remarks>
    /// <param name="id">The container ID.</param>
    /// <param name="length">The container length.</param>
    /// <param name="width">The container width.</param>
    /// <param name="height">The container height.</param>
    public sealed class Container(int id, decimal length, decimal width, decimal height)
    {
        /// <summary>
        /// Gets or sets the container height.
        /// </summary>
        /// <value>
        /// The container height.
        /// </value>
        public decimal Height { get; } = height;

        /// <summary>
        /// Gets or sets the container ID.
        /// </summary>
        /// <value>
        /// The container ID.
        /// </value>
        public int ID { get; } = id;

        /// <summary>
        /// Gets or sets the container length.
        /// </summary>
        /// <value>
        /// The container length.
        /// </value>
        public decimal Length { get; } = length;

        /// <summary>
        /// Gets or sets the volume of the container.
        /// </summary>
        /// <value>
        /// The volume of the container.
        /// </value>
        public decimal Volume { get; } = length * width * height;

        /// <summary>
        /// Gets or sets the container width.
        /// </summary>
        /// <value>
        /// The container width.
        /// </value>
        public decimal Width { get; } = width;
    }
}
