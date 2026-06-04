import { packContainers as packContainersApi } from './container-packing/api-client.js';
import { elements } from './container-packing/dom-elements.js';
import { getSampleContainers, getSampleItemsToPack } from './container-packing/sample-data.js';
import { state } from './container-packing/state.js';
import { render, renderAlgorithms, renderContainers, renderItems } from './container-packing/table-renderer.js';
import { clearInputValidation, showValidationErrors, validatePackingInput } from './container-packing/validation.js';

let packingViewerModulePromise;

function loadPackingViewer() {
    packingViewerModulePromise ??= import('./container-packing/packing-viewer.js');
    return packingViewerModulePromise;
}

function addAlgorithmToUse() {
    const selectedOption = elements.algorithmSelect.options[elements.algorithmSelect.selectedIndex];
    state.algorithmTypeIDs.push({
        AlgorithmID: Number(selectedOption.value),
        AlgorithmName: selectedOption.text
    });
    renderAlgorithms();
}

function generateItemsToPack() {
    state.itemsToPack = getSampleItemsToPack();
    renderItems();
}

function generateContainers() {
    state.containers = getSampleContainers();
    renderContainers();
}

function addNewItemToPack() {
    state.itemsToPack.push({ ID: state.itemCounter++, Name: '', Length: '', Width: '', Height: '', Quantity: '' });
    renderItems();
}

function addNewContainer() {
    state.containers.push({ ID: state.containerCounter++, Name: '', Length: '', Width: '', Height: '', AlgorithmPackingResults: [] });
    renderContainers();
}

async function packContainers() {
    clearInputValidation();

    const validationResult = validatePackingInput();
    if (validationResult.errors.length > 0) {
        showValidationErrors(validationResult.errors);
        return;
    }

    const packingResults = await packContainersApi(validationResult.request);
    packingResults.forEach(containerPackingResult => {
        const container = state.containers.find(current => current.ID == containerPackingResult.ContainerID);
        if (container) {
            container.AlgorithmPackingResults = containerPackingResult.AlgorithmPackingResults;
        }
    });

    renderContainers();
}

function handleTableInput(event) {
    const input = event.target.closest('input[data-entity]');
    if (!input) {
        return;
    }

    input.classList.remove('is-invalid');
    input.removeAttribute('aria-invalid');
    input.removeAttribute('title');

    const collection = input.dataset.entity === 'item' ? state.itemsToPack : state.containers;
    collection[Number(input.dataset.index)][input.dataset.field] = input.value;
}

function handleContentClick(event) {
    const algorithmButton = event.target.closest('[data-remove-algorithm]');
    const itemButton = event.target.closest('[data-remove-item]');
    const containerButton = event.target.closest('[data-remove-container]');
    const showPackingButton = event.target.closest('[data-show-packing-view]');

    if (algorithmButton) {
        state.algorithmTypeIDs.splice(Number(algorithmButton.dataset.removeAlgorithm), 1);
        renderAlgorithms();
    } else if (itemButton) {
        state.itemsToPack.splice(Number(itemButton.dataset.removeItem), 1);
        renderItems();
    } else if (containerButton) {
        state.containers.splice(Number(containerButton.dataset.removeContainer), 1);
        renderContainers();
    } else if (showPackingButton) {
        loadPackingViewer()
            .then(({ openPackingView }) => openPackingView(Number(showPackingButton.dataset.containerIndex), Number(showPackingButton.dataset.resultIndex)))
            .catch(error => {
                console.error(error);
                alert(error.message || 'Не удалось открыть визуализацию. Подробности смотрите в консоли браузера.');
            });
    }
}

function initializeUi() {
    elements.addAlgorithmButton.addEventListener('click', addAlgorithmToUse);
    elements.generateItemsButton.addEventListener('click', generateItemsToPack);
    elements.addItemButton.addEventListener('click', addNewItemToPack);
    elements.generateContainersButton.addEventListener('click', generateContainers);
    elements.addContainerButton.addEventListener('click', addNewContainer);
    elements.packContainersButton.addEventListener('click', async () => {
        elements.packContainersButton.disabled = true;

        try {
            await packContainers();
        } catch (error) {
            console.error(error);
            alert(error.message || 'Не удалось выполнить упаковку. Подробности смотрите в консоли браузера.');
        } finally {
            elements.packContainersButton.disabled = false;
        }
    });
    elements.packRenderItemButton.addEventListener('click', () => {
        loadPackingViewer().then(({ packItemInRender }) => packItemInRender());
    });
    elements.unpackRenderItemButton.addEventListener('click', () => {
        loadPackingViewer().then(({ unpackItemInRender }) => unpackItemInRender());
    });
    elements.content.addEventListener('click', handleContentClick);
    elements.content.addEventListener('input', handleTableInput);
    elements.renderModal.addEventListener('shown.bs.modal', () => {
        loadPackingViewer()
            .then(({ renderSelectedPackingView }) => renderSelectedPackingView())
            .catch(error => {
                console.error(error);
                alert('Не удалось открыть визуализацию. Подробности смотрите в консоли браузера.');
            });
    });
    elements.renderModal.addEventListener('hidden.bs.modal', () => {
        if (!packingViewerModulePromise) {
            return;
        }

        loadPackingViewer().then(({ closePackingView }) => closePackingView());
    });
}

document.addEventListener('DOMContentLoaded', () => {
    initializeUi();
    render();
});
