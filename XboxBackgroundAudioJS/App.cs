using System;
using WebView.Interop.UWP;

namespace XboxBackgroundAudioJS
{
    sealed partial class App : HybridWebApplication
    {
        // Make sure to grant WebView access to these in the appxmanifest.
        // <uap:ApplicationContentUriRules>
        //   <uap:Rule Match="ms-appx-web:///index.html" Type="include" WindowsRuntimeAccess="all" />
        // </uap:ApplicationContentUriRules>
        private static readonly Uri _appSource = new Uri("ms-appx-web:///index.html");

        static void Main(string[] args)
        {
            Start(_ => new App());
        }

        public App() : base(_appSource)
        {
            MemoryLogger.Start(true);
        }
    }
}
