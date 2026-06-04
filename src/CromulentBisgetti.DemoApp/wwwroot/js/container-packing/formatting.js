export const fieldLabels = {
    Length: 'длина',
    Width: 'ширина',
    Height: 'высота',
    Quantity: 'количество'
};

export function escapeHtml(value) {
    return String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}
