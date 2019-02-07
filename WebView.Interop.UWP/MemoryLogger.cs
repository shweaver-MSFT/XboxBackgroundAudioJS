using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Diagnostics;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;

namespace WebView.Interop.UWP
{
    public static class MemoryLogger
    {
        // A variable to track whether you are currently running in the background.
        private static bool _isInBackgroundMode = false;

        // Is the logger started?
        private static bool _isLoggerRunning = false;

        // File logging
        private static bool _logToFile = false;
        private static StorageFile _logFile = null;
        private static Stream _fileStream = null;
        private static readonly object _fileLock = new object();
        private const string _fileName = "memoryLog.txt";

        static MemoryLogger() { }

        public static async void Start(bool logToFile = false)
        {
            if (_isLoggerRunning) return;

            _logToFile = logToFile;

            if (_logToFile)
            {
                // AppData\Local\Packages\a6e3c334-0e9e-4314-93db-3a2d4066d3ac_80c4904e66sn0\LocalState
                _logFile = await ApplicationData.Current.LocalFolder.CreateFileAsync($"[{DateTime.Now.ToString("hhmmss")}]{_fileName}");
                _fileStream = await _logFile.OpenStreamForWriteAsync();
            }

            // Subscribe to key lifecyle events to know when the app
            // transitions to and from foreground and background.
            // Leaving the background is an important transition
            // because the app may need to restore UI.
            Application.Current.EnteredBackground += App_EnteredBackground;
            Application.Current.LeavingBackground += App_LeavingBackground;

            // During the transition from foreground to background the
            // memory limit allowed for the application changes. The application
            // has a short time to respond by bringing its memory usage
            // under the new limit.
            MemoryManager.AppMemoryUsageLimitChanging += MemoryManager_AppMemoryUsageLimitChanging;

            // After an application is backgrounded it is expected to stay
            // under a memory target to maintain priority to keep running.
            // Subscribe to the event that informs the app of this change.
            MemoryManager.AppMemoryUsageIncreased += MemoryManager_AppMemoryUsageIncreased;

            InitMemoryLogLoopAsync();

            _isLoggerRunning = true;
            Log($"MemoryLogger started");
        }

        public static void Stop()
        {
            if (!_isLoggerRunning) return;

            Application.Current.EnteredBackground -= App_EnteredBackground;
            Application.Current.LeavingBackground -= App_LeavingBackground;
            MemoryManager.AppMemoryUsageLimitChanging -= MemoryManager_AppMemoryUsageLimitChanging;
            MemoryManager.AppMemoryUsageIncreased -= MemoryManager_AppMemoryUsageIncreased;

            _isLoggerRunning = false;
            Log($"MemoryLogger stopped");
        }

        private static async void InitMemoryLogLoopAsync()
        {
            while(true)
            {
                Log($"Current memory usage: {BytesToMegaByteString(MemoryManager.AppMemoryUsage)}");
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// The application entered the background.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Log("App entered background.");

            _isInBackgroundMode = true;

            // An application may wish to release views and view data
            // here since the UI is no longer visible.
            //
            // As a performance optimization, here we note instead that
            // the app has entered background mode with _isInBackgroundMode and
            // defer unloading views until AppMemoryUsageLimitChanging or
            // AppMemoryUsageIncreased is raised with an indication that
            // the application is under memory pressure.
        }

        /// <summary>
        /// The application is leaving the background.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void App_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            Log("App leaving background.");

            // Mark the transition out of the background state
            _isInBackgroundMode = false;

            // Restore view content if it was previously unloaded
            if (Window.Current.Content == null)
            {
                // TODO: Restore unloaded UI
            }
        }

