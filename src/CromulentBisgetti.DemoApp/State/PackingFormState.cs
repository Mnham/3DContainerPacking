using CromulentBisgetti.ContainerPacking;
using CromulentBisgetti.ContainerPacking.Entities;
using Microsoft.AspNetCore.Components.Forms;

namespace CromulentBisgetti.DemoApp.State
{
    public sealed class PackingFormState
    {
        private readonly ValidationMessageStore _validationMessages;
        private int _nextItemId = 1003;
        private int _nextContainerId = 1003;

        public PackingFormState()
        {
            EditContext = new EditContext(this);
            EditContext.SetFieldCssClassProvider(new BootstrapFieldCssClassProvider());
            EditContext.OnFieldChanged += OnFieldChanged;
            _validationMessages = new ValidationMessageStore(EditContext);
        }

        public event Action? Changed;

        public EditContext EditContext { get; }

        public List<PackingItemFormModel> Items { get; private set; } =
        [
            new(1000, 5, 4, 2, 1),
            new(1001, 2, 1, 1, 3),
            new(1002, 9, 7, 3, 4)
        ];

        public List<PackingContainerFormModel> Containers { get; private set; } =
        [
            new(1000, 15, 13, 9),
            new(1001, 23, 9, 4),
            new(1002, 16, 16, 6)
        ];

        public List<string> PackingErrors { get; } = [];

        public bool IsPacking { get; private set; }

        public void SetPackingInProgress(bool isPacking)
        {
            if (IsPacking == isPacking)
            {
                return;
            }

            IsPacking = isPacking;
            NotifyChanged();
        }

        public async Task PackAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            PrepareForPacking();

            PackingInput input = ValidateInput();
            if (!input.IsValid)
            {
                ApplyInputValidationErrors(input.FieldErrors);
                ApplyPackingErrors(input.SummaryErrors);
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            List<ContainerPackingResult> packingResults = await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    List<ContainerPackingResult> results = PackingService.Pack(input.Containers, input.ItemsToPack);
                    cancellationToken.ThrowIfCancellationRequested();
                    return results;
                },
                cancellationToken);

            ApplyPackingResults(packingResults);
        }

        public void AddItem()
        {
            Items.Add(new PackingItemFormModel(_nextItemId++));
            ClearPackingResults();
            NotifyChanged();
        }

        public void RemoveItem(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
                Items.RemoveAt(index);
                ClearPackingResults();
                NotifyChanged();
            }
        }

        public void GenerateSampleItems()
        {
            Items =
            [
                new(1000, 5, 4, 2, 1),
                new(1001, 2, 1, 1, 3),
                new(1002, 9, 7, 3, 4),
                new(1003, 13, 6, 3, 8),
                new(1004, 17, 8, 6, 1),
                new(1005, 3, 3, 2, 2)
            ];

            _nextItemId = 1006;
            ClearPackingResults();
            NotifyChanged();
        }

        public void AddContainer()
        {
            Containers.Add(new PackingContainerFormModel(_nextContainerId++));
            ClearPackingResults();
            NotifyChanged();
        }

        public void RemoveContainer(int index)
        {
            if (index >= 0 && index < Containers.Count)
            {
                Containers.RemoveAt(index);
                ClearPackingResults();
                NotifyChanged();
            }
        }

        public void GenerateSampleContainers()
        {
            Containers =
            [
                new(1000, 15, 13, 9),
                new(1001, 23, 9, 4),
                new(1002, 16, 16, 6),
                new(1003, 10, 8, 5),
                new(1004, 40, 28, 20),
                new(1005, 29, 19, 4),
                new(1006, 18, 13, 1),
                new(1007, 6, 6, 6),
                new(1008, 8, 5, 5),
                new(1009, 18, 13, 8),
                new(1010, 17, 16, 15),
                new(1011, 32, 10, 9),
                new(1012, 60, 60, 60)
            ];

            _nextContainerId = 1013;
            ClearPackingResults();
            NotifyChanged();
        }

        private void PrepareForPacking()
        {
            PackingErrors.Clear();
            ClearInputValidation();
            NotifyChanged();
        }

        public void ApplyPackingErrors(IEnumerable<string> errors)
        {
            PackingErrors.Clear();
            PackingErrors.AddRange(errors);
            ClearContainerPackingResults();
            NotifyChanged();
        }

        private void ApplyInputValidationErrors(IEnumerable<PackingFieldValidationError> errors)
        {
            _validationMessages.Clear();

            foreach (PackingFieldValidationError error in errors)
            {
                _validationMessages.Add(new FieldIdentifier(error.Model, error.FieldName), error.Message);
            }

            EditContext.NotifyValidationStateChanged();
            NotifyChanged();
        }

        private void ApplyPackingResults(IReadOnlyCollection<ContainerPackingResult> packingResults)
        {
            PackingErrors.Clear();

            foreach (PackingContainerFormModel container in Containers)
            {
                ContainerPackingResult? containerResult = packingResults.FirstOrDefault(result => result.ContainerID == container.Id);
                container.PackingResult = containerResult?.PackingResult;
            }

            NotifyChanged();
        }

        public bool HasFieldError(object model, string fieldName) =>
            EditContext.GetValidationMessages(new FieldIdentifier(model, fieldName)).Any();

        public string? GetFieldError(object model, string fieldName) =>
            EditContext.GetValidationMessages(new FieldIdentifier(model, fieldName)).FirstOrDefault();

        private void NotifyChanged() => Changed?.Invoke();

        private void ClearPackingResults()
        {
            PackingErrors.Clear();
            ClearInputValidation();
            ClearContainerPackingResults();
        }

        private void ClearContainerPackingResults()
        {
            foreach (PackingContainerFormModel container in Containers)
            {
                container.PackingResult = null;
            }
        }

        private void ClearInputValidation()
        {
            _validationMessages.Clear();
            EditContext.NotifyValidationStateChanged();
        }

        private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
        {
            _validationMessages.Clear(args.FieldIdentifier);
            PackingErrors.Clear();
            ClearContainerPackingResults();
            EditContext.NotifyValidationStateChanged();
            NotifyChanged();
        }

        private PackingInput ValidateInput()
        {
            var input = new PackingInput();
            BuildContainers(input);
            BuildItemsToPack(input);

            return input;
        }

        private void BuildContainers(PackingInput input)
        {
            if (Containers.Count == 0)
            {
                input.SummaryErrors.Add("Добавьте хотя бы один контейнер.");
                return;
            }

            for (int index = 0; index < Containers.Count; index++)
            {
                PackingContainerFormModel container = Containers[index];
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

        private void BuildItemsToPack(PackingInput input)
        {
            if (Items.Count == 0)
            {
                input.SummaryErrors.Add("Добавьте хотя бы один предмет для упаковки.");
                return;
            }

            for (int index = 0; index < Items.Count; index++)
            {
                PackingItemFormModel item = Items[index];
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

        private sealed record PackingFieldValidationError(object Model, string FieldName, string Message);

        private sealed class PackingInput
        {
            public List<Container> Containers { get; } = [];

            public List<Item> ItemsToPack { get; } = [];

            public List<string> SummaryErrors { get; } = [];

            public List<PackingFieldValidationError> FieldErrors { get; } = [];

            public bool IsValid => SummaryErrors.Count == 0 && FieldErrors.Count == 0;
        }
    }

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
