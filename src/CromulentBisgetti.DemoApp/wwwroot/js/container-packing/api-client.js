async function getResponseErrorMessage(response) {
    const fallbackMessage = `Не удалось выполнить упаковку. Сервер вернул статус ${response.status}.`;

    try {
        const errorBody = await response.json();
        if (Array.isArray(errorBody?.errors)) {
            return errorBody.errors.join(' ');
        }

        if (typeof errorBody?.title === 'string') {
            return errorBody.title;
        }
    } catch {
        // Keep the fallback when the server does not return JSON.
    }

    return fallbackMessage;
}

export async function packContainers(request) {
    const response = await fetch('/api/containerpacking', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json; charset=utf-8' },
        body: JSON.stringify(request)
    });

    if (!response.ok) {
        throw new Error(await getResponseErrorMessage(response));
    }

    return response.json();
}
