﻿(function () {

    var logger = document.logger;
    var app = document.app;

    var ActivationKind = {
        launch: 0
    };

    var ActivationService = function () {

        // Handle app launch activation
        function onLaunched(args) {
            logger.Log("Launch activated");
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

            if (!WebUIApplication) {
                onWebActivated(args);
                return;
            }

            if (args && args.kind !== undefined) {

                switch (args.kind) {
                    case ActivationKind.launch: onLaunched(args); return;
                }
            }

            // TODO: Handle activation with invalid/missing args
            logger.Log("Unhandled activation");

        }.bind(this);
    };

    var activationService = new ActivationService();
    document.activation = activationService;

    // Register for activation
    if (WebUIApplication) {
        // Hybrid Web Application (WebView)
        // Register for activation on injected IWebUIApplication implementation
        WebUIApplication.onactivated = activationService.OnActivated;
    }
    else {
        // Running on web, not WebView
        activationService.OnActivated();
    }
})();