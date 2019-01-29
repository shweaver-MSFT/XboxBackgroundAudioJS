using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WebView.Interop
{
    internal class WebViewPage : Page
    {
        private Uri _sourceUri = null;
        private Windows.UI.Xaml.Controls.WebView _webView = null;
        private WebUIApplication _webApp = null;

        public WebViewPage(WebUIApplication webApp, Uri sourceUri)
        {
            _webApp = webApp;
            _sourceUri = sourceUri;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        public void Load()
        {
            if (_webView == null)
            {
                _webView = CreateWebView();
                _webView.Source = _sourceUri;
                _webView.AddWebAllowedObject(_webApp.GetType().Name, _webApp);
                _webView.DOMContentLoaded += (s, e) => _webApp.Activate(_webApp.LaunchArgs);

                Content = _webView;
            }
        }

        public void Unload()
        {
            Content = null;
            UnwireWebViewDiagnostics(_webView);

            if (_webView != null)
            {
                _webView = null;
            }

            GC.Collect();
        }

        private static Windows.UI.Xaml.Controls.WebView CreateWebView()
        {
            var wv = new Windows.UI.Xaml.Controls.WebView(WebViewExecutionMode.SeparateProcess);
            wv.Settings.IsJavaScriptEnabled = true;
            wv.AddWebAllowedObject("mediaPlayer", PlaybackService.Instance);
            WireUpWebViewDiagnostics(wv);
            return wv;
        }

        private static void WireUpWebViewDiagnostics(Windows.UI.Xaml.Controls.WebView webView)
        {
            webView.NavigationStarting += OnWebViewNavigationStarting;
            webView.SeparateProcessLost += OnWebViewSeparateProcessLost;
            webView.ScriptNotify += OnWebViewScriptNotify;
        }

        private static void UnwireWebViewDiagnostics(Windows.UI.Xaml.Controls.WebView webView)
        {
            webView.NavigationStarting -= OnWebViewNavigationStarting;
            webView.SeparateProcessLost -= OnWebViewSeparateProcessLost;
            webView.ScriptNotify -= OnWebViewScriptNotify;
        }

        private static async void OnWebViewScriptNotify(object sender, NotifyEventArgs e)
        {
            if (sender is Windows.UI.Xaml.Controls.WebView wv)
            {
                //If you want to trigger an exteranl event without passing in a WinRT object, 
                // use window.external.notify("some string") which will call this method. The string will 
                // be accessible via e.Value. 
            }
        }

        private static void OnWebViewSeparateProcessLost(Windows.UI.Xaml.Controls.WebView sender, WebViewSeparateProcessLostEventArgs args)
        {
            UnwireWebViewDiagnostics(sender);
        }

        private static void OnWebViewNavigationStarting(Windows.UI.Xaml.Controls.WebView sender, WebViewNavigationStartingEventArgs args)
        {
            Debug.WriteLine(args.Uri?.ToString());
        }

        //private void ButtonUnload_Click(object sender, RoutedEventArgs e)
        //{
        //    Unload();
        //}
        //private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        //{
        //    Load();
        //}
    }
}
