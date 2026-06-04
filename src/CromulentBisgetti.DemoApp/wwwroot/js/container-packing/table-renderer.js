import { elements } from './dom-elements.js';
import { escapeHtml, fieldLabels } from './formatting.js';
import { state } from './state.js';

function inputCell(entityType, index, field, value) {
    const step = field === 'Quantity' ? '1' : 'any';
    const inputMode = field === 'Quantity' ? 'numeric' : 'decimal';
    const label = `${entityType === 'item' ? 'Предмет' : 'Контейнер'} ${index + 1}: ${fieldLabels[field]}`;

    return `<td class="data-column text-center"><input type="number" min="1" step="${step}" inputmode="${inputMode}" class="form-control form-control-sm text-center" data-entity="${entityType}" data-index="${index}" data-field="${field}" value="${escapeHtml(value)}" aria-label="${escapeHtml(label)}" /></td>`;
}

export function renderAlgorithms() {
    elements.algorithmsToUseBody.innerHTML = state.algorithmTypeIDs.map((algorithm, index) => `
        <tr>
            <td class="button-column">
                <button type="button" class="btn btn-danger btn-sm" data-remove-algorithm="${index}" aria-label="Удалить алгоритм ${index + 1}">×</button>
            </td>
            <td class="algorithm-name-column"><p class="form-control-plaintext mb-0">${escapeHtml(algorithm.AlgorithmName)}</p></td>
        </tr>
    `).join('');
}

export function renderItems() {
    elements.itemsToPackBody.innerHTML = state.itemsToPack.map((item, index) => `
        <tr>
            <td class="button-column">
                <button type="button" class="btn btn-danger btn-sm" data-remove-item="${index}" aria-label="Удалить предмет ${index + 1}">×</button>
            </td>
            ${inputCell('item', index, 'Length', item.Length)}
            ${inputCell('item', index, 'Width', item.Width)}
            ${inputCell('item', index, 'Height', item.Height)}
            ${inputCell('item', index, 'Quantity', item.Quantity)}
        </tr>
    `).join('');
}

function renderAlgorithmResultRows(container, fieldRenderer) {
    return (container.AlgorithmPackingResults ?? []).map((result, resultIndex) => fieldRenderer(result, resultIndex)).join('');
}

function resultCountCell(count, result) {
    const successClass = (result.UnpackedItems?.length ?? 0) === 0 && (result.PackedItems?.length ?? 0) !== 0 ? 'bg-success' : '';
    return `<tr class="text-center"><td class="${successClass}"><p class="form-control-plaintext mb-0">${count}</p></td></tr>`;
}

export function renderContainers() {
    elements.containersBody.innerHTML = state.containers.map((container, containerIndex) => `
        <tr>
            <td class="button-column">
                <button type="button" class="btn btn-danger btn-sm" data-remove-container="${containerIndex}" aria-label="Удалить контейнер ${containerIndex + 1}">×</button>
            </td>
            ${inputCell('container', containerIndex, 'Length', container.Length)}
            ${inputCell('container', containerIndex, 'Width', container.Width)}
            ${inputCell('container', containerIndex, 'Height', container.Height)}
            <td class="button-column gray-cell"></td>
            <td class="algorithm-name-column">
                <table aria-label="Алгоритмы для контейнера ${containerIndex + 1}"><tbody>${renderAlgorithmResultRows(container, result => `<tr><td><p class="form-control-plaintext mb-0">${escapeHtml(result.AlgorithmName)}</p></td></tr>`)}</tbody></table>
            </td>
            <td class="data-column text-center">
                <table class="mx-auto" aria-label="Время упаковки контейнера ${containerIndex + 1}"><tbody>${renderAlgorithmResultRows(container, result => `<tr><td><p class="form-control-plaintext mb-0">${escapeHtml(result.PackTimeInMilliseconds)}</p></td></tr>`)}</tbody></table>
            </td>
            <td class="data-column text-center">
                <table class="mx-auto" aria-label="Процент заполнения контейнера ${containerIndex + 1}"><tbody>${renderAlgorithmResultRows(container, result => `<tr><td><p class="form-control-plaintext mb-0">${escapeHtml(result.PercentContainerVolumePacked)}</p></td></tr>`)}</tbody></table>
            </td>
            <td class="data-column">
                <table class="w-100" aria-label="Количество упакованных предметов для контейнера ${containerIndex + 1}"><tbody>${renderAlgorithmResultRows(container, result => resultCountCell(result.PackedItems?.length ?? 0, result))}</tbody></table>
            </td>
            <td class="data-column">
                <table class="w-100" aria-label="Количество неупакованных предметов для контейнера ${containerIndex + 1}"><tbody>${renderAlgorithmResultRows(container, result => resultCountCell(result.UnpackedItems?.length ?? 0, result))}</tbody></table>
            </td>
            <td class="data-column text-end">
                <table class="ms-auto" aria-label="Визуализация упаковки контейнера ${containerIndex + 1}"><tbody>${renderAlgorithmResultRows(container, (_result, resultIndex) => `
                    <tr><td><button type="button" class="btn btn-link btn-sm" data-show-packing-view data-container-index="${containerIndex}" data-result-index="${resultIndex}" aria-label="Показать визуализацию контейнера ${containerIndex + 1}, результат ${resultIndex + 1}">Показать</button></td></tr>
                `)}</tbody></table>
            </td>
        </tr>
    `).join('');
}

export function render() {
    renderAlgorithms();
    renderItems();
    renderContainers();
}
