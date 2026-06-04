let packingViewerModulePromise;

function loadPackingViewer() {
    packingViewerModulePromise ??= import('./packing-viewer.js');
    return packingViewerModulePromise;
}

export async function createPackingViewer(
    renderModal,
    drawingContainer,
    unpackRenderItemButton,
    packRenderItemButton) {
    const elements = {
        renderModal,
        drawingContainer,
        unpackRenderItemButton,
        packRenderItemButton
    };

    const { createViewer } = await loadPackingViewer();
    const viewer = createViewer(elements);

    const handlePackClick = () => viewer.packItem();
    const handleUnpackClick = () => viewer.unpackItem();
    const handleShown = () => {
        viewer.render().catch(error => {
            console.error(error);
            alert('Не удалось открыть визуализацию. Подробности смотрите в консоли браузера.');
        });
    };
    const handleHidden = () => viewer.close();

    elements.packRenderItemButton.addEventListener('click', handlePackClick);
    elements.unpackRenderItemButton.addEventListener('click', handleUnpackClick);
    elements.renderModal.addEventListener('shown.bs.modal', handleShown);
    elements.renderModal.addEventListener('hidden.bs.modal', handleHidden);

    return {
        open(container) {
            viewer.open(container);
        },
        dispose() {
            elements.packRenderItemButton.removeEventListener('click', handlePackClick);
            elements.unpackRenderItemButton.removeEventListener('click', handleUnpackClick);
            elements.renderModal.removeEventListener('shown.bs.modal', handleShown);
            elements.renderModal.removeEventListener('hidden.bs.modal', handleHidden);
            viewer.dispose();
        }
    };
}
