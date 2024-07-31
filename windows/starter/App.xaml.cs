using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ReactNative;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace starter
{
    sealed partial class App : ReactApplication
    {
        public IReactContext ReactContext { get; set; }

        public ReactNativeHost SecondaryHost;

        public App()
        {
#if BUNDLE
            JavaScriptBundleFile = "index.windows";
            InstanceSettings.UseFastRefresh = false;
#else
            JavaScriptBundleFile = "index";
            InstanceSettings.UseFastRefresh = true;
#endif

#if DEBUG
            InstanceSettings.UseDirectDebugger = true;
            InstanceSettings.UseDeveloperSupport = true;
#else
            InstanceSettings.UseDirectDebugger = false;
            InstanceSettings.UseDeveloperSupport = false;
#endif

            Microsoft.ReactNative.Managed.AutolinkedNativeModules.RegisterAutolinkedNativeModulePackages(
                PackageProviders
            ); // Includes any autolinked modules

            PackageProviders.Add(new ReactPackageProvider());

            Host.InstanceSettings.InstanceCreated += OnReactInstanceCreated;

            ReactInstanceSettings ris = new ReactInstanceSettings()
            {
#if BUNDLE
                JavaScriptBundleFile = "index_SecondaryWindow.windows",
                UseFastRefresh = false,
#else
                JavaScriptBundleFile = "index_SecondaryWindow",
                UseFastRefresh = true,
#endif

#if DEBUG
                UseDirectDebugger = true,
                UseDeveloperSupport = true,
#else
                UseDirectDebugger = false,
                UseDeveloperSupport = false,
#endif
            };

            ris.Properties.Set(
                ReactPropertyBagHelper.GetName(
                    ReactPropertyBagHelper.GetNamespace("ReactNative.Dispatcher"),
                    "UIDispatcher"
                ),
                ris.UIDispatcher
            );

            SecondaryHost = new ReactNativeHost() { InstanceSettings = ris, };

            Microsoft.ReactNative.Managed.AutolinkedNativeModules.RegisterAutolinkedNativeModulePackages(
                SecondaryHost.PackageProviders
            );
            SecondaryHost.PackageProviders.Add(new ReactPackageProvider());

            InitializeComponent();
        }

        private void OnReactInstanceCreated(object sender, InstanceCreatedEventArgs args)
        {
            // This event is triggered when a React Native instance is created.
            // At this point, the ReactContext becomes valid.
            // However, it's better to emit events only after the JS code that can handle them is loaded.

            // If you need to perform any initialization based on ReactContext creation, do it here.
            // capsterNativeEmitter._reactContext = args.Context;
            this.ReactContext = args.Context;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            base.OnLaunched(e);
            var frame = (Frame)Window.Current.Content;
            frame.Navigate(typeof(MainPage), e.Arguments);
            OpenSecondaryWindow();
        }

        /// <summary>
        /// Invoked when the application is activated by some means other than normal launching.
        /// </summary>
        protected override void OnActivated(
            Windows.ApplicationModel.Activation.IActivatedEventArgs e
        )
        {
            var preActivationContent = Window.Current.Content;
            base.OnActivated(e);
            if (preActivationContent == null && Window.Current != null)
            {
                // Display the initial content
                var frame = (Frame)Window.Current.Content;
                frame.Navigate(typeof(MainPage), null);
            }
        }

        public async Task OpenSecondaryWindow()
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    Frame frame = new Frame();
                    frame.Navigate(typeof(SecondaryWindow), null);
                    Window.Current.Content = frame;
                    // You have to activate the window in order to show it later.
                    Window.Current.Activate();

                    newViewId = ApplicationView.GetForCurrentView().Id;
                }
            );
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
            await newView.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    SecondaryHost.ReloadInstance();
                }
            );
        }
    }
}
