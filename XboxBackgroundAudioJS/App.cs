using System;
using WebView.Interop.UWP;
using Windows.System;

namespace XboxBackgroundAudioJS
{
    sealed partial class App : HybridWebApplication
    {
        // Make sure to grant WebView access to these in the appxmanifest.
        // <uap:ApplicationContentUriRules>
        //   <uap:Rule Match="ms-appx-web:///index.html"        Type="include" WindowsRuntimeAccess="all" />
        // </uap:ApplicationContentUriRules>
        private static readonly Uri _appSource = new Uri("ms-appx-web:///index.html");

        static void Main(string[] args)
        {
            Start(_ => new App());
        }

        // A variable to track whether you are currently running in the background.
        bool _isInBackgroundMode = false;

        public App() : base(_appSource)
        {
            EnteredBackground += App_EnteredBackground;
            LeavingBackground += App_LeavingBackground;

            MemoryManager.AppMemoryUsageIncreased += MemoryManager_AppMemoryUsageIncreased;
            MemoryManager.AppMemoryUsageLimitChanging += MemoryManager_AppMemoryUsageLimitChanging;
        }

        private void MemoryManager_AppMemoryUsageIncreased(object sender, object e)
        {
            if (_isInBackgroundMode && MemoryManager.AppMemoryUsage >= MemoryManager.AppMemoryUsageLimit)
            {
                // Memory usage has exceeded the limit from the background

            }
        }

        private void MemoryManager_AppMemoryUsageLimitChanging(object sender, AppMemoryUsageLimitChangingEventArgs e)
        {
            if (_isInBackgroundMode && MemoryManager.AppMemoryUsage >= e.NewLimit)
            {
                // Memory usage limit is about to drop below the current usage in the background.
                // Cleanup extra resources

            }
        }

        private void App_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            _isInBackgroundMode = false;
        }

        private void App_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            _isInBackgroundMode = true;
        }
    }
}
