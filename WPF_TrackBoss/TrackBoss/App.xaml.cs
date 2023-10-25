using Castle.MicroKernel.Registration;
using Syncfusion.SfSkinManager;
using System;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using TrackBoss.Configuration;
using TrackBoss.Configuration.Enumerations;
using TrackBoss.Configuration.IO;
using TrackBoss.Data;
using TrackBoss.Model.Shared;
using TrackBoss.Shared.Utilities.OS;
using TrackBoss.Utilities;
using TrackBoss.View;
using TrackBoss.ViewModel.Main;
using TrackBoss.ViewModel.Settings;
using TrackBoss.Windsor;

namespace TrackBoss
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public partial class App : Application
    {
        #region Fields

        private Conductor conductor;

        private FastClockModel fastClockModel;

        private TrackBossEntities trackBossEntities;

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates all application resources for colors and brushes.
        /// </summary>
        private void updateColors()
        {
            Color currentColor;
            SolidColorBrush currentSolidColorBrush = null;

            // Update colors. Update colors which are linked to brushes first.

            // Hover Color
            currentColor = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.HoverColor.Value);
            Application.Current.Resources["HoverColor"] = currentColor;
            currentSolidColorBrush = new SolidColorBrush(currentColor);
            currentSolidColorBrush.Freeze();
            Resources["HoverBrush"] = currentSolidColorBrush;

            // Backstage (Accent) Color
            currentColor = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.BackstageColor.Value);
            Application.Current.Resources["BackstageColor"] = currentColor;
            currentSolidColorBrush = new SolidColorBrush(currentColor);
            currentSolidColorBrush.Freeze();
            Resources["BackstageBrush"] = currentSolidColorBrush;
            
            // Backstage (Accent) Text Color
            currentColor = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.BackstageTextColor.Value);
            Application.Current.Resources["BackstageTextColor"] = currentColor;
            currentSolidColorBrush = new SolidColorBrush(currentColor);
            currentSolidColorBrush.Freeze();
            Resources["BackstageTextBrush"] = currentSolidColorBrush;

            // Dark Background Color
            currentColor = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.BackgroundDarkColor.Value);
            Application.Current.Resources["BackgroundDarkColor"] = currentColor;
            currentSolidColorBrush = new SolidColorBrush(currentColor);
            currentSolidColorBrush.Freeze();
            Resources["BackgroundDarkBrush"] = currentSolidColorBrush;

            // Light Background Color
            currentColor = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.BackgroundLightColor.Value);
            Application.Current.Resources["BackgroundLightColor"] = currentColor;
            currentSolidColorBrush = new SolidColorBrush(currentColor);
            currentSolidColorBrush.Freeze();
            Resources["BackgroundLightBrush"] = currentSolidColorBrush;

            // Update remaining colors.
            Application.Current.Resources["PendingTrainsBackgroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.PendingTrainBackgroundColor.Value);
            Application.Current.Resources["PendingTrainsForegroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.PendingTrainForegroundColor.Value);
            Application.Current.Resources["ScheduledTrainsBackgroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.ScheduledTrainBackgroundColor.Value);
            Application.Current.Resources["ScheduledTrainsForegroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.ScheduledTrainForegroundColor.Value);
            Application.Current.Resources["PassengerTrainsBackgroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.PassengerTrainBackgroundColor.Value);
            Application.Current.Resources["PassengerTrainsForegroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.PassengerTrainForegroundColor.Value);
            Application.Current.Resources["ExtraBoardTrainsBackgroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.ExtraBoardTrainBackgroundColor.Value);
            Application.Current.Resources["ExtraBoardTrainsForegroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.ExtraBoardTrainForegroundColor.Value);
            Application.Current.Resources["SwitcherBackgroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.SwitcherBackgroundColor.Value);
            Application.Current.Resources["SwitcherForegroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.SwitcherForegroundColor.Value);
            Application.Current.Resources["WorksOriginBackgroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.WorksOriginBackgroundColor.Value);
            Application.Current.Resources["WorksOriginForegroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.WorksOriginForegroundColor.Value);
            Application.Current.Resources["WorksStopBackgroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.WorksStopBackgroundColor.Value);
            Application.Current.Resources["WorksStopForegroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.WorksStopForegroundColor.Value);
            Application.Current.Resources["WorksDestinationBackgroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.WorksDestinationBackgroundColor.Value);
            Application.Current.Resources["WorksDestinationForegroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.WorksDestinationForegroundColor.Value);
            Application.Current.Resources["FastClockBackgroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.FastClockBackgroundColor.Value);
            Application.Current.Resources["FastClockForegroundColor"] = Shared.Utilities.OS.ColorHelper.IntToColor(this.conductor.Settings.Colors.FastClockForegroundColor.Value);
        }

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            // Get application GUID as defined in assembly.
            string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

            // Unique ID for global mutex - Global prefix means it is global to the machine.
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);

            try
            {
                using (MutexHelper mutex = new MutexHelper(mutexId))
                {
                    if (mutex.IsSingleInstance)
                    {
                        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt/QHRqVVhjVFpFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF5jS39Rd0xmXH1YcH1cQw==;Mgo+DSMBPh8sVXJ0S0J+XE9HflRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS31TdERhWHZbdXZTR2FdVA==;ORg4AjUWIQA/Gnt2VVhkQlFadVdJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxQdkdiW35XdXFXQGFeVEE=;MTExNTc1NkAzMjMwMmUzNDJlMzBrZzJkU2p1cEZqcUNCdHFlTExyc2NqQThQd3pWTzRvTU5zS2o4K0RsWWdJPQ==;MTExNTc1N0AzMjMwMmUzNDJlMzBMYUNNQU10WjN2ZG11Ny9nOFBXM016Tm1kVktoN3NmNkZJckhEUzJ1TDdnPQ==;NRAiBiAaIQQuGjN/V0Z+WE9EaFxKVmJLYVB3WmpQdldgdVRMZVVbQX9PIiBoS35RdUVhWH1feXFQRGZeUUNy;MTExNTc1OUAzMjMwMmUzNDJlMzBVMVhKd0pIdVIySjN5eVV2NWZMOGtLMm9wSU9sSis3L1VpR003N04weW5rPQ==;MTExNTc2MEAzMjMwMmUzNDJlMzBPRmFnRENhL3F5Q1l4RW9md2U5b1J5WlRBVVpKZC9sZFp3T1JNb3lqbE9jPQ==;Mgo+DSMBMAY9C3t2VVhkQlFadVdJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxQdkdiW35XdXFXQGJbUkA=;MTExNTc2MkAzMjMwMmUzNDJlMzBPVzkxSlQ4R2tINEhJQ0VvcW9qVkk5VFA5dXlhNUMwbEZDbEFVb2FjWHFvPQ==;MTExNTc2M0AzMjMwMmUzNDJlMzBQbUNOMEYzeXBFazlZZlV3S0hsU0wxNzFHdjlLSjJvQVk5cUFZRzFBdUlNPQ==;MTExNTc2NEAzMjMwMmUzNDJlMzBVMVhKd0pIdVIySjN5eVV2NWZMOGtLMm9wSU9sSis3L1VpR003N04weW5rPQ==\r\n");//("##SyncfusionLicense##");

                        SplashScreen splashScreen = new SplashScreen("/Resources/Images/splash-screen.png");
                        splashScreen.Show(false);

                        // Perform any base initialization.
                        base.OnStartup(e);

                        // Prepare conductor.
                        this.conductor = CastleWindsor.Default.Resolve<Conductor>();

                        // Prepare colors.
                        this.updateColors();

                        // Attach event handler.
                        this.conductor.Settings.Colors.ColorsChanged += this.Colors_ColorsChanged;

                        // Create fast clock and register as the singleton for the app.
                        this.fastClockModel = FastClockModel.Create(FileUtilities.GetFullpath(SpecialFileName.FastClockSnapshots));
                        CastleWindsor.Default.Register(Component.For<FastClockModel>().Instance(this.fastClockModel).IsDefault().Named(CastleWindsorHelper.DefaultFastClockInstanceName));

                        // Create data context and register as the singleton for the app.
                        this.trackBossEntities = new TrackBossEntities(Conductor.ConnectionString);
                        CastleWindsor.Default.Register(Component.For<TrackBossEntities>().Instance(this.trackBossEntities));

                        // Create initial required ViewModels.
                        using (MainViewModel mainViewModel = CastleWindsor.Default.Resolve<MainViewModel>())
                        {
                            // Prepare primary ViewModels.
                            BackstageWelcomeViewModel backstageWelcomeViewModel = CastleWindsor.Default.Resolve<BackstageWelcomeViewModel>();
                            BackstageConductViewModel backstageConductViewModel = CastleWindsor.Default.Resolve<BackstageConductViewModel>(
                                new
                                {
                                    historyManager = this.conductor.HistoryManager
                                });
                            BackstagePrintingViewModel backstagePrintingViewModel = CastleWindsor.Default.Resolve<BackstagePrintingViewModel>();
                            BackstageLicenseViewModel backstageLicenseViewModel = CastleWindsor.Default.Resolve<BackstageLicenseViewModel>();
                            BackstageHelpViewModel backstageHelpViewModel = CastleWindsor.Default.Resolve<BackstageHelpViewModel>();
                            ColorSettingsViewModel colorSettingsViewModel = CastleWindsor.Default.Resolve<ColorSettingsViewModel>();
                            AppearanceSettingsViewModel appearanceSettingsViewModel = CastleWindsor.Default.Resolve<AppearanceSettingsViewModel>(
                                new
                                {
                                    colorSettingsViewModel = colorSettingsViewModel,
                                });
                            LayoutSettingsViewModel layoutSettingsViewModel = CastleWindsor.Default.Resolve<LayoutSettingsViewModel>();
                            OperationsSettingsViewModel operationsSettingsViewModel = CastleWindsor.Default.Resolve<OperationsSettingsViewModel>();
                            GeneralSettingsViewModel generalSettingsViewModel = CastleWindsor.Default.Resolve<GeneralSettingsViewModel>();
                            BackstageSettingsViewModel backstageSettingViewModel = CastleWindsor.Default.Resolve<BackstageSettingsViewModel>(
                                new
                                {
                                    appearanceSettingsViewModel = appearanceSettingsViewModel,
                                    layoutSettingsViewModel = layoutSettingsViewModel,
                                    operationsSettingsViewModel = operationsSettingsViewModel,
                                    generalSettingsViewModel = generalSettingsViewModel,
                                });
                            HelpViewModel helpViewModel = CastleWindsor.Default.Resolve<HelpViewModel>();
                            StatusViewModel statusViewModel = CastleWindsor.Default.Resolve<StatusViewModel>();
                            
                            // Instantiate main view.
                            MainWindowView window = CastleWindsor.Default.Resolve<MainWindowView>(
                                new
                                {
                                    mainViewModel = mainViewModel,
                                    statusViewModel = statusViewModel,
                                    backstageWelcomeViewModel = backstageWelcomeViewModel,
                                    backstageConductViewModel = backstageConductViewModel,
                                    backstageLicenseViewModel = backstageLicenseViewModel,
                                    backstagePrintingViewModel = backstagePrintingViewModel,
                                    backstageHelpViewModel = backstageHelpViewModel,
                                    backstageSettingViewModel = backstageSettingViewModel,
                                    helpViewModel = helpViewModel,
                                });

                            // Close splash screen. A better way of doing this 
                            // would be to have the main window kill the
                            // splash screen.
                            splashScreen.Close(TimeSpan.FromSeconds(1));

                            // Set theme.
                            // TODO: Add code to set this based on user's preferences shortly.
                            SfSkinManager.SetVisualStyle(window, VisualStyles.Office2016White);

                            // Output skin colors for testing.
                            //Debug.WriteLine("MetroColor: " + SkinStorage.GetMetroBrush(window).ToString());
                            //Debug.WriteLine("MetroBackgroundColor: " + SkinStorage.GetMetroBackgroundBrush(window).ToString());
                            //Debug.WriteLine("MetroPanelBackgroundColor: " + SkinStorage.GetMetroPanelBackgroundBrush(window).ToString());
                            //Debug.WriteLine("MetroBorderColor: " + SkinStorage.GetMetroBorderBrush(window).ToString());
                            //Debug.WriteLine("MetroForegroundColor: " + SkinStorage.GetMetroForegroundBrush(window).ToString());
                            //Debug.WriteLine("MetroHoverColor: " + SkinStorage.GetMetroHoverBrush(window).ToString());
                            //Debug.WriteLine("MetroFocusedBorderColor: " + SkinStorage.GetMetroFocusedBorderBrush(window).ToString());
                            //Debug.WriteLine("MetroHighlightedForegroundColor: " + SkinStorage.GetMetroHighlightedForegroundBrush(window).ToString());

                            // Begin execution.
                            window.ShowDialog();
                        }
                    }
                    else
                    {
                        // Construct message.
                        string message = string.Format("Another instance of {0} is already running.", Conductor.ProductName);

                        // Display message.
                        MessageBox.Show(message, Conductor.ProductName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            finally
            {
                // Unhook event handlers, if applicable.
                if (this.conductor != null)
                    this.conductor.Settings.Colors.ColorsChanged -= this.Colors_ColorsChanged;

                // Write fast clock state.
                if (this.fastClockModel != null)
                {
                    this.fastClockModel.Stop();
                    FastClockModel.SaveFastClock(FileUtilities.GetFullpath(SpecialFileName.FastClockSnapshots), this.fastClockModel);
                }

                // Dispose of default IOC container and its references.
                CastleWindsor.Default.Dispose();

                // Shutdown application.
                this.Shutdown();
            }
        }

        private void Colors_ColorsChanged(object sender, EventArgs e)
        {
            this.updateColors();
        }

        public App() : base()
        {
            // Enable multi-core JIT compiler optimization to improve startup-performance.
            string programData = FolderUtilities.GetFolderPath(SpecialFolder.ProgramDataFolder);
            ProfileOptimization.SetProfileRoot(programData);
            ProfileOptimization.StartProfile("trackboss.profile");
        }
    }
}
