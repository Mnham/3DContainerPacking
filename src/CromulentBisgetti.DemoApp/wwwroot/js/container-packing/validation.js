import { fieldLabels } from './formatting.js';
import { state } from './state.js';

export function clearInputValidation() {
    document.querySelectorAll('input[data-entity].is-invalid').forEach(input => {
        input.classList.remove('is-invalid');
        input.removeAttribute('aria-invalid');
        input.removeAttribute('title');
    });
}

function findInput(entity, index, field) {
    return document.querySelector(`input[data-entity="${entity}"][data-index="${index}"][data-field="${field}"]`);
}

function parsePositiveNumber(value) {
    if (typeof value === 'number') {
        return Number.isFinite(value) && value > 0 ? value : null;
    }

    if (typeof value !== 'string' || value.trim() === '') {
        return null;
    }

    const parsed = Number(value);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : null;
}

function validatePositiveField(errors, entity, rowLabel, index, field, value, requireInteger = false) {
    const parsed = parsePositiveNumber(value);
    const isValid = parsed !== null && (!requireInteger || Number.isInteger(parsed));

    if (!isValid) {
        const expectedValue = requireInteger ? 'целым числом больше 0' : 'числом больше 0';
        errors.push({ entity, index, field, message: `${rowLabel}: поле "${fieldLabels[field]}" должно быть ${expectedValue}.` });
        return null;
    }

    return parsed;
}

export function validatePackingInput() {
    const errors = [];
    const containers = [];
    const itemsToPack = [];

    if (state.algorithmTypeIDs.length === 0) {
        errors.push({ message: 'Добавьте хотя бы один алгоритм упаковки.' });
    }

    if (state.containers.length === 0) {
        errors.push({ message: 'Добавьте хотя бы один контейнер.' });
    }

    if (state.itemsToPack.length === 0) {
        errors.push({ message: 'Добавьте хотя бы один предмет для упаковки.' });
    }

    state.containers.forEach((container, index) => {
        const rowLabel = `Контейнер ${index + 1}`;
        const length = validatePositiveField(errors, 'container', rowLabel, index, 'Length', container.Length);
        const width = validatePositiveField(errors, 'container', rowLabel, index, 'Width', container.Width);
        const height = validatePositiveField(errors, 'container', rowLabel, index, 'Height', container.Height);

        if (length !== null && width !== null && height !== null) {
            containers.push({
                ID: container.ID,
                Length: length,
                Width: width,
                Height: height
            });
        }
    });

    state.itemsToPack.forEach((item, index) => {
        const rowLabel = `Предмет ${index + 1}`;
        const length = validatePositiveField(errors, 'item', rowLabel, index, 'Length', item.Length);
        const width = validatePositiveField(errors, 'item', rowLabel, index, 'Width', item.Width);
        const height = validatePositiveField(errors, 'item', rowLabel, index, 'Height', item.Height);
        const quantity = validatePositiveField(errors, 'item', rowLabel, index, 'Quantity', item.Quantity, true);

        if (length !== null && width !== null && height !== null && quantity !== null) {
            itemsToPack.push({
                ID: item.ID,
                Dim1: length,
                Dim2: width,
                Dim3: height,
                Quantity: quantity
            });
        }
    });

    return {
        errors,
        request: {
            Containers: containers,
            ItemsToPack: itemsToPack,
            AlgorithmTypeIDs: state.algorithmTypeIDs.map(algorithm => algorithm.AlgorithmID)
        }
    };
}

export function showValidationErrors(errors) {
    errors.forEach(error => {
        if (!error.entity) {
            return;
        }

        const input = findInput(error.entity, error.index, error.field);
        if (input) {
            input.classList.add('is-invalid');
            input.setAttribute('aria-invalid', 'true');
            input.title = error.message;
        }
    });

    alert([...new Set(errors.map(error => error.message))].join('\n'));
    const firstFieldError = errors.find(error => error.entity);
    const firstInvalidInput = firstFieldError ? findInput(firstFieldError.entity, firstFieldError.index, firstFieldError.field) : null;
    firstInvalidInput?.focus();
}
