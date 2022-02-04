using System;

namespace CromulentBisgetti.ContainerPacking.Entities
{
    /// <summary>
    /// An item to be packed. Also used to hold post-packing details for the item.
    /// </summary>
    public sealed class Item
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the x coordinate of the location of the packed item within the container.
        /// </summary>
        /// <value>
        /// The x coordinate of the location of the packed item within the container.
        /// </value>
        public decimal CoordX { get; set; }

        /// <summary>
        /// Gets or sets the y coordinate of the location of the packed item within the container.
        /// </summary>
        /// <value>
        /// The y coordinate of the location of the packed item within the container.
        /// </value>
        public decimal CoordY { get; set; }

        /// <summary>
        /// Gets or sets the z coordinate of the location of the packed item within the container.
        /// </summary>
        /// <value>
        /// The z coordinate of the location of the packed item within the container.
        /// </value>
        public decimal CoordZ { get; set; }

        /// <summary>
        /// Gets or sets the length of one of the item dimensions.
        /// </summary>
        /// <value>
        /// The first item dimension.
        /// </value>
        public decimal Dim1 { get; set; }

        /// <summary>
        /// Gets or sets the length another of the item dimensions.
        /// </summary>
        /// <value>
        /// The second item dimension.
        /// </value>
        public decimal Dim2 { get; set; }

        /// <summary>
        /// Gets or sets the third of the item dimensions.
        /// </summary>
        /// <value>
        /// The third item dimension.
        /// </value>
        public decimal Dim3 { get; set; }

        /// <summary>
        /// Gets or sets the item ID.
        /// </summary>
        /// <value>
        /// The item ID.
        /// </value>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item has already been packed.
        /// </summary>
        /// <value>
        ///   True if the item has already been packed; otherwise, false.
        /// </value>
        public bool IsPacked { get; set; }

        /// <summary>
        /// Gets or sets the x dimension of the orientation of the item as it has been packed.
        /// </summary>
        /// <value>
        /// The x dimension of the orientation of the item as it has been packed.
        /// </value>
        public decimal PackDimX { get; set; }

        /// <summary>
        /// Gets or sets the y dimension of the orientation of the item as it has been packed.
        /// </summary>
        /// <value>
        /// The y dimension of the orientation of the item as it has been packed.
        /// </value>
        public decimal PackDimY { get; set; }

        /// <summary>
        /// Gets or sets the z dimension of the orientation of the item as it has been packed.
        /// </summary>
        /// <value>
        /// The z dimension of the orientation of the item as it has been packed.
        /// </value>
        public decimal PackDimZ { get; set; }

        /// <summary>
        /// Gets or sets the item quantity.
        /// </summary>
        /// <value>
        /// The item quantity.
        /// </value>
        public int Quantity { get; set; }

        public decimal TotalVolume { get; set; }

        /// <summary>
        /// Gets the item volume.
        /// </summary>
        /// <value>
        /// The item volume.
        /// </value>
        public decimal Volume { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public Item()
        { }

        public Item(Item item)
        {
            ID = item.ID;
            Dim1 = item.Dim1;
            Dim2 = item.Dim2;
            Dim3 = item.Dim3;
            Volume = item.Volume;
            Quantity = item.Quantity;
        }

        /// <summary>
        /// Initializes a new instance of the Item class.
        /// </summary>
        /// <param name="id">The item ID.</param>
        /// <param name="dim1">The length of one of the three item dimensions.</param>
        /// <param name="dim2">The length of another of the three item dimensions.</param>
        /// <param name="dim3">The length of the other of the three item dimensions.</param>
        /// <param name="itemQuantity">The item quantity.</param>
        public Item(int id, decimal dim1, decimal dim2, decimal dim3, int quantity)
        {
            ID = id;
            Dim1 = dim1;
            Dim2 = dim2;
            Dim3 = dim3;
            Volume = dim1 * dim2 * dim3;
            TotalVolume = Volume * quantity;
            Quantity = quantity;
        }

        #endregion Public Constructors

        #region Public Methods

        public decimal GetDimDif(decimal exDim)
        {
            decimal dimDif = Math.Abs(exDim - Dim1);
            if (Math.Abs(exDim - Dim2) < dimDif)
            {
                dimDif = Math.Abs(exDim - Dim2);
            }

            if (Math.Abs(exDim - Dim3) < dimDif)
            {
                dimDif = Math.Abs(exDim - Dim3);
            }

            return dimDif;
        }

        public decimal GetMinDim() => Math.Min(Math.Min(Dim1, Dim2), Dim3);

        public override string ToString() => $"{ID} L{Dim1} W{Dim2} H{Dim3} Q{Quantity}";

        #endregion Public Methods
    }
}