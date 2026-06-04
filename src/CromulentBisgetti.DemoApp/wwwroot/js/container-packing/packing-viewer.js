import * as THREE from '../../lib/three/build/three.module.js';
import { OrbitControls } from '../../lib/three/examples/jsm/controls/OrbitControls.js';

export function createViewer(elements) {
    let drawingInitialized = false;
    let scene;
    let camera;
    let renderer;
    let controls;
    let itemMaterial;
    let renderedObjectsGroup;
    let animationLoopRunning = false;

    const viewerState = {
        itemsToRender: [],
        lastItemRenderedIndex: -1,
        containerOriginOffset: { x: 0, y: 0, z: 0 },
        selectedContainer: null
    };

    function ensureDrawingInitialized() {
        if (drawingInitialized) {
            return;
        }

        scene = new THREE.Scene();
        camera = new THREE.PerspectiveCamera(50, 1, 0.1, 10000);

        const light = new THREE.PointLight(0xffffff);
        light.position.set(0, 150, 100);
        scene.add(light);

        itemMaterial = new THREE.MeshNormalMaterial({ transparent: true, opacity: 0.6 });
        renderedObjectsGroup = new THREE.Group();
        scene.add(renderedObjectsGroup);

        renderer = new THREE.WebGLRenderer({ antialias: true });
        renderer.setClearColor(0xf0f0f0);
        renderer.setPixelRatio(window.devicePixelRatio);
        elements.drawingContainer.append(renderer.domElement);
        resizeDrawing();

        controls = new OrbitControls(camera, renderer.domElement);
        window.addEventListener('resize', resizeDrawing, false);

        drawingInitialized = true;
    }

    function getDrawingSize() {
        return {
            width: elements.drawingContainer.clientWidth || Math.min(window.innerWidth * 0.9, 1140),
            height: elements.drawingContainer.clientHeight || Math.min(window.innerHeight * 0.7, 760)
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

    function renderFrame() {
        controls.update();
        renderer.render(scene, camera);
    }

    function startAnimationLoop() {
        if (!renderer || animationLoopRunning) {
            return;
        }

        renderer.setAnimationLoop(renderFrame);
        animationLoopRunning = true;
    }

    function stopAnimationLoop() {
        if (!renderer || !animationLoopRunning) {
            return;
        }

        renderer.setAnimationLoop(null);
        animationLoopRunning = false;
    }

    function disposeMaterial(material) {
        if (Array.isArray(material)) {
            material.forEach(disposeMaterial);
            return;
        }

        if (material && material !== itemMaterial) {
            material.dispose();
        }
    }

    function disposeRenderedObject(object) {
        object.traverse(child => {
            if (child.geometry) {
                child.geometry.dispose();
            }

            disposeMaterial(child.material);
        });
    }

    function clearPackingScene() {
        if (!renderedObjectsGroup) {
            return;
        }

        renderedObjectsGroup.children.forEach(disposeRenderedObject);
        renderedObjectsGroup.clear();
    }

    function createContainerFrame(container) {
        const boxGeometry = new THREE.BoxGeometry(container.length, container.height, container.width);
        const edgesGeometry = new THREE.EdgesGeometry(boxGeometry);
        boxGeometry.dispose();

        const material = new THREE.LineBasicMaterial({ color: 0x000000, linewidth: 2 });
        const wireframe = new THREE.LineSegments(edgesGeometry, material);

        return wireframe;
    }

    function showPackingView(container) {
        ensureDrawingInitialized();

        const packingResult = container.packingResult;

        clearPackingScene();

        camera.position.set(container.length, container.length, container.length);
        controls.update();

        viewerState.itemsToRender = packingResult.packedItems ?? [];
        viewerState.lastItemRenderedIndex = -1;
        viewerState.containerOriginOffset.x = -1 * container.length / 2;
        viewerState.containerOriginOffset.y = -1 * container.height / 2;
        viewerState.containerOriginOffset.z = -1 * container.width / 2;

        renderedObjectsGroup.add(createContainerFrame(container));

        updateRenderButtons();
        resizeDrawing();
    }

    function open(container) {
        viewerState.selectedContainer = container;
        elements.drawingContainer.style.visibility = 'hidden';
        elements.packRenderItemButton.disabled = true;
        elements.unpackRenderItemButton.disabled = true;

        window.bootstrap.Modal.getOrCreateInstance(elements.renderModal).show();
    }

    async function render() {
        const container = viewerState.selectedContainer;
        if (!container) {
            return;
        }

        showPackingView(container);

        await new Promise(resolve => requestAnimationFrame(resolve));
        if (!drawingInitialized || viewerState.selectedContainer !== container) {
            return;
        }

        resizeDrawing();
        startAnimationLoop();
        renderFrame();
        elements.drawingContainer.style.visibility = 'visible';
    }

    function close() {
        stopAnimationLoop();
        clearPackingScene();
        elements.drawingContainer.style.visibility = 'hidden';

        if (controls) {
            controls.dispose();
        }

        if (itemMaterial) {
            itemMaterial.dispose();
        }

        if (renderer) {
            renderer.dispose();
            renderer.domElement.remove();
        }

        window.removeEventListener('resize', resizeDrawing, false);
        drawingInitialized = false;
        scene = undefined;
        camera = undefined;
        renderer = undefined;
        controls = undefined;
        itemMaterial = undefined;
        renderedObjectsGroup = undefined;
        viewerState.itemsToRender = [];
        viewerState.lastItemRenderedIndex = -1;
        viewerState.selectedContainer = null;
    }

    function packItem() {
        if (!drawingInitialized) {
            return;
        }

        const itemIndex = viewerState.lastItemRenderedIndex + 1;
        const item = viewerState.itemsToRender[itemIndex];
        if (!item) {
            return;
        }

        const itemOriginOffset = {
            x: item.packDimX / 2,
            y: item.packDimY / 2,
            z: item.packDimZ / 2
        };

        const itemGeometry = new THREE.BoxGeometry(item.packDimX, item.packDimY, item.packDimZ);
        const cube = new THREE.Mesh(itemGeometry, itemMaterial);
        cube.position.set(
            viewerState.containerOriginOffset.x + itemOriginOffset.x + item.coordX,
            viewerState.containerOriginOffset.y + itemOriginOffset.y + item.coordY,
            viewerState.containerOriginOffset.z + itemOriginOffset.z + item.coordZ);
        renderedObjectsGroup.add(cube);

        viewerState.lastItemRenderedIndex = itemIndex;
        updateRenderButtons();
    }

    function unpackItem() {
        if (!drawingInitialized || viewerState.lastItemRenderedIndex < 0) {
            return;
        }

        const selectedObject = renderedObjectsGroup.children[viewerState.lastItemRenderedIndex + 1];
        if (selectedObject) {
            disposeRenderedObject(selectedObject);
            renderedObjectsGroup.remove(selectedObject);
        }
        viewerState.lastItemRenderedIndex -= 1;
        updateRenderButtons();
    }

    function updateRenderButtons() {
        elements.unpackRenderItemButton.disabled = viewerState.lastItemRenderedIndex < 0;
        elements.packRenderItemButton.disabled = viewerState.itemsToRender.length === viewerState.lastItemRenderedIndex + 1;
    }

    function dispose() {
        close();
        window.bootstrap.Modal.getInstance(elements.renderModal)?.dispose();
    }

    return {
        open,
        render,
        close,
        packItem,
        unpackItem,
        dispose
    };
}
