using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WebView.Interop
{
    internal class WebViewPage : Page
    {
        public WebViewPage(Uri uri)
        {
            //InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Load();
        }

        public void Unload()
        {
            //Grid.Children.Remove(wv1);
            //UnwireWebViewDiagnostics(wv1);

            //if (wv1 != null)
            //{
            //    wv1 = null;
            //}

            //GC.Collect();
        }

        public void Load()
        {
            //if (wv1 == null)
            //{
            //    wv1 = CreateWebView();
            //    wv1.Source = MainHTMLPageUri;
            //}
        }

        private Windows.UI.Xaml.Controls.WebView CreateWebView()
        {
            var wv = new Windows.UI.Xaml.Controls.WebView(WebViewExecutionMode.SeparateProcess);
            wv.Settings.IsJavaScriptEnabled = true;
            wv.AddWebAllowedObject("mediaPlayer", PlaybackService.Instance);
            WireUpWebViewDiagnostics(wv);
            Grid.SetRow(wv, 0);
            //this.Grid.Children.Add(wv);
            return wv;
        }

        private void WireUpWebViewDiagnostics(Windows.UI.Xaml.Controls.WebView webView)
        {
            webView.NavigationStarting += OnWebViewNavigationStarting;
            webView.SeparateProcessLost += OnWebViewSeparateProcessLost;
            webView.ScriptNotify += OnWebViewScriptNotify;
        }

        private void UnwireWebViewDiagnostics(Windows.UI.Xaml.Controls.WebView webView)
        {
            webView.NavigationStarting -= OnWebViewNavigationStarting;
            webView.SeparateProcessLost -= OnWebViewSeparateProcessLost;
            webView.ScriptNotify -= OnWebViewScriptNotify;
        }

        private async void OnWebViewScriptNotify(object sender, NotifyEventArgs e)
        {
            if (sender is Windows.UI.Xaml.Controls.WebView wv)
            {
                //If you want to trigger an exteranl event without passing in a WinRT object, 
                // use window.external.notify("some string") which will call this method. The string will 
                // be accessible via e.Value. 
            }
        }

        private void OnWebViewSeparateProcessLost(Windows.UI.Xaml.Controls.WebView sender, WebViewSeparateProcessLostEventArgs args)
        {
            UnwireWebViewDiagnostics(sender);
        }



        private void OnWebViewNavigationStarting(Windows.UI.Xaml.Controls.WebView sender, WebViewNavigationStartingEventArgs args)
        {
            Debug.WriteLine(args.Uri?.ToString());
        }

        private void ButtonUnload_Click(object sender, RoutedEventArgs e)
        {
            Unload();
        }
        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }
    }
}
