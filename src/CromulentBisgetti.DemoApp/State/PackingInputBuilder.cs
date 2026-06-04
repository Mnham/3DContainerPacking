using CromulentBisgetti.ContainerPacking.Entities;

namespace CromulentBisgetti.DemoApp.State
{
    internal static class PackingInputBuilder
    {
        public static PackingInput Build(
            IReadOnlyList<PackingContainerFormModel> containers,
            IReadOnlyList<PackingItemFormModel> items)
        {
            var input = new PackingInput();
            BuildContainers(containers, input);
            BuildItemsToPack(items, input);

            return input;
        }

        private static void BuildContainers(IReadOnlyList<PackingContainerFormModel> containers, PackingInput input)
        {
            if (containers.Count == 0)
            {
                input.SummaryErrors.Add("Добавьте хотя бы один контейнер.");
                return;
            }

            for (int index = 0; index < containers.Count; index++)
            {
                PackingContainerFormModel container = containers[index];
                string rowLabel = $"Контейнер {index + 1}";
                decimal? length = ValidatePositiveDecimal(container, nameof(container.Length), "Длина", rowLabel, container.Length, input);
                decimal? width = ValidatePositiveDecimal(container, nameof(container.Width), "Ширина", rowLabel, container.Width, input);
                decimal? height = ValidatePositiveDecimal(container, nameof(container.Height), "Высота", rowLabel, container.Height, input);

                if (length is not null && width is not null && height is not null)
                {
                    input.Containers.Add(new Container(container.Id, length.Value, width.Value, height.Value));
                }
            }
        }

        private static void BuildItemsToPack(IReadOnlyList<PackingItemFormModel> items, PackingInput input)
        {
            if (items.Count == 0)
            {
                input.SummaryErrors.Add("Добавьте хотя бы один предмет для упаковки.");
                return;
            }

            for (int index = 0; index < items.Count; index++)
            {
                PackingItemFormModel item = items[index];
                string rowLabel = $"Предмет {index + 1}";
                decimal? length = ValidatePositiveDecimal(item, nameof(item.Length), "Длина", rowLabel, item.Length, input);
                decimal? width = ValidatePositiveDecimal(item, nameof(item.Width), "Ширина", rowLabel, item.Width, input);
                decimal? height = ValidatePositiveDecimal(item, nameof(item.Height), "Высота", rowLabel, item.Height, input);
                int? quantity = ValidatePositiveInteger(item, nameof(item.Quantity), "Количество", rowLabel, item.Quantity, input);

                if (length is not null && width is not null && height is not null && quantity is not null)
                {
                    input.ItemsToPack.Add(new Item(item.Id, length.Value, width.Value, height.Value, quantity.Value));
                }
            }
        }

        private static decimal? ValidatePositiveDecimal(
            object model,
            string fieldName,
            string fieldLabel,
            string rowLabel,
            decimal? value,
            PackingInput input)
        {
            if (value is > 0)
            {
                return value;
            }

            input.FieldErrors.Add(new PackingFieldValidationError(model, fieldName, $"{rowLabel}: поле \"{fieldLabel}\" должно быть числом больше 0."));
            return null;
        }

        private static int? ValidatePositiveInteger(
            object model,
            string fieldName,
            string fieldLabel,
            string rowLabel,
            int? value,
            PackingInput input)
        {
            if (value is > 0)
            {
                return value;
            }

            input.FieldErrors.Add(new PackingFieldValidationError(model, fieldName, $"{rowLabel}: поле \"{fieldLabel}\" должно быть целым числом больше 0."));
            return null;
        }
    }

    internal sealed record PackingFieldValidationError(object Model, string FieldName, string Message);

    internal sealed class PackingInput
    {
        public List<Container> Containers { get; } = [];

        public List<Item> ItemsToPack { get; } = [];

        public List<string> SummaryErrors { get; } = [];

        public List<PackingFieldValidationError> FieldErrors { get; } = [];

        public bool IsValid => SummaryErrors.Count == 0 && FieldErrors.Count == 0;
    }
}
