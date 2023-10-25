using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using TrackBoss.Configuration;
using TrackBoss.Data.Validation;
using TrackBoss.Mvvm.Shared.Commands;
using TrackBoss.Mvvm.Validation.ViewModel;
using TrackBoss.ViewModel.WheelReport;
using System.Windows.Data;
using TrackBoss.Configuration.Preferences.Layout;
using TrackBoss.Data;
using TrackBoss.ViewModel.Cabeese;
using TrackBoss.ViewModel.Cars;
using TrackBoss.ViewModel.Cities;
using TrackBoss.ViewModel.RollingStocks;
using TrackBoss.ViewModel.Shared;
using TrackBoss.Windsor;
using System.Diagnostics;
using TrackBoss.Shared.Events;
using TrackBoss.Data.Enumerations;
using TrackBoss.Shared.Extensions;
using System.ComponentModel;
using System.Windows.Input;
using TrackBoss.ViewModel.Yards;
using GalaSoft.MvvmLight.Messaging;
using TrackBoss.Mvvm.Shared.Messages;
using TrackBoss.ViewModel.Locomotives;

namespace TrackBoss.ViewModel.WheelReport
{
    public enum ReportState
    {
        Overview,
        Cars,
        Cabooses,
        Locomotives,
        Cities,
        Yards,
        Trains
    }

    internal class WheelReportDesignerViewModel : GenericListBasedViewModel<WheelReportViewModel>, IInitializableViewModel
    {
        private Conductor conductor;
        private Validator<WheelReportViewModel> validator;
        private readonly DispatcherTimer validationTimer;
        private ObservableCollection<DataValidationResultViewModel<WheelReportViewModel>> validationResultsValue;

        private ReportState reportState = ReportState.Overview;

        private GridLength currentErrorWindowSplitterPosition;
        private GridLength currentListSplitterPosition;

        private ServicesDesignerLayoutConfiguration configuration;

        private ListCollectionView viewModelsViewValue;

        private TrackBossEntities trackBossConnection;
        private readonly AsyncCommand initializeCommandValue;

        private readonly RelayCommand overviewCommandValue;
        private readonly RelayCommand administrateCarsCommandValue;
        private readonly RelayCommand administrateCaboosesCommandValue;
        private readonly RelayCommand administrateLocomotivesCommandValue;
        private readonly RelayCommand administrateCitiesCommandValue;
        private readonly RelayCommand administrateYardsCommandValue;
        private readonly RelayCommand administrateTrainsCommandValue;



        private List<WheelReportDesignerViewModel> viewModelsHistory;

        private bool hasChanges;

        public WheelReportDesignerViewModel()
        {
            // Prepare the conductor.
            this.conductor = CastleWindsor.Default.Resolve<Conductor>();
            this.conductor.Preferences.PreferencesChanged += this.Preferences_PreferencesChanged;
            this.validationTimer = new DispatcherTimer(DispatcherPriority.Input);
            this.validationTimer.Interval = new TimeSpan(0, 0, Conductor.DefaultValidationInterval);
            this.validationTimer.Tick += this.ValidationTimer_Tick;

            // Initialize fields.
            this.viewModelsHistory = new List<WheelReportDesignerViewModel>();
            this.currentListSplitterPosition = new GridLength(Conductor.DefaultDesignerListWidth);
            this.currentErrorWindowSplitterPosition = new GridLength(Conductor.DefaultDesignerErrorWindowHeight);

            // Hookup command handlers

            // Use stored or default layout configuration, whichever is applicable.
            ServicesDesignerLayoutConfiguration defaultConfiguration = new ServicesDesignerLayoutConfiguration();
            this.configuration = this.conductor.Preferences.LoadLayoutConfiguration<ServicesDesignerLayoutConfiguration>(defaultConfiguration.Name);
            if (this.configuration == null)
            {
                this.configuration = defaultConfiguration;
                this.conductor.Preferences.SaveLayoutConfiguration(this.configuration);
            }
            this.currentListSplitterPosition = new GridLength(this.configuration.ListSplitterPosition);
            this.currentErrorWindowSplitterPosition = new GridLength(this.configuration.ErrorWindowSplitterPosition);

            this.initializeCommandValue = new AsyncCommand(this.InitializeCommandExecute, this.InitializeCommandCanExecute);

            // Hookup command handlers
            this.overviewCommandValue = new RelayCommand(this.OverviewCommandExecute, this.OverviewCommandCanExecute);
            this.administrateCarsCommandValue = new RelayCommand(this.AdministrateCarsCommandExecute, this.AdministrateCarsCommandCanExecute);
            this.administrateCaboosesCommandValue = new RelayCommand(this.AdministrateCaboosesCommandExecute, this.AdministrateCaboosesCommandCanExecute);
            this.administrateLocomotivesCommandValue = new RelayCommand(this.AdministrateLocomotivesCommandExecute, this.AdministrateLocomotivesCommandCanExecute);
            this.administrateCitiesCommandValue = new RelayCommand(this.AdministrateCitiesCommandExecute, this.AdministrateCitiesCommandCanExecute);
            this.administrateYardsCommandValue = new RelayCommand(this.AdministrateYardsCommandExecute, this.AdministrateYardsCommandCanExecute);
            this.administrateTrainsCommandValue = new RelayCommand(this.AdministrateTrainsCommandExecute, this.AdministrateTrainsCommandCanExecute);
        }


