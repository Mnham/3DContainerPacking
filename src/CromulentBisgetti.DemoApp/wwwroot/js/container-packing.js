const state = {
    algorithmTypeIDs: [],
    itemsToPack: [],
    containers: [],
    itemCounter: 0,
    containerCounter: 0,
    itemsToRender: [],
    lastItemRenderedIndex: -1,
    containerOriginOffset: { x: 0, y: 0, z: 0 }
};

let THREE;
let OrbitControls;
let drawingInitialized = false;
let scene;
let camera;
let renderer;
let controls;
let itemMaterial;

const algorithmSelect = document.getElementById('algorithm-select');
const algorithmsToUseBody = document.getElementById('algorithms-to-use');
const itemsToPackBody = document.getElementById('items-to-pack');
const containersBody = document.getElementById('containers');
const packRenderItemButton = document.getElementById('pack-render-item');
const unpackRenderItemButton = document.getElementById('unpack-render-item');

async function loadThree() {
    if (THREE && OrbitControls) {
        return;
    }

    const [threeModule, orbitControlsModule] = await Promise.all([
        import('https://esm.sh/three@0.184.0'),
        import('https://esm.sh/three@0.184.0/examples/jsm/controls/OrbitControls.js')
    ]);

    THREE = threeModule;
    OrbitControls = orbitControlsModule.OrbitControls;
}

function escapeHtml(value) {
    return String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}

function inputCell(entityType, index, field, value) {
    return `<td class="data-column text-center"><input type="text" class="form-control form-control-sm text-center" data-entity="${entityType}" data-index="${index}" data-field="${field}" value="${escapeHtml(value)}" /></td>`;
}

function renderAlgorithms() {
    algorithmsToUseBody.innerHTML = state.algorithmTypeIDs.map((algorithm, index) => `
        <tr>
            <td class="button-column">
                <button class="btn btn-danger btn-sm" data-remove-algorithm="${index}" aria-label="Удалить алгоритм">×</button>
            </td>
            <td class="algorithm-name-column"><p class="form-control-plaintext mb-0">${escapeHtml(algorithm.AlgorithmName)}</p></td>
        </tr>
    `).join('');
}

function renderItems() {
    itemsToPackBody.innerHTML = state.itemsToPack.map((item, index) => `
        <tr>
            <td class="button-column">
                <button class="btn btn-danger btn-sm" data-remove-item="${index}" aria-label="Удалить предмет">×</button>
            </td>
            ${inputCell('item', index, 'Length', item.Length)}
            ${inputCell('item', index, 'Width', item.Width)}
            ${inputCell('item', index, 'Height', item.Height)}
            ${inputCell('item', index, 'Quantity', item.Quantity)}
        </tr>
    `).join('');
}

function renderAlgorithmResultRows(containerIndex, container, fieldRenderer) {
    return (container.AlgorithmPackingResults ?? []).map((result, resultIndex) => fieldRenderer(result, resultIndex, containerIndex)).join('');
}

