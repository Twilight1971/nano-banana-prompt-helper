using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace NanoBananaPromptHelper
{
    public class AppEntry
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        [STAThread]
        public static void Main()
        {
            var runtimeDir = EmbeddedFiles.PrepareRuntimeFiles();
            SetDllDirectory(runtimeDir);
            AppDomain.CurrentDomain.AssemblyResolve += EmbeddedFiles.ResolveAssembly;

            var app = new Application();
            var splash = new SplashWindow();
            splash.Show();
            app.Run(new MainWindow(splash));
        }
    }

    public class MainWindow : Window
    {
        private readonly WebView2 webView;
        private readonly Window splash;

        public MainWindow(Window splashWindow)
        {
            splash = splashWindow;
            Title = "Nano Banana Prompt Helper";
            Width = 1280;
            Height = 860;
            MinWidth = 960;
            MinHeight = 640;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Icon = BitmapFrame.Create(new Uri(EmbeddedFiles.GetLogoPath()));

            webView = new WebView2();
            Content = webView;

            Loaded += async (sender, args) =>
            {
                var userDataDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "NanoBananaPromptHelper",
                    "WebView2");

                var environment = await CoreWebView2Environment.CreateAsync(null, userDataDir);
                await webView.EnsureCoreWebView2Async(environment);

                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView.CoreWebView2.DownloadStarting += OnDownloadStarting;
                webView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    if (splash != null && splash.IsVisible)
                    {
                        splash.Close();
                    }
                };

                var htmlPath = EmbeddedFiles.GetHtmlPath();
                if (!File.Exists(htmlPath))
                {
                    MessageBox.Show(
                        "NanoBananaPromptHelper.html konnte nicht vorbereitet werden.",
                        "Datei fehlt",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Close();
                    return;
                }

                webView.Source = new Uri(htmlPath);
            };
        }

        private static void OnDownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            var downloads = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
            e.ResultFilePath = Path.Combine(downloads, Path.GetFileName(e.ResultFilePath));
            e.Handled = false;
        }
    }

    public class SplashWindow : Window
    {
        public SplashWindow()
        {
            Width = 520;
            Height = 360;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Topmost = true;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var border = new Border
            {
                CornerRadius = new CornerRadius(18),
                Background = new SolidColorBrush(Color.FromRgb(15, 15, 20)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(245, 197, 66)),
                BorderThickness = new Thickness(1),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 28,
                    ShadowDepth = 0,
                    Opacity = 0.65
                }
            };

            var grid = new Grid();
            var splash = new Image
            {
                Source = BitmapFrame.Create(new Uri(EmbeddedFiles.GetSplashPath())),
                Stretch = Stretch.UniformToFill
            };
            grid.Children.Add(splash);

            var title = new TextBlock
            {
                Text = "Nano Banana Prompt Helper",
                Foreground = new SolidColorBrush(Color.FromRgb(245, 197, 66)),
                FontSize = 22,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 24),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.8
                }
            };
            grid.Children.Add(title);

            border.Child = grid;
            Content = border;
        }
    }

    internal static class EmbeddedFiles
    {
        private const string ResourcePrefix = "NanoBananaPromptHelper.Resources.";

        private static readonly string RuntimeDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NanoBananaPromptHelper",
            "Runtime",
            Process.GetCurrentProcess().Id.ToString());

        public static string PrepareRuntimeFiles()
        {
            Directory.CreateDirectory(RuntimeDir);
            ExtractResource("NanoBananaPromptHelper.html");
            ExtractResource("Logo.png");
            ExtractResource("Splash.jpg");
            ExtractResource("WebView2Loader.dll");
            return RuntimeDir;
        }

        public static string GetHtmlPath()
        {
            return Path.Combine(RuntimeDir, "NanoBananaPromptHelper.html");
        }

        public static string GetLogoPath()
        {
            return Path.Combine(RuntimeDir, "Logo.png");
        }

        public static string GetSplashPath()
        {
            return Path.Combine(RuntimeDir, "Splash.jpg");
        }

        public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name + ".dll";
            if (name != "Microsoft.Web.WebView2.Core.dll" &&
                name != "Microsoft.Web.WebView2.Wpf.dll")
            {
                return null;
            }

            using (var stream = OpenResource(name))
            {
                if (stream == null)
                {
                    return null;
                }

                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return Assembly.Load(bytes);
            }
        }

        private static void ExtractResource(string fileName)
        {
            var targetPath = Path.Combine(RuntimeDir, fileName);
            using (var stream = OpenResource(fileName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("Embedded resource not found.", fileName);
                }

                using (var output = File.Create(targetPath))
                {
                    stream.CopyTo(output);
                }
            }
        }

        private static Stream OpenResource(string fileName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix + fileName);
        }
    }
}
