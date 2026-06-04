import { elements } from './dom-elements.js';
import { state } from './state.js';

let initialized = false;
let packingViewerModulePromise;

function loadPackingViewer() {
    packingViewerModulePromise ??= import('./packing-viewer.js');
    return packingViewerModulePromise;
}

function readValue(source, ...propertyNames) {
    for (const propertyName of propertyNames) {
        if (source?.[propertyName] !== undefined) {
            return source[propertyName];
        }
    }

    return undefined;
}

function readNumber(source, ...propertyNames) {
    return Number(readValue(source, ...propertyNames) ?? 0);
}

function normalizeItem(item) {
    return {
        ID: readNumber(item, 'ID', 'id'),
        CoordX: readNumber(item, 'CoordX', 'coordX'),
        CoordY: readNumber(item, 'CoordY', 'coordY'),
        CoordZ: readNumber(item, 'CoordZ', 'coordZ'),
        PackDimX: readNumber(item, 'PackDimX', 'packDimX'),
        PackDimY: readNumber(item, 'PackDimY', 'packDimY'),
        PackDimZ: readNumber(item, 'PackDimZ', 'packDimZ')
    };
}

function normalizeResult(result) {
    return {
        PackTimeInMilliseconds: readNumber(result, 'PackTimeInMilliseconds', 'packTimeInMilliseconds'),
        PercentContainerVolumePacked: readNumber(result, 'PercentContainerVolumePacked', 'percentContainerVolumePacked'),
        PackedItems: (readValue(result, 'PackedItems', 'packedItems') ?? []).map(normalizeItem),
        UnpackedItems: (readValue(result, 'UnpackedItems', 'unpackedItems') ?? []).map(normalizeItem)
    };
}

function normalizeContainer(container) {
    return {
        Length: readNumber(container, 'Length', 'length'),
        Width: readNumber(container, 'Width', 'width'),
        Height: readNumber(container, 'Height', 'height'),
        PackingResult: normalizeResult(readValue(container, 'PackingResult', 'packingResult'))
    };
}

async function renderSelectedPackingView() {
    const { renderSelectedPackingView: renderView } = await loadPackingViewer();
    await renderView();
}

async function closePackingView() {
    if (!packingViewerModulePromise) {
        return;
    }

    const { closePackingView: closeView } = await loadPackingViewer();
    closeView();
}

function initialize() {
    if (initialized) {
        return;
    }

    elements.packRenderItemButton.addEventListener('click', async () => {
        const { packItemInRender } = await loadPackingViewer();
        packItemInRender();
    });

    elements.unpackRenderItemButton.addEventListener('click', async () => {
        const { unpackItemInRender } = await loadPackingViewer();
        unpackItemInRender();
    });

    elements.renderModal.addEventListener('shown.bs.modal', () => {
        renderSelectedPackingView().catch(error => {
            console.error(error);
            alert('Не удалось открыть визуализацию. Подробности смотрите в консоли браузера.');
        });
    });

    elements.renderModal.addEventListener('hidden.bs.modal', () => {
        closePackingView().catch(error => console.error(error));
    });

    initialized = true;
}

export async function openPackingView(container) {
    initialize();
    state.containers = [normalizeContainer(container)];

    const { openPackingView: openView } = await loadPackingViewer();
    openView(0);
}
