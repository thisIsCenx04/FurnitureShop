(() => {
    if (!window.signalR) return;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .build();

    connection.on("notification:new", (data) => {
        console.log("notification:new", data);
    });

    connection.start()
        .then(() => console.log("SignalR connected"))
        .catch(err => console.error("SignalR connect error:", err));
})();