        /// <summary>
        /// Handle system notifications that the app has increased its
        /// memory usage level compared to its current target.
        /// </summary>
        /// <remarks>
        /// The app may have increased its usage or the app may have moved
        /// to the background and the system lowered the target for the app
        /// In either case, if the application wants to maintain its priority
        /// to avoid being suspended before other apps, it may need to reduce
        /// its memory usage.
        ///
        /// This is not a replacement for handling AppMemoryUsageLimitChanging
        /// which is critical to ensure the app immediately gets below the new
        /// limit. However, once the app is allowed to continue running and
        /// policy is applied, some apps may wish to continue monitoring
        /// usage to ensure they remain below the limit.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MemoryManager_AppMemoryUsageIncreased(object sender, object e)
        {
            // Obtain the current usage level
            var level = MemoryManager.AppMemoryUsageLevel;

            Log($"Memory usage increased: {Enum.GetName(typeof(AppMemoryUsageLevel), level)}");

            // Check the usage level to determine whether reducing memory is necessary.
            // Memory usage may have been fine when initially entering the background but
            // the app may have increased its memory usage since then and will need to trim back.
            if (level == AppMemoryUsageLevel.OverLimit || level == AppMemoryUsageLevel.High)
            {
                ReduceMemoryUsage(MemoryManager.AppMemoryUsageLimit);
            }
        }

        /// <summary>
        /// Raised when the memory limit for the app is changing, such as when the app
        /// enters the background.
        /// </summary>
        /// <remarks>
        /// If the app is using more than the new limit, it must reduce memory within 2 seconds
        /// on some platforms in order to avoid being suspended or terminated.
        ///
        /// While some platforms will allow the application
        /// to continue running over the limit, reducing usage in the time
        /// allotted will enable the best experience across the broadest range of devices.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MemoryManager_AppMemoryUsageLimitChanging(object sender, AppMemoryUsageLimitChangingEventArgs e)
        {
            if (e.OldLimit == e.NewLimit && e.OldLimit == ulong.MaxValue) return;

            Log($"Memory usage limit changing: {BytesToMegaByteString(e.OldLimit)} -> {BytesToMegaByteString(e.NewLimit)}");

            // If app memory usage is over the limit, reduce usage within 2 seconds
            // so that the system does not suspend the app
            if (MemoryManager.AppMemoryUsage >= e.NewLimit)
            {
                ReduceMemoryUsage(e.NewLimit);
            }
        }

        /// <summary>
        /// Reduces application memory usage.
        /// </summary>
        /// <remarks>
        /// When the app enters the background, receives a memory limit changing
        /// event, or receives a memory usage increased event, it can
        /// can optionally unload cached data or even its view content in
        /// order to reduce memory usage and the chance of being suspended.
        ///
        /// This must be called from multiple event handlers because an application may already
        /// be in a high memory usage state when entering the background, or it
        /// may be in a low memory usage state with no need to unload resources yet
        /// and only enter a higher state later.
        /// </remarks>
        public static void ReduceMemoryUsage(ulong limit)
        {
            Log("Reducing memory usage...");

            // If the app has caches or other memory it can free, it should do so now.
            // << App can release memory here >>

            // Additionally, if the application is currently
            // in background mode and still has a view with content
            // then the view can be released to save memory and
            // can be recreated again later when leaving the background.
            if (_isInBackgroundMode && Window.Current.Content != null)
            {
                // Some apps may wish to use this helper to explicitly disconnect
                // child references.
                Windows.UI.Xaml.Media.VisualTreeHelper.DisconnectChildrenRecursive(Window.Current.Content);

                // Clear the view content. Note that views should rely on
                // events like Page.Unloaded to further release resources.
                // Release event handlers in views since references can
                // prevent objects from being collected.
                Window.Current.Content = null;
            }

            // Run the GC to collect released resources.
            GC.Collect();

            Log("Memory usage reduced.");
        }

        private static void Log(string message)
        {
            message = $"[{DateTime.Now.ToString("T")}] {message}\r\n";
            System.Diagnostics.Debug.WriteLine(message);

            if (_logToFile)
            {
                lock (_fileLock)
                {
                    WriteToFile(new UnicodeEncoding().GetBytes(message));
                }
            }
        }

        private static async void WriteToFile(byte[] messageBytes)
        {
            _fileStream.Seek(0, SeekOrigin.End);
            await _fileStream.WriteAsync(messageBytes, 0, messageBytes.Length);
        }

        private static string BytesToMegaByteString(double bytes)
        {
            var mb = bytes / 1048576d; // Math.Pow(1024, 2) = 1048576
            return mb.ToString("#.##") + "MB";
        }
    }
}
