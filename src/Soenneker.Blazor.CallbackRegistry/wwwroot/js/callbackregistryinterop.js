let dotNetInstance;

export function initialize(dotNet) {
    dotNetInstance = dotNet;
}

export function sendToCallback(id, data) {
    const jsonPayload = JSON.stringify(data);

    dotNetInstance.invokeMethodAsync('ReceiveJsCallback', id, jsonPayload);
}
