window.test = () => {

    var blah = {
        id: "blah",
        int: 1
    };

    window.CallbackRegistryInterop.sendToCallback("blah", blah);
}