function renderContainers() {
    containersBody.innerHTML = state.containers.map((container, containerIndex) => `
        <tr>
            <td class="button-column">
                <button class="btn btn-danger btn-sm" data-remove-container="${containerIndex}" aria-label="Удалить контейнер">×</button>
            </td>
            ${inputCell('container', containerIndex, 'Length', container.Length)}
            ${inputCell('container', containerIndex, 'Width', container.Width)}
            ${inputCell('container', containerIndex, 'Height', container.Height)}
            <td class="button-column gray-cell"></td>
            <td class="algorithm-name-column">
                <table><tbody>${renderAlgorithmResultRows(containerIndex, container, result => `<tr><td><p class="form-control-plaintext mb-0">${escapeHtml(result.AlgorithmName)}</p></td></tr>`)}</tbody></table>
            </td>
            <td class="data-column text-center">
                <table class="mx-auto"><tbody>${renderAlgorithmResultRows(containerIndex, container, result => `<tr><td><p class="form-control-plaintext mb-0">${escapeHtml(result.PackTimeInMilliseconds)}</p></td></tr>`)}</tbody></table>
            </td>
            <td class="data-column text-center">
                <table class="mx-auto"><tbody>${renderAlgorithmResultRows(containerIndex, container, result => `<tr><td><p class="form-control-plaintext mb-0">${escapeHtml(result.PercentContainerVolumePacked)}</p></td></tr>`)}</tbody></table>
            </td>
            <td class="data-column">
                <table class="w-100"><tbody>${renderAlgorithmResultRows(containerIndex, container, result => resultCountCell(result.PackedItems?.length ?? 0, result))}</tbody></table>
            </td>
            <td class="data-column">
                <table class="w-100"><tbody>${renderAlgorithmResultRows(containerIndex, container, result => resultCountCell(result.UnpackedItems?.length ?? 0, result))}</tbody></table>
            </td>
            <td class="data-column text-end">
                <table class="ms-auto"><tbody>${renderAlgorithmResultRows(containerIndex, container, (_result, resultIndex) => `
                    <tr><td><button class="btn btn-link btn-sm" data-show-packing-view data-container-index="${containerIndex}" data-result-index="${resultIndex}" data-bs-toggle="modal" data-bs-target="#renderModal">View</button></td></tr>
                `)}</tbody></table>
            </td>
        </tr>
    `).join('');
}

function resultCountCell(count, result) {
    const successClass = (result.UnpackedItems?.length ?? 0) === 0 && (result.PackedItems?.length ?? 0) !== 0 ? 'bg-success' : '';
    return `<tr class="text-center"><td class="${successClass}"><p class="form-control-plaintext mb-0">${count}</p></td></tr>`;
}

function render() {
    renderAlgorithms();
    renderItems();
    renderContainers();
}

function addAlgorithmToUse() {
    const selectedOption = algorithmSelect.options[algorithmSelect.selectedIndex];
    state.algorithmTypeIDs.push({
        AlgorithmID: Number(selectedOption.value),
        AlgorithmName: selectedOption.text
    });
    renderAlgorithms();
}

function generateItemsToPack() {
    state.itemsToPack = [
        { ID: 1000, Name: 'Item1', Length: 5, Width: 4, Height: 2, Quantity: 1 },
        { ID: 1001, Name: 'Item2', Length: 2, Width: 1, Height: 1, Quantity: 3 },
        { ID: 1002, Name: 'Item3', Length: 9, Width: 7, Height: 3, Quantity: 4 },
        { ID: 1003, Name: 'Item4', Length: 13, Width: 6, Height: 3, Quantity: 8 },
        { ID: 1004, Name: 'Item5', Length: 17, Width: 8, Height: 6, Quantity: 1 },
        { ID: 1005, Name: 'Item6', Length: 3, Width: 3, Height: 2, Quantity: 2 }
    ];
    renderItems();
}

