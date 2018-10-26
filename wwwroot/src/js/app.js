(function () {

    var App = function () {

        this.TitleBarColor = { a: 255, r: 32, g: 32, b: 32 };
        this.TitleBarTextColor = { a: 255, r: 230, g: 230, b: 230 };

        // Initialize the TitleBar
        if (window.Windows) {
            try {
                var currentView = Windows.UI.ViewManagement.ApplicationView.getForCurrentView();

                if (currentView !== null) {
                    var titleBar = currentView.titleBar;
                    titleBar.backgroundColor = this.TitleBarColor;
                    titleBar.buttonBackgroundColor = this.TitleBarColor;
                    titleBar.foregroundColor = this.TitleBarTextColor;

                    currentView.setDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.useCoreWindow);
                }
            }
            catch(e) {
                // Windows.UI.ViewManagement.ApplicationView.getForCurrentView() will fail on Xbox
            }
        }
    };

    var app = new App();
    document.app = app;

})();