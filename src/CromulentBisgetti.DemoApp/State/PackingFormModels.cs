using CromulentBisgetti.ContainerPacking.Entities;
using Microsoft.AspNetCore.Components.Forms;

namespace CromulentBisgetti.DemoApp.State
{
    public sealed class PackingItemFormModel(int id, decimal? length = null, decimal? width = null, decimal? height = null, int? quantity = null)
    {
        public int Id { get; } = id;

        public decimal? Length { get; set; } = length;

        public decimal? Width { get; set; } = width;

        public decimal? Height { get; set; } = height;

        public int? Quantity { get; set; } = quantity;
    }

    public sealed class PackingContainerFormModel(int id, decimal? length = null, decimal? width = null, decimal? height = null)
    {
        public int Id { get; } = id;

        public decimal? Length { get; set; } = length;

        public decimal? Width { get; set; } = width;

        public decimal? Height { get; set; } = height;

        public AlgorithmPackingResult? PackingResult { get; set; }
    }

    internal sealed class BootstrapFieldCssClassProvider : FieldCssClassProvider
    {
        public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier) =>
            editContext.GetValidationMessages(fieldIdentifier).Any() ? "is-invalid" : string.Empty;
    }
}