function generateContainers() {
    state.containers = [
        { ID: 1000, Name: 'Box1', Length: 15, Width: 13, Height: 9, AlgorithmPackingResults: [] },
        { ID: 1001, Name: 'Box2', Length: 23, Width: 9, Height: 4, AlgorithmPackingResults: [] },
        { ID: 1002, Name: 'Box3', Length: 16, Width: 16, Height: 6, AlgorithmPackingResults: [] },
        { ID: 1003, Name: 'Box4', Length: 10, Width: 8, Height: 5, AlgorithmPackingResults: [] },
        { ID: 1004, Name: 'Box5', Length: 40, Width: 28, Height: 20, AlgorithmPackingResults: [] },
        { ID: 1005, Name: 'Box6', Length: 29, Width: 19, Height: 4, AlgorithmPackingResults: [] },
        { ID: 1006, Name: 'Box7', Length: 18, Width: 13, Height: 1, AlgorithmPackingResults: [] },
        { ID: 1007, Name: 'Box8', Length: 6, Width: 6, Height: 6, AlgorithmPackingResults: [] },
        { ID: 1008, Name: 'Box9', Length: 8, Width: 5, Height: 5, AlgorithmPackingResults: [] },
        { ID: 1009, Name: 'Box10', Length: 18, Width: 13, Height: 8, AlgorithmPackingResults: [] },
        { ID: 1010, Name: 'Box11', Length: 17, Width: 16, Height: 15, AlgorithmPackingResults: [] },
        { ID: 1011, Name: 'Box12', Length: 32, Width: 10, Height: 9, AlgorithmPackingResults: [] },
        { ID: 1012, Name: 'Box13', Length: 60, Width: 60, Height: 60, AlgorithmPackingResults: [] }
    ];
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

function toNumber(value) {
    return value === '' ? 0 : Number(value);
}

async function packContainers() {
    const request = {
        Containers: state.containers.map(container => ({
            ID: container.ID,
            Length: toNumber(container.Length),
            Width: toNumber(container.Width),
            Height: toNumber(container.Height)
        })),
        ItemsToPack: state.itemsToPack.map(item => ({
            ID: item.ID,
            Dim1: toNumber(item.Length),
            Dim2: toNumber(item.Width),
            Dim3: toNumber(item.Height),
            Quantity: toNumber(item.Quantity)
        })),
        AlgorithmTypeIDs: state.algorithmTypeIDs.map(algorithm => algorithm.AlgorithmID)
    };

    const response = await fetch('/api/containerpacking', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json; charset=utf-8' },
        body: JSON.stringify(request)
    });

    if (!response.ok) {
        throw new Error(`Packing request failed with status ${response.status}`);
    }

    const packingResults = await response.json();
    packingResults.forEach(containerPackingResult => {
        const container = state.containers.find(current => current.ID == containerPackingResult.ContainerID);
        if (container) {
            container.AlgorithmPackingResults = containerPackingResult.AlgorithmPackingResults;
        }
    });

    renderContainers();
}

async function ensureDrawingInitialized() {
    if (drawingInitialized) {
        return;
    }

    await loadThree();

    const container = document.getElementById('drawing-container');

    scene = new THREE.Scene();
    camera = new THREE.PerspectiveCamera(50, 1, 0.1, 10000);
    camera.lookAt(scene.position);

    const light = new THREE.PointLight(0xffffff);
    light.position.set(0, 150, 100);
    scene.add(light);

    itemMaterial = new THREE.MeshNormalMaterial({ transparent: true, opacity: 0.6 });

    renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setClearColor(0xf0f0f0);
    renderer.setPixelRatio(window.devicePixelRatio);
    container.append(renderer.domElement);
    resizeDrawing();

    controls = new OrbitControls(camera, renderer.domElement);
    window.addEventListener('resize', resizeDrawing, false);

    drawingInitialized = true;
    animate();
}

function getDrawingSize() {
    const container = document.getElementById('drawing-container');

    return {
        width: container.clientWidth || Math.min(window.innerWidth * 0.9, 1140),
        height: container.clientHeight || Math.min(window.innerHeight * 0.7, 760)
    };
}

function resizeDrawing() {
    if (!renderer || !camera) {
        return;
    }

    const size = getDrawingSize();
    camera.aspect = size.width / size.height;
    camera.updateProjectionMatrix();
    renderer.setSize(size.width, size.height);
}

function animate() {
    requestAnimationFrame(animate);
    controls.update();
    renderer.render(scene, camera);
}

function clearPackingScene() {
    const selectedContainer = scene.getObjectByName('container');
    if (selectedContainer) {
        scene.remove(selectedContainer);
    }

    for (let i = 0; i < 1000; i++) {
        const selectedObject = scene.getObjectByName(`cube${i}`);
        if (selectedObject) {
            scene.remove(selectedObject);
        }
    }
}

