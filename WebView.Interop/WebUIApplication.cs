﻿using System;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WebView.Interop
{
    [AllowForWeb]
    public sealed class WebUIApplication
    {
        private Application _app;
        private Windows.UI.Xaml.Controls.WebView _webView;
        private IActivatedEventArgs _launchArgs;

        // Occurs when the app is activated.
        public event EventHandler<Object> Activated;

        // Occurs when the app has begins running in the background (no UI is shown for the app).
        public event EventHandler<Object> EnteredBackground;

        // Occurs when the app is about to leave the background and before the app's UI is shown.
        public event EventHandler<Object> LeavingBackground;

        // Occurs when the app is navigating.
        public event EventHandler<Object> Navigated;

        // Occurs when the app is resuming.
        public event EventHandler<Object> Resuming;

        // Occurs when the app is suspending.
        public event EventHandler<Object> Suspending;

        // Occurs when the app creates a new window.
        public event EventHandler<Object> WindowCreated;

        public WebUIApplication(Application app)
        {
            _app = app;

            _app.EnteredBackground += App_EnteredBackground;
            _app.LeavingBackground += App_LeavingBackground;
            _app.Resuming += App_Resuming;
            _app.Suspending += App_Suspending;
            _app.UnhandledException += App_UnhandledException;
        }

        ~WebUIApplication()
        {
            _launchArgs = null;

            if (_webView != null)
            {
                _webView.NavigationStarting -= WebView_NavigationStarting;
                _webView.DOMContentLoaded -= WebView_DOMContentLoaded;
                _webView.Unloaded -= WebView_Unloaded;
                _webView = null;
            }

            if (_app != null)
            {
                _app.EnteredBackground -= App_EnteredBackground;
                _app.LeavingBackground -= App_LeavingBackground;
                _app.Resuming -= App_Resuming;
                _app.Suspending -= App_Suspending;
                _app.UnhandledException -= App_UnhandledException;
                _app = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="source"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        [DefaultOverload]
        public void Launch(Uri source, IActivatedEventArgs e)
        {
            _launchArgs = e;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is Windows.UI.Xaml.Controls.WebView))
            {
                if (_webView != null)
                {
                    _webView.NavigationStarting -= WebView_NavigationStarting;
                    _webView.DOMContentLoaded -= WebView_DOMContentLoaded;
                    _webView.Unloaded -= WebView_Unloaded;
                }

                _webView = new Windows.UI.Xaml.Controls.WebView();
                _webView.NavigationStarting += WebView_NavigationStarting;
                _webView.DOMContentLoaded += WebView_DOMContentLoaded;
                _webView.Unloaded += WebView_Unloaded;

                Window.Current.Content = _webView;
            }

            _webView.Navigate(source);

            if (!(e is IPrelaunchActivatedEventArgs) ||
                e is IPrelaunchActivatedEventArgs && (e as IPrelaunchActivatedEventArgs).PrelaunchActivated == false)
            {
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void Launch(Uri source, ContactPanelActivatedEventArgs e)
        {
            var webView = new Windows.UI.Xaml.Controls.WebView();
            _webView.NavigationStarting += WebView_NavigationStarting;
            _webView.DOMContentLoaded += WebView_DOMContentLoaded;
            _webView.Unloaded += WebView_Unloaded;

            Window.Current.Content = webView;
            webView.Navigate(source);

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Activation handlers
        /// </summary>
        /// <param name="e"></param>
        public void Activate(IActivatedEventArgs e) => EventDispatcher.Dispatch(() => Activated?.Invoke(this, e));

        public void BackgroundActivate(Windows.ApplicationModel.Activation.BackgroundActivatedEventArgs e)
        {
            EventDispatcher.Dispatch(() => Activated?.Invoke(this, new BackgroundActivatedEventArgs(_launchArgs, e)));
        }

        public void CachedFileUpdaterActivate(CachedFileUpdaterActivatedEventArgs e)
        {
            EventDispatcher.Dispatch(() => Activated?.Invoke(this, e));
        }

        public void FileActivate(FileActivatedEventArgs e)
        {
            EventDispatcher.Dispatch(() => Activated?.Invoke(this, e));
        }

        public void FileOpenPickerActivate(FileOpenPickerActivatedEventArgs e)
        {
            EventDispatcher.Dispatch(() => Activated?.Invoke(this, e));
        }

        public void FileSavePickerActivate(FileSavePickerActivatedEventArgs e)
        {
            EventDispatcher.Dispatch(() => Activated?.Invoke(this, e));
        }

        public void SearchActivate(SearchActivatedEventArgs e)
        {
            EventDispatcher.Dispatch(() => Activated?.Invoke(this, e));
        }

        public void ShareTargetActivate(ShareTargetActivatedEventArgs e)
        {
            EventDispatcher.Dispatch(() => Activated?.Invoke(this, e));
        }

        public void OnWindowCreated(WindowCreatedEventArgs e)
        {
            EventDispatcher.Dispatch(() => WindowCreated?.Invoke(this, e));
        }

        /// <summary>
        /// Enable or disable the operating system's ability to prelaunch your app.
        /// </summary>
        /// <param name="value"></param>
        public void EnablePrelaunch(bool value)
        {
            CoreApplication.EnablePrelaunch(value);
        }

        /// <summary>
        /// Restart the app.
        /// </summary>
        /// <param name="launchArguments"></param>
        /// <returns></returns>
        public IAsyncOperation<AppRestartFailureReason> RequestRestartAsync(string launchArguments)
        {
            return CoreApplication.RequestRestartAsync(launchArguments);
        }

        /// <summary>
        /// Restart the app in the context of a different user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="launchArguments"></param>
        /// <returns></returns>
        public IAsyncOperation<AppRestartFailureReason> RequestRestartForUserAsync(User user, String launchArguments)
        {
            return CoreApplication.RequestRestartForUserAsync(user, launchArguments);
        }

        #region Application event handlers
        private void App_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            EventDispatcher.Dispatch(() => EnteredBackground?.Invoke(this, e));
        }

        private void App_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            EventDispatcher.Dispatch(() => LeavingBackground?.Invoke(this, e));
        }

        private void App_Resuming(object sender, object e)
        {
            EventDispatcher.Dispatch(() => Resuming?.Invoke(this, e));
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            EventDispatcher.Dispatch(() => Suspending?.Invoke(this, e));
        }

        private async void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // Windows Runtime HRESULTs in the range over 0x80070000 are converted to JavaScript errors 
            // by taking the hexadecimal value of the low bits and converting it to a decimal. 
            // For example, the HRESULT 0x80070032 is converted to the decimal value 50, and the JavaScript error is SCRIPT50. 
            // The HRESULT 0x80074005 is converted to the decimal value 16389, and the JavaScript error is SCRIPT16389. 
            // https://docs.microsoft.com/en-us/scripting/javascript/reference/javascript-run-time-errors

            var hResult = e.Exception.HResult;
            var lowBits = hResult & 0xFF;
            var number = "SCRIPT" + int.Parse(Convert.ToString(lowBits), System.Globalization.NumberStyles.HexNumber);
            var description = e.Message;

            await _webView.InvokeScriptAsync("eval", new string[] { $"throw new Error('{number}', '{description}')" });

            e.Handled = true;
        }
        #endregion Application event handlers

        #region WebView event handlers
        private void WebView_NavigationStarting(Windows.UI.Xaml.Controls.WebView sender, WebViewNavigationStartingEventArgs e)
        {
            sender.AddWebAllowedObject(GetType().Name, this);
        }

        private void WebView_DOMContentLoaded(Windows.UI.Xaml.Controls.WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            Activate(_launchArgs);
        }

        private void WebView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is Windows.UI.Xaml.Controls.WebView webView && webView != null)
            {
                webView.NavigationStarting -= WebView_NavigationStarting;
                webView.DOMContentLoaded -= WebView_DOMContentLoaded;
                webView.Unloaded -= WebView_Unloaded;
            }

            GC.Collect();
        }
        #endregion WebView event handlers
    }
}
