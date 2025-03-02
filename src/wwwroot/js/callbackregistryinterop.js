export class CallbackRegistryInterop {

    initialize(dotNetInstance) {
        this.dotNetInstance = dotNetInstance;
    }

    sendToCallback(id, data) {
        const jsonPayload = JSON.stringify(data);

        if (this.dotNetInstance) {
            this.dotNetInstance.invokeMethodAsync('ReceiveJsCallback', id, jsonPayload);
        }
    }
}

window.CallbackRegistryInterop = new CallbackRegistryInterop();