        private void Preferences_PreferencesChanged(object sender, EventArgs<string> e)
        {
            Debug.WriteLine(e.Value);
        }

        private async void ValidationTimer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("YardsDesignerViewModel - ValidationTimer_Tick");

            // Deactivate timer.
            this.validationTimer.Stop();

            // Perform validation.
            await this.performValidation();
        }

        private async Task performValidation()
        {
            Action performValidation = () =>
            {
                // Run validation.
                Debug.WriteLine("Performing validation.");
                List<DataValidationResult<WheelReportViewModel>> validationResults = new List<DataValidationResult<WheelReportViewModel>>();
                foreach (WheelReportViewModel wheelReportViewModel in this.ViewModels)
                {
                    //List<DataValidationResult<ServiceViewModel>> results = this.validator.Validate(serviceViewModel);
                    //validationResults.AddRange(results);
                }

                // Clear any existing results.
                if (this.validationResultsValue != null)
                    this.validationResultsValue.Clear();
                else
                    this.validationResultsValue = new ObservableCollection<DataValidationResultViewModel<WheelReportViewModel>>();

                // If there are results, then perform addition.
                if (validationResults.Count > 0)
                {
                    Debug.WriteLine("Adding validation results.");

                    // This MUST happen on the UI thread.
                    Action addResults = () =>
                    {
                        // Add current results.
                        foreach (DataValidationResult<WheelReportViewModel> result in validationResults)
                            this.validationResultsValue.Add(new DataValidationResultViewModel<WheelReportViewModel>(result));
                    };
                    App.Current.Dispatcher.Invoke(addResults);
                }

                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(WheelReportDesignerViewModel.ValidationResults));
            };
            await Task.Run(performValidation);
        }

        private async Task initializeAsync()
        {
            try
            {
                // Prepare status.
                this.StatusModel.SetStatus("Loading wheel report... please wait.");

                // Initialize context object.
                this.trackBossConnection = new TrackBossEntities(Conductor.ConnectionString);

                //// Disconnect any prior event handlers attached to the supporting
                //// lists.

                /*if (this.serviceAttributesValue != null)
                {
                    foreach (ServiceAttributeViewModel serviceAttribute in this.serviceAttributesValue)
                        serviceAttribute.PropertyChanged -= this.ServiceAttributeViewModel_PropertyChanged;
                }*/


                //// Initialize all supporting lists.
                //Debug.WriteLine("Initializing supporting lists");
                await this.initializeSupportingLists(this.trackBossConnection);

                //// Attach event handlers for supporting lists.
                //foreach (CommodityViewModel commodityViewModel in this.commoditiesValue)
                //    commodityViewModel.PropertyChanged += this.CommodityViewModel_PropertyChanged;

                // Initialize the list of servces.
                Debug.WriteLine("Initializing wheelreport");
                await this.initializeWheelReport(this.trackBossConnection);


                /*if (this.serviceAttributesValue != null)
                {
                    foreach (ServiceAttributeViewModel serviceAttribute in this.serviceAttributesValue)
                        serviceAttribute.PropertyChanged += this.ServiceAttributeViewModel_PropertyChanged;
                }*/

                //// Load user lists dependent on rolling stock. This MUST BE
                //// done after the cars are loaded.
                //Debug.WriteLine("Loading user rolling stock lengths");
                //this.loadUserRollingStockLengths();

                // Create list view collection.
                Debug.WriteLine(string.Format("ViewModels.Count: {0}", this.ViewModels.Count));
                this.viewModelsViewValue = new ListCollectionView(this.ViewModels)
                {
                    IsLiveSorting = true,
                };
                this.viewModelsViewValue.MoveCurrentToFirst();

                // Finally, load validation rule set.
                this.StatusModel.SetStatus("Validating wheelreport... please wait.");
                await this.initializeValidator();
                await this.performValidation();

                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(WheelReportDesignerViewModel.ViewModelsView));
                this.NotifyPropertyChanged(nameof(WheelReportDesignerViewModel.ValidationResults));
            }
            finally
            {
                // Always restore status to default.
                this.StatusModel.ClearStatus();
            }
        }

        private async Task initializeValidator()
        {
            Action initializeValidator = () =>
            {
                // Prepare validator.
                Debug.WriteLine("Initializing validator.");
                //this.validator = new Validator<ServiceViewModel>(FileUtilities.GetFullpath(SpecialFileName.ServiceValidationRuleSet));
            };
            await Task.Run(initializeValidator);
        }

        private async Task initializeSupportingLists(TrackBossEntities connection)
        {
            
        }

        private async Task initializeWheelReport(TrackBossEntities connection)
        {
            // Prepare new ViewModel.
            WheelReportViewModel newWheelReportViewModel = new WheelReportViewModel(this, connection);
            newWheelReportViewModel.ChangesMade += this.WheelReportViewModel_ChangesMade;
            this.ViewModels.Add(newWheelReportViewModel);
        }

        public void WheelReportViewModel_ChangesMade(object sender, EventArgs e)
        {
            // Validate parameters.
            Debug.WriteLine(string.Format("WheelReportDesignerViewModel - WheelReportViewModel_ChangesMade: {0}", sender.ToString()));
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            // Update whether or not this object has changes.
            this.hasChanges = true;

            // Restart validation timer.
            this.validationTimer.Stop();
            this.validationTimer.Start();

            // Invalidate commands.
            this.invalidateAllCommands();
        }



        private bool InitializeCommandCanExecute()
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private async Task InitializeCommandExecute()
        {
            // Make sure this is allowed.
            // The AsyncCommand object handles the call of CanExecute before
            // executing automatically.

            // Perform initialization.
            await this.initializeAsync();

            // If there are services and none are selected, then
            // select the first one.
            //if (this.SelectedViewModel == null && this.ViewModels.Count > 0)
            //    this.SelectedViewModel = (WheelReportViewModel)this.ViewModelsView.CurrentItem;
            SelectedViewModel = null;
        }

        protected override void invalidateAllCommands()
        {
            // Perform base invalidation of commands.
            base.invalidateAllCommands();

            // Perform invalidate on commands this object defines.
            this.initializeCommandValue.RaiseCanExecuteChanged();
            this.overviewCommandValue.InvalidateCanExecuteChanged();
            this.administrateCarsCommandValue.InvalidateCanExecuteChanged();
            this.administrateCaboosesCommandValue.InvalidateCanExecuteChanged();
            this.administrateLocomotivesCommandValue.InvalidateCanExecuteChanged();
            this.administrateCitiesCommandValue.InvalidateCanExecuteChanged();
            this.administrateYardsCommandValue.InvalidateCanExecuteChanged();
            this.administrateTrainsCommandValue.InvalidateCanExecuteChanged();
        }

        private bool OverviewCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy && reportState != ReportState.Overview;
        }

        private void OverviewCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.OverviewCommand.CanExecute(obj))
                return;

            ReportState = ReportState.Overview;
            invalidateAllCommands();
        }

        private bool AdministrateCarsCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy && reportState != ReportState.Cars;
        }

        private async void AdministrateCarsCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AdministrateCarsCommand.CanExecute(obj))
                return;

            await ViewModels[0].carsDesignerViewModel.InitializeCommand.ExecuteAsync();
            ReportState = ReportState.Cars;
            invalidateAllCommands();
        }

        private bool AdministrateCaboosesCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy && reportState != ReportState.Cabooses;
        }

        private async void AdministrateCaboosesCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AdministrateCaboosesCommand.CanExecute(obj))
                return;

            await ViewModels[0].caboosesDesignerViewModel.InitializeCommand.ExecuteAsync();
            ReportState = ReportState.Cabooses;
            invalidateAllCommands();
        }

        private bool AdministrateLocomotivesCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy && reportState != ReportState.Locomotives;
        }

        private async void AdministrateLocomotivesCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AdministrateLocomotivesCommand.CanExecute(obj))
                return;

            await ViewModels[0].locomotivesDesignerViewModel.InitializeCommand.ExecuteAsync();
            ReportState = ReportState.Locomotives;
            invalidateAllCommands();
        }

        private bool AdministrateCitiesCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy && reportState != ReportState.Cities;
        }

        private async void AdministrateCitiesCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AdministrateCitiesCommand.CanExecute(obj))
                return;

            await ViewModels[0].citiesDesignerViewModel.InitializeCommand.ExecuteAsync();
            ReportState = ReportState.Cities;
            invalidateAllCommands();
        }

        private bool AdministrateYardsCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy && reportState != ReportState.Yards;
        }

        private async void AdministrateYardsCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AdministrateYardsCommand.CanExecute(obj))
                return;

            await ViewModels[0].yardsDesignerViewModel.InitializeCommand.ExecuteAsync();
            ReportState = ReportState.Yards;
            invalidateAllCommands();
        }

        private bool AdministrateTrainsCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            //return !this.StatusModel.IsBusy && reportState != ReportState.Trains;
            return false;
        }

        private void AdministrateTrainsCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AdministrateTrainsCommand.CanExecute(obj))
                return;

            ReportState = ReportState.Trains;
            invalidateAllCommands();
        }





        public IAsyncCommand InitializeCommand
        {
            get { return this.initializeCommandValue; }
        }

        protected override Task closeCommandExecuteHelper()
        {
            throw new NotImplementedException();
        }

        protected override void deleteCommandExecuteHelper(object obj)
        {
            throw new NotImplementedException();
        }

        protected override void newCommandExecuteHelper(object obj)
        {
            throw new NotImplementedException();
        }

        protected override bool SaveCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy && this.hasChanges;
        }

        private async Task<bool> saveAsync()
        {
            try
            {
                // Prepare status.
                this.StatusModel.SetStatus("Saving changes... please wait.");

                // Perform save.
                Action save = () =>
                {
                    if (ReportState == ReportState.Overview)
                    {

                    }
                    else if (ReportState == ReportState.Cars)
                    {
                        this.ViewModels[0].carsDesignerViewModel.SaveCommand.Execute(this);
                    }
                    else if (ReportState == ReportState.Locomotives)
                    {

                    }
                    else if (ReportState == ReportState.Cities)
                    {

                    }
                    else if (ReportState == ReportState.Yards)
                    {

                    }
                    else if (ReportState == ReportState.Trains)
                    {

                    }
                };
                await Task.Run(save);
            }
            finally
            {
                // Always restore status to default.
                this.StatusModel.ClearStatus();
            }

            // Return save was completed.
            return true;
        }

        protected override async void saveCommandExecuteHelper(object obj)
        {
            //await this.saveAsync();
            if (ReportState == ReportState.Overview)
            {

            }
            else if (ReportState == ReportState.Cars)
            {
                await this.ViewModels[0].carsDesignerViewModel.saveAsyncWithConnection(this.trackBossConnection);
            }
            else if (ReportState == ReportState.Locomotives)
            {

            }
            else if (ReportState == ReportState.Cities)
            {

            }
            else if (ReportState == ReportState.Yards)
            {

            }
            else if (ReportState == ReportState.Trains)
            {

            }
            this.StatusModel.ClearStatus();

            // Perform cleanup.
            this.performViewModelCleanUp();

            // Destroy connection object.
            this.trackBossConnection.Dispose();
            this.trackBossConnection = null;

            // Reinitialize the data set.
            if (this.InitializeCommand.CanExecute(null))
                await this.initializeAsync();

            // Clear changes.
            this.hasChanges = false;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        protected override bool CancelCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy && this.hasChanges;
        }

        protected override void cancelCommandExecuteHelper(object obj)
        {
            // Make sure command is allowed.
            if (!this.CancelCommand.CanExecute(obj))
                return;

            // Display message.
            ShowMessageBoxMessage message = new
                ShowMessageBoxMessage()
            {
                Title = this.conductor.Name,
                Message = "WARNING: This will cancel all current changes.\n\nAre you sure you want to continue?",
                Button = MessageBoxButton.YesNo,
            };
            Messenger.Default.Send(message);

            // Check result of message.
            if (message.Result == MessageBoxResult.Yes)
            {
                // Cancel all history for this session. Note: the session
                // ID will be reused.
                this.viewModelsHistory.Clear();

                // Remove all current information and reload.
                this.validationResultsValue.Clear();
                this.performViewModelCleanUp();
                this.hasChanges = false;
                this.InitializeCommand.Execute(obj);

                // Invalidate commands.
                this.invalidateAllCommands();
            }
        }

        private void performViewModelCleanUp()
        {
            // Dispose of existing data set.
            foreach (CarViewModel carViewModel in this.ViewModels[0].carViewModels)
            {
                carViewModel.ChangesMade -= this.WheelReportViewModel_ChangesMade;
                carViewModel.Dispose();
            }
            foreach (CabooseViewModel cabooseViewModel in this.ViewModels[0].cabooseViewModels)
            {
                cabooseViewModel.ChangesMade -= this.WheelReportViewModel_ChangesMade;
                cabooseViewModel.Dispose();
            }
            foreach (LocomotiveViewModel locomotiveViewModel in this.ViewModels[0].locomotiveViewModels)
            {
                locomotiveViewModel.ChangesMade -= this.WheelReportViewModel_ChangesMade;
                locomotiveViewModel.Dispose();
            }
            foreach (CityViewModel cityViewModel in this.ViewModels[0].cityViewModels)
            {
                cityViewModel.ChangesMade -= this.WheelReportViewModel_ChangesMade;
                cityViewModel.Dispose();
            }
            foreach (YardViewModel yardViewModel in this.ViewModels[0].yardViewModels)
            {
                yardViewModel.ChangesMade -= this.WheelReportViewModel_ChangesMade;
                yardViewModel.Dispose();
            }
            this.ViewModels.Clear();
        }


        public ICollectionView ViewModelsView
        {
            get { return this.viewModelsViewValue; }
        }

        public ObservableCollection<DataValidationResultViewModel<WheelReportViewModel>> ValidationResults
        {
            get { return this.validationResultsValue; }
        }

        public ReportState ReportState
        {
            get
            {
                return this.reportState;
            }
            set
            {
                this.reportState = value;
                this.NotifyPropertyChanged();
            }
        }



        public ICommand OverviewCommand
        {
            get { return this.overviewCommandValue; }
        }
        public ICommand AdministrateCarsCommand
        {
            get { return this.administrateCarsCommandValue; }
        }
        public ICommand AdministrateCaboosesCommand
        {
            get { return this.administrateCaboosesCommandValue; }
        }
        public ICommand AdministrateLocomotivesCommand
        {
            get { return this.administrateLocomotivesCommandValue; }
        }
        public ICommand AdministrateCitiesCommand
        {
            get { return this.administrateCitiesCommandValue; }
        }
        public ICommand AdministrateYardsCommand
        {
            get { return this.administrateYardsCommandValue; }
        }
        public ICommand AdministrateTrainsCommand
        {
            get { return this.administrateTrainsCommandValue; }
        }
    }
}
