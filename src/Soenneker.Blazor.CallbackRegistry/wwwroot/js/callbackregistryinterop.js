export class CallbackRegistryInterop {

    initialize(dotNetInstance) {
        this.dotNetInstance = dotNetInstance;
    }

    sendToCallback(id, data) {
        const jsonPayload = JSON.stringify(data);

        this.dotNetInstance.invokeMethodAsync('ReceiveJsCallback', id, jsonPayload);
    }
}

window.CallbackRegistryInterop = new CallbackRegistryInterop();