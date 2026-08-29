let dotNetInstance;

export function initialize(dotNet) {
    dotNetInstance = dotNet;
}

export function sendToCallback(id, data) {
    if (!dotNetInstance)
        throw new Error("The Blazor callback registry has not been initialized.");

    const jsonPayload = JSON.stringify(data);

    return dotNetInstance.invokeMethodAsync('ReceiveJsCallback', id, jsonPayload);
}

export function dispose() {
    dotNetInstance = undefined;
}
