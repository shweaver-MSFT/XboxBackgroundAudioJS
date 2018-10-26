(function () {

    var logger = document.logger;
    var app = document.app;

    var ActivationService = function () {

        // Handle app launch activation
        function onLaunched(args) {
            logger.Log("Launch activated");

            var activation = Windows.ApplicationModel.Activation;

            if (args && (args.previousExecutionState === activation.ApplicationExecutionState.terminated || args.previousExecutionState === activation.ApplicationExecutionState.closedByUser)) {
                // TODO: Populate the UI with the previously saved application data
            }
            else {
                // TODO: Populate the UI with defaults
            }
        }

        // Handle a web activation
        function onWebActivated(args) {
            logger.Log("Web Activated");
        }

        // The most recent activation args
        this.LastActivationArgs;

        // Handles app activation
        this.OnActivated = function (args) {

            this.LastActivationArgs = args;

            if (!window.Windows) {
                onWebActivated(args);
                return;
            }

            if (args && args.kind !== undefined) {

                var activation = Windows.ApplicationModel.Activation;

                switch (args.kind) {
                    case activation.ActivationKind.launch: onLaunched(args); return;
                }
            }

            // TODO: Handle activation with invalid/missing args
            logger.Log("Unhandled activation: Missing args - " + args);

        }.bind(this);
    };

    var activationService = new ActivationService();
    document.activation = activationService;

    // Register for activation
    if (window.Windows && window.Windows.UI && window.Windows.UI.WebUI) {
        try {
            // Native JavaScript or Progressive Web Application (WWAHost)
            // Registering for activation will throw an exception outside of WWAHost
            Windows.UI.WebUI.WebUIApplication.onactivated = activationService.OnActivated;
        }
        catch (e) {
            // Hybrid Web Application (WebView)
            // Register for activation on injected IWebUIApplication implementation
            WebUIApplication.onactivated = activationService.OnActivated;
        }
    }
    else {
        // Running on web, not UWP
        activationService.OnActivated();
    }
})();