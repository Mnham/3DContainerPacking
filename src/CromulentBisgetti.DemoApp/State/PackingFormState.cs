using CromulentBisgetti.ContainerPacking;
using CromulentBisgetti.ContainerPacking.Entities;

namespace CromulentBisgetti.DemoApp.State
{
    public interface IFieldValidationTarget
    {
        void SetFieldError(string fieldName, string message);

        void ClearFieldErrors();
    }

    public sealed class PackingFormState
    {
        private int _nextItemId = 1003;
        private int _nextContainerId = 1003;

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

        public void PackContainers()
        {
            PackingErrors.Clear();
            ClearInputValidation();

            List<Container> containers = BuildContainers();
            List<Item> itemsToPack = BuildItemsToPack();

            if (PackingErrors.Count > 0)
            {
                ClearContainerPackingResults();
                return;
            }

            List<ContainerPackingResult> packingResults = PackingService.Pack(containers, itemsToPack);

            foreach (PackingContainerFormModel container in Containers)
            {
                ContainerPackingResult? containerResult = packingResults.FirstOrDefault(result => result.ContainerID == container.Id);
                container.PackingResult = containerResult?.PackingResult;
            }
        }

        public void AddItem()
        {
            Items.Add(new PackingItemFormModel(_nextItemId++));
            ClearPackingResults();
        }

        public void RemoveItem(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
                Items.RemoveAt(index);
                ClearPackingResults();
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
        }

        public void AddContainer()
        {
            Containers.Add(new PackingContainerFormModel(_nextContainerId++));
            ClearPackingResults();
        }

        public void RemoveContainer(int index)
        {
            if (index >= 0 && index < Containers.Count)
            {
                Containers.RemoveAt(index);
                ClearPackingResults();
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
        }

        private List<Container> BuildContainers()
        {
            if (Containers.Count == 0)
            {
                PackingErrors.Add("Добавьте хотя бы один контейнер.");
                return [];
            }

            var containers = new List<Container>();
            for (int index = 0; index < Containers.Count; index++)
            {
                PackingContainerFormModel container = Containers[index];
                string rowLabel = $"Контейнер {index + 1}";
                decimal? length = ValidatePositiveDecimal(container, nameof(container.Length), "Длина", rowLabel, container.Length);
                decimal? width = ValidatePositiveDecimal(container, nameof(container.Width), "Ширина", rowLabel, container.Width);
                decimal? height = ValidatePositiveDecimal(container, nameof(container.Height), "Высота", rowLabel, container.Height);

                if (length is not null && width is not null && height is not null)
                {
                    containers.Add(new Container(container.Id, length.Value, width.Value, height.Value));
                }
            }

            return containers;
        }

        private List<Item> BuildItemsToPack()
        {
            if (Items.Count == 0)
            {
                PackingErrors.Add("Добавьте хотя бы один предмет для упаковки.");
                return [];
            }

            var itemsToPack = new List<Item>();
            for (int index = 0; index < Items.Count; index++)
            {
                PackingItemFormModel item = Items[index];
                string rowLabel = $"Предмет {index + 1}";
                decimal? length = ValidatePositiveDecimal(item, nameof(item.Length), "Длина", rowLabel, item.Length);
                decimal? width = ValidatePositiveDecimal(item, nameof(item.Width), "Ширина", rowLabel, item.Width);
                decimal? height = ValidatePositiveDecimal(item, nameof(item.Height), "Высота", rowLabel, item.Height);
                int? quantity = ValidatePositiveInteger(item, nameof(item.Quantity), "Количество", rowLabel, item.Quantity);

                if (length is not null && width is not null && height is not null && quantity is not null)
                {
                    itemsToPack.Add(new Item(item.Id, length.Value, width.Value, height.Value, quantity.Value));
                }
            }

            return itemsToPack;
        }

        private decimal? ValidatePositiveDecimal(IFieldValidationTarget target, string fieldName, string fieldLabel, string rowLabel, decimal? value)
        {
            if (value is > 0)
            {
                return value;
            }

            AddFieldError(target, fieldName, $"{rowLabel}: поле \"{fieldLabel}\" должно быть числом больше 0.");
            return null;
        }

        private int? ValidatePositiveInteger(IFieldValidationTarget target, string fieldName, string fieldLabel, string rowLabel, int? value)
        {
            if (value is > 0)
            {
                return value;
            }

            AddFieldError(target, fieldName, $"{rowLabel}: поле \"{fieldLabel}\" должно быть целым числом больше 0.");
            return null;
        }

        private void AddFieldError(IFieldValidationTarget target, string fieldName, string message)
        {
            target.SetFieldError(fieldName, message);
            PackingErrors.Add(message);
        }

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
            foreach (PackingItemFormModel item in Items)
            {
                ((IFieldValidationTarget)item).ClearFieldErrors();
            }

            foreach (PackingContainerFormModel container in Containers)
            {
                ((IFieldValidationTarget)container).ClearFieldErrors();
            }
        }
    }

    public sealed class PackingItemFormModel(int id, decimal? length = null, decimal? width = null, decimal? height = null, int? quantity = null) : IFieldValidationTarget
    {
        private readonly Dictionary<string, string> _fieldErrors = [];

        public int Id { get; } = id;

        public decimal? Length { get; set; } = length;

        public decimal? Width { get; set; } = width;

        public decimal? Height { get; set; } = height;

        public int? Quantity { get; set; } = quantity;

        public string? GetFieldError(string fieldName) =>
            _fieldErrors.TryGetValue(fieldName, out string? error) ? error : null;

        public bool HasFieldError(string fieldName) => _fieldErrors.ContainsKey(fieldName);

        void IFieldValidationTarget.SetFieldError(string fieldName, string message) => _fieldErrors[fieldName] = message;

        void IFieldValidationTarget.ClearFieldErrors() => _fieldErrors.Clear();
    }

    public sealed class PackingContainerFormModel(int id, decimal? length = null, decimal? width = null, decimal? height = null) : IFieldValidationTarget
    {
        private readonly Dictionary<string, string> _fieldErrors = [];

        public int Id { get; } = id;

        public decimal? Length { get; set; } = length;

        public decimal? Width { get; set; } = width;

        public decimal? Height { get; set; } = height;

        public AlgorithmPackingResult? PackingResult { get; set; }

        public string? GetFieldError(string fieldName) =>
            _fieldErrors.TryGetValue(fieldName, out string? error) ? error : null;

        public bool HasFieldError(string fieldName) => _fieldErrors.ContainsKey(fieldName);

        void IFieldValidationTarget.SetFieldError(string fieldName, string message) => _fieldErrors[fieldName] = message;

        void IFieldValidationTarget.ClearFieldErrors() => _fieldErrors.Clear();
    }
}