async function showPackingView(containerIndex, resultIndex) {
    await ensureDrawingInitialized();

    const container = state.containers[containerIndex];
    const algorithmPackingResult = container.AlgorithmPackingResults[resultIndex];

    clearPackingScene();

    camera.position.set(container.Length, container.Length, container.Length);
    controls.update();

    state.itemsToRender = algorithmPackingResult.PackedItems ?? [];
    state.lastItemRenderedIndex = -1;
    state.containerOriginOffset.x = -1 * container.Length / 2;
    state.containerOriginOffset.y = -1 * container.Height / 2;
    state.containerOriginOffset.z = -1 * container.Width / 2;

    const geometry = new THREE.BoxGeometry(container.Length, container.Height, container.Width);
    const geo = new THREE.EdgesGeometry(geometry);
    const mat = new THREE.LineBasicMaterial({ color: 0x000000, linewidth: 2 });
    const wireframe = new THREE.LineSegments(geo, mat);
    wireframe.position.set(0, 0, 0);
    wireframe.name = 'container';
    scene.add(wireframe);

    updateRenderButtons();
    resizeDrawing();
}

function packItemInRender() {
    if (!drawingInitialized) {
        return;
    }

    const itemIndex = state.lastItemRenderedIndex + 1;
    const item = state.itemsToRender[itemIndex];
    if (!item) {
        return;
    }

    const itemOriginOffset = {
        x: item.PackDimX / 2,
        y: item.PackDimY / 2,
        z: item.PackDimZ / 2
    };

    const itemGeometry = new THREE.BoxGeometry(item.PackDimX, item.PackDimY, item.PackDimZ);
    const cube = new THREE.Mesh(itemGeometry, itemMaterial);
    cube.position.set(
        state.containerOriginOffset.x + itemOriginOffset.x + item.CoordX,
        state.containerOriginOffset.y + itemOriginOffset.y + item.CoordY,
        state.containerOriginOffset.z + itemOriginOffset.z + item.CoordZ);
    cube.name = `cube${itemIndex}`;
    scene.add(cube);

    state.lastItemRenderedIndex = itemIndex;
    updateRenderButtons();
}

function unpackItemInRender() {
    if (!drawingInitialized) {
        return;
    }

    const selectedObject = scene.getObjectByName(`cube${state.lastItemRenderedIndex}`);
    if (selectedObject) {
        scene.remove(selectedObject);
    }
    state.lastItemRenderedIndex -= 1;
    updateRenderButtons();
}

function updateRenderButtons() {
    unpackRenderItemButton.disabled = state.lastItemRenderedIndex < 0;
    packRenderItemButton.disabled = state.itemsToRender.length === state.lastItemRenderedIndex + 1;
}

function handleTableInput(event) {
    const input = event.target.closest('input[data-entity]');
    if (!input) {
        return;
    }

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
        showPackingView(Number(showPackingButton.dataset.containerIndex), Number(showPackingButton.dataset.resultIndex)).catch(error => {
            console.error(error);
            alert('Не удалось открыть визуализацию. Подробности смотрите в консоли браузера.');
        });
    }
}

function initializeUi() {
    document.getElementById('add-algorithm').addEventListener('click', addAlgorithmToUse);
    document.getElementById('generate-items').addEventListener('click', generateItemsToPack);
    document.getElementById('add-item').addEventListener('click', addNewItemToPack);
    document.getElementById('generate-containers').addEventListener('click', generateContainers);
    document.getElementById('add-container').addEventListener('click', addNewContainer);
    document.getElementById('pack-containers').addEventListener('click', () => {
        packContainers().catch(error => {
            console.error(error);
            alert('Не удалось выполнить упаковку. Подробности смотрите в консоли браузера.');
        });
    });
    packRenderItemButton.addEventListener('click', packItemInRender);
    unpackRenderItemButton.addEventListener('click', unpackItemInRender);
    document.getElementById('content').addEventListener('click', handleContentClick);
    document.getElementById('content').addEventListener('input', handleTableInput);
    document.getElementById('renderModal').addEventListener('shown.bs.modal', resizeDrawing);
}

document.addEventListener('DOMContentLoaded', () => {
    initializeUi();
    render();
});