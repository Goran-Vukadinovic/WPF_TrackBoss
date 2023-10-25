using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using TrackBoss.Configuration;
using TrackBoss.Configuration.Enumerations;
using TrackBoss.Configuration.IO;
using TrackBoss.Configuration.Preferences.Layout;
using TrackBoss.Data;
using TrackBoss.Data.Enumerations;
using TrackBoss.Data.Validation;
using TrackBoss.Mvvm.Shared.Commands;
using TrackBoss.Mvvm.Shared.Messages;
using TrackBoss.Mvvm.Validation.ViewModel;
using TrackBoss.Shared.Enumerations;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.Cars;
using TrackBoss.ViewModel.Cities;
using TrackBoss.ViewModel.Shared;
using TrackBoss.ViewModel.Yards;
using TrackBoss.Windsor;

namespace TrackBoss.ViewModel.Crew
{
    internal class CrewDesignerViewModel : GenericListBasedViewModel<CrewViewModel>, IInitializableViewModel
    {
        #region Fields
        private Conductor conductor;
        private Validator<CrewViewModel> validator;
        private readonly DispatcherTimer validationTimer;
        private ObservableCollection<DataValidationResultViewModel<CrewViewModel>> validationResultsValue;

        private GridLength currentErrorWindowSplitterPosition;
        private GridLength currentListSplitterPosition;

        private readonly RelayCommand<CrewSortType> sortCommandValue;
        private readonly RelayCommand reverseSortCommandValue;
        private readonly RelayCommand saveAndAddNewCommandValue;

        private CrewDesignerLayoutConfiguration configuration;

        private ListCollectionView viewModelsViewValue;

        private TrackBossEntities trackBossConnection;
        private readonly AsyncCommand initializeCommandValue;

        private CrewSortType selectedSort;
        private ListSortDirection currentSortDirection;

        private List<CrewViewModel> viewModelsHistory;

        private bool hasChanges;

        #endregion

        #region Constructor(s)
        public CrewDesignerViewModel()
        {
            // Prepare the conductor.
            this.conductor = CastleWindsor.Default.Resolve<Conductor>();
            this.conductor.Preferences.PreferencesChanged += this.Preferences_PreferencesChanged;
            this.validationTimer = new DispatcherTimer(DispatcherPriority.Input);
            this.validationTimer.Interval = new TimeSpan(0, 0, Conductor.DefaultValidationInterval);
            this.validationTimer.Tick += this.ValidationTimer_Tick;

            // Initialize fields.
            this.viewModelsHistory = new List<CrewViewModel>();
            this.currentListSplitterPosition = new GridLength(Conductor.DefaultDesignerListWidth);
            this.currentErrorWindowSplitterPosition = new GridLength(Conductor.DefaultDesignerErrorWindowHeight);
            this.selectedSort = CrewSortType.FullName;
            this.currentSortDirection = ListSortDirection.Ascending;

            // Hookup command handlers
            this.sortCommandValue = new RelayCommand<CrewSortType>(this.SortCommandExecute, this.SortCommandCanExecute);
            this.reverseSortCommandValue = new RelayCommand(this.ReverseSortCommandExecute, this.ReverseSortCommandCanExecute);
            this.saveAndAddNewCommandValue = new RelayCommand(this.SaveAndAddNewCommandExecute, this.SaveAndAddNewCommandCanExecute);


            // Use stored or default layout configuration, whichever is applicable.
            CrewDesignerLayoutConfiguration defaultConfiguration = new CrewDesignerLayoutConfiguration();
            this.configuration = this.conductor.Preferences.LoadLayoutConfiguration<CrewDesignerLayoutConfiguration>(defaultConfiguration.Name);
            if (this.configuration == null)
            {
                this.configuration = defaultConfiguration;
                this.conductor.Preferences.SaveLayoutConfiguration(this.configuration);
            }
            this.currentListSplitterPosition = new GridLength(this.configuration.ListSplitterPosition);
            this.currentErrorWindowSplitterPosition = new GridLength(this.configuration.ErrorWindowSplitterPosition);

            this.selectedSort = this.configuration.SortType;
            this.currentSortDirection = this.configuration.SortDirection;

            this.initializeCommandValue = new AsyncCommand(this.InitializeCommandExecute, this.InitializeCommandCanExecute);
        }
        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            // If disposing perform clean-up.
            if (disposing)
            {
                // Clean-up ViewModels.
                this.performViewModelCleanUp();

                // Unhook events.
                this.conductor.Preferences.PreferencesChanged -= this.Preferences_PreferencesChanged;

                // Unhook message handlers.
                Messenger.Default.Unregister(this);
            }

            // Call base dispose processes last.
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods
        private async Task performValidation()
        {
            Action performValidation = () =>
            {
                // Run validation.
                Debug.WriteLine("Performing validation.");
                List<DataValidationResult<CrewViewModel>> validationResults = new List<DataValidationResult<CrewViewModel>>();
                foreach (CrewViewModel crewViewModel in this.ViewModels)
                {
                    //List<DataValidationResult<CrewViewModel>> results = this.validator.Validate(crewViewModel);
                    //validationResults.AddRange(results);
                }

                // Clear any existing results.
                if (this.validationResultsValue != null)
                    this.validationResultsValue.Clear();
                else
                    this.validationResultsValue = new ObservableCollection<DataValidationResultViewModel<CrewViewModel>>();

                // If there are results, then perform addition.
                if (validationResults.Count > 0)
                {
                    Debug.WriteLine("Adding validation results.");

                    // This MUST happen on the UI thread.
                    Action addResults = () =>
                    {
                        // Add current results.
                        foreach (DataValidationResult<CrewViewModel> result in validationResults)
                            this.validationResultsValue.Add(new DataValidationResultViewModel<CrewViewModel>(result));
                    };
                    App.Current.Dispatcher.Invoke(addResults);
                }

                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(CrewDesignerViewModel.ValidationResults));
            };
            await Task.Run(performValidation);
        }

        private async Task initializeAsync()
        {
            try
            {
                // Prepare status.
                this.StatusModel.SetStatus("Loading crews... please wait.");

                // Initialize context object.
                this.trackBossConnection = new TrackBossEntities(Conductor.ConnectionString);

                //// Disconnect any prior event handlers attached to the supporting
                //// lists.
                //Debug.WriteLine("Detaching event handlers from supporting lists.");
                //if (this.commoditiesValue != null)
                //{
                //    foreach (CommodityViewModel commodityViewModel in this.commoditiesValue)
                //        commodityViewModel.PropertyChanged -= this.CommodityViewModel_PropertyChanged;
                //}

                //// Initialize all supporting lists.
                //Debug.WriteLine("Initializing supporting lists");
                await this.initializeSupportingLists(this.trackBossConnection);

                //// Attach event handlers for supporting lists.
                //foreach (CommodityViewModel commodityViewModel in this.commoditiesValue)
                //    commodityViewModel.PropertyChanged += this.CommodityViewModel_PropertyChanged;

                // Initialize the list of yards.
                Debug.WriteLine("Initializing crews");
                await this.initializeCrews(this.trackBossConnection);

                //// Load user lists dependent on rolling stock. This MUST BE
                //// done after the cars are loaded.
                //Debug.WriteLine("Loading user rolling stock lengths");
                //this.loadUserRollingStockLengths();

                // Create list view collection.
                Debug.WriteLine(string.Format("ViewModels.Count: {0}", this.ViewModels.Count));
                this.viewModelsViewValue = new ListCollectionView(this.ViewModels)
                {
                    IsLiveSorting = true,
                    Filter = this.ViewModelsViewFilter,
                    //    CustomSort = new CityViewModelComparer(),
                };
                this.viewModelsViewValue.MoveCurrentToFirst();

                // Finally, load validation rule set.
                this.StatusModel.SetStatus("Validating yards... please wait.");
                await this.initializeValidator();
                await this.performValidation();

                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(CrewDesignerViewModel.ViewModelsView));
                this.NotifyPropertyChanged(nameof(CrewDesignerViewModel.ValidationResults));
            }
            finally
            {
                // Always restore status to default.
                this.StatusModel.ClearStatus();
            }
        }

        private bool ViewModelsViewFilter(object obj)
        {
            return true;
        }

        private async Task initializeCrews(TrackBossEntities connection)
        {
            // Fetch list of crews.
            List<Crewmember> crews = null;
            await Task.Run(() => crews = connection.Crewmembers.ToList());

            this.ViewModels.Clear();
            // Add yards to list.
            if (crews != null)
            {
                foreach (Crewmember crew in crews)
                {
                    // Prepare new ViewModel.
                    CrewViewModel newCrewViewModel = new CrewViewModel(this, crew);
                    newCrewViewModel.ChangesMade += this.CrewViewModel_ChangesMade;
                    this.ViewModels.Add(newCrewViewModel);
                }
            }
        }

        private async Task initializeValidator()
        {
            Action initializeValidator = () =>
            {
                // Prepare validator.
                Debug.WriteLine("Initializing validator.");
                //this.validator = new Validator<CrewViewModel>(FileUtilities.GetFullpath(SpecialFileName.CrewValidationRuleSet));
            };
            await Task.Run(initializeValidator);
        }

        private async Task initializeSupportingLists(TrackBossEntities connection)
        {
            // Fetch locations.
            List<Location> locations = null;
            Debug.WriteLine("Initializing locations");
            await Task.Run(() => locations = connection.Locations.ToList());
        }
        #endregion

        #region Command Handlers
        protected override void invalidateAllCommands()
        {
            // Perform base invalidation of commands.
            base.invalidateAllCommands();

            // Perform invalidate on commands this object defines.
            this.sortCommandValue.InvalidateCanExecuteChanged();
            this.reverseSortCommandValue.InvalidateCanExecuteChanged();
            this.initializeCommandValue.RaiseCanExecuteChanged();
            this.saveAndAddNewCommandValue.InvalidateCanExecuteChanged();
            //this.duplicateCommandValue.InvalidateCanExecuteChanged();
        }

        protected override bool SaveCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy && this.hasChanges;
        }

        protected override bool DeleteCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy && this.SelectedViewModel != null;
        }

        protected override bool CancelCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy && this.hasChanges;
        }

        protected override void selectionChangeHelper()
        {
            // Run base processes.
            base.selectionChangeHelper();

            if (this.SelectedViewModel != null)
            {
                
            }

            // Invalidate commands which are selection dependent.
            ((RelayCommand)this.DeleteCommand).InvalidateCanExecuteChanged();
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

            // If there are cars and none are selected, then
            // select the first one.
            if (this.SelectedViewModel == null && this.ViewModels.Count > 0)
                this.SelectedViewModel = (CrewViewModel)this.ViewModelsView.CurrentItem;
        }

        private void updateConfiguration()
        {
            this.configuration.SortType = this.selectedSort;
            this.configuration.SortDirection = this.currentSortDirection;
        }

        private bool SaveAndAddNewCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy && this.hasChanges;
        }

        private void SaveAndAddNewCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.SaveAndAddNewCommand.CanExecute(obj))
                return;

            // Call commands. Note: their respective CanExecute methods
            // are called by their Execute methods.

            // Save pending changes.
            this.SaveCommand.Execute(obj);

            // Add new crew.
            this.NewCommand.Execute(obj);
        }

        private bool ReverseSortCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private void ReverseSortCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.ReverseSortCommand.CanExecute(obj))
                return;

            // Store any current selection.
            CrewViewModel lastSelectedViewModel = this.SelectedViewModel;

            // Reverse sort direction.
            if (this.CurrentSortDirection == ListSortDirection.Ascending)
                this.CurrentSortDirection = ListSortDirection.Descending;
            else
                this.CurrentSortDirection = ListSortDirection.Ascending;

            // Apply sort.
            this.updateSort();

            // Restore any previous selection.
            if (lastSelectedViewModel != null)
                this.SelectedViewModel = lastSelectedViewModel;

            // Update configuration.
            this.updateConfiguration();
        }

        private void updateSort()
        {
            this.viewModelsViewValue.SortDescriptions.Clear();
            if (this.SelectedSort == CrewSortType.FullName)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("Crew.LastName", this.currentSortDirection));
            else if (this.SelectedSort == CrewSortType.LastNameOnly)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("Crew.LastName", this.currentSortDirection));
            else if (this.SelectedSort == CrewSortType.FirstNameOnly)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("Crew.FirstName", this.currentSortDirection));
            else
                throw new NotSupportedException("The specified reverse sort is unsupported.");
        }

        private bool SortCommandCanExecute(CrewSortType obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private void SortCommandExecute(CrewSortType obj)
        {
            // Make sure this is allowed.
            if (!this.SortCommand.CanExecute(obj))
                return;

            // Select sort and default order.
            this.SelectedSort = obj;
            this.CurrentSortDirection = ListSortDirection.Ascending;

            // Apply sort.
            this.updateSort();

            // Update configuration.
            this.updateConfiguration();
        }

        private void performViewModelCleanUp()
        {
            // Dispose of existing data set.
            foreach (CrewViewModel crewViewModel in this.ViewModels)
            {
                crewViewModel.ChangesMade -= this.CrewViewModel_ChangesMade;
                crewViewModel.Dispose();
            }
            this.ViewModels.Clear();
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

        protected override async Task closeCommandExecuteHelper()
        {
            // Check and see if there are pending changes.
            if (!this.hasChanges)
                return;

            // If there are pending changes, two things must happen:
            //
            //  1) Need to make sure there are no validation errors.
            //  2) Need to prompt to save changes.
            //
            // To prevent the most number of clicks, ask about the
            // saving of the changes first. Validation errors which
            // occur as the result of changes which are being discarded
            // shouldn't result in an extra click for the user. Also,
            // this offloads and compartmentalizes the check for
            // validation errors to the save command's methods.
            ShowMessageBoxMessage message = new
                ShowMessageBoxMessage()
            {
                Title = string.Format("{0} - Crews", this.conductor.Name),
                Button = MessageBoxButton.YesNo,
                Icon = MessageBoxImage.Question,
                Message = "There are unsaved changes. Do you wish to save them?",
            };

            // Send message.
            Messenger.Default.Send<ShowMessageBoxMessage>(message);

            // Check result.
            if (message.Result == MessageBoxResult.No)
                return;

            // Call save processes.

            // Perform save.
            bool cancelled = !await this.saveAsync();
            if (cancelled)
                return;

            // Commit modifications and additions (deletions are handled on-the-fly).
            //this.commitHistory();
        }

        protected override void deleteCommandExecuteHelper(object obj)
        {
            // Get selected crew.
            CrewViewModel selectedCrewViewModel = this.SelectedViewModel;
            Crewmember selectedCrew = SelectedViewModel.ToCrew().ToCrewMember();

            // Verify the user wants to delete this car.
            ShowMessageBoxMessage message = new ShowMessageBoxMessage();
            message.Message = string.Format(
                                "This will delete the currently selected car:\n\n{0}\n\nAre you sure you want to do this?", selectedCrew);
            message.Button = MessageBoxButton.YesNo;
            message.Icon = MessageBoxImage.Exclamation;
            message.Title = this.conductor.Name;
            Messenger.Default.Send<ShowMessageBoxMessage>(message);

            // Check result.
            if (message.Result == MessageBoxResult.Yes)
            {
                // Remove selection and remove ViewModel from list.
                this.SelectedViewModel = null;
                this.ViewModels.Remove(selectedCrewViewModel);

                // Disconnect change event. This event should not fire
                // for deleted ViewModels.
                selectedCrewViewModel.ChangesMade -= this.CrewViewModel_ChangesMade;

                // Remove validation results pertaining to this car, if any.
                for (int i = this.validationResultsValue.Count - 1; i > -1; i--)
                {
                    DataValidationResultViewModel<CrewViewModel> result = this.validationResultsValue[i];
                    if (result.Source == selectedCrewViewModel)
                        this.validationResultsValue.RemoveAt(i);
                }

                // Remove history item, if applicable.
                //this.conductor.HistoryManager.RemoveHistoryItem(selectedCrewViewModel.UniqueId);

                // Dispose of removed ViewModel.
                selectedCrewViewModel.Dispose();

                // Get crew being removed.
                Crewmember crewMemberToRemove = selectedCrewViewModel.ToCrew().ToCrewMember();

                // Perform clean-up on crew.
                //await carToRemove.RollingStock.Photo.Clear();

                // Remove car from data context. If this is a new crew, there
                // will be no currently assigned ID and nothing else to do.
                if (crewMemberToRemove.ID != 0)
                {
                    if (this.trackBossConnection.Crewmembers.Remove(crewMemberToRemove) == null)
                    {
                        // TODO: Decide what to do here.
                    }
                }

                // Set changes state.
                this.hasChanges = true;

                // Invalidate commands.
                this.invalidateAllCommands();
            }
        }

        private void addNewCrewHelper(Crewmember newCrew)
        {
            // Create ViewModel and attach event handlers.
            CrewViewModel newCrewViewModel = new CrewViewModel(this, newCrew);
            newCrewViewModel.ChangesMade += this.CrewViewModel_ChangesMade;

            // Add to list.
            this.ViewModels.Add(newCrewViewModel);

            // Select new crew.
            this.SelectedViewModel = newCrewViewModel;

            // Mark designer as having changes.
            this.hasChanges = true;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        protected override void newCommandExecuteHelper(object obj)
        {
            // Create new crew.
            Crewmember newCrew = Crewmember.Create();

            // Add new car to list.
            this.addNewCrewHelper(newCrew);

            // Queue validation.
            this.validationTimer.Stop();
            this.validationTimer.Start();
        }

        private void saveCrews(TrackBossEntities connection)
        {
            // New crews will have changes but will also have an ID of zero.
            //IEnumerable<Crewmember> modifiedCrews = this.ViewModels.Where(x => x.HasChanges).Select(x => x.ToCrew().ToCrewMember());
            //IEnumerable<Crewmember> newCrews = modifiedCrews.Where(x => x.ID == 0);
            //modifiedCrews = modifiedCrews.Except(newCrews);

            IEnumerable<CrewViewModel> newViewModels = this.ViewModels.Where(x => x.ToCrew().ToCrewMember().ID == 0);
            IEnumerable<Crewmember> allCrews = this.ViewModels.Select(x => x.ToCrew().ToCrewMember());
            IEnumerable<Crewmember> newCrews = newViewModels.Select(x => x.ToCrew().ToCrewMember());

            // Add new owners.
            foreach (Crewmember newCrew in newCrews)
                connection.Crewmembers.Add(newCrew);
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
                    // Save supporting lists.
                    this.saveCrews(this.trackBossConnection);
                    //this.saveRoads(this.trackBossConnection);
                    //this.saveCarTypes(this.trackBossConnection);

                    // Save cars.
                    //this.saveCars(this.trackBossConnection);

                    // Next, save.
                    this.trackBossConnection.SaveChanges();
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

        private void SelectCrewById(long id)
        {
            foreach (CrewViewModel crewViewModel in this.ViewModels)
            {
                if (crewViewModel.CrewMemViewModel.ID == id)
                {
                    this.SelectedViewModel = crewViewModel;
                    break;
                }
            }
        }

        protected override async void saveCommandExecuteHelper(object obj)
        {
            // Perform save.
            bool cancelled = !await this.saveAsync();
            if (cancelled)
                return;

            // Commit modifications and additions (deletions are handled on-the-fly).
            //this.commitHistory();

            // Store selected car so it can be reselected after save
            // (if applicable).
            long lastSelectedCrewId = -1;
            if (this.SelectedViewModel != null)
                lastSelectedCrewId = this.SelectedViewModel.ToCrew().ID;

            // Perform cleanup.
            this.performViewModelCleanUp();

            // Destroy connection object.
            this.trackBossConnection.Dispose();
            this.trackBossConnection = null;

            // Reinitialize the data set.
            if (this.InitializeCommand.CanExecute(null))
                await this.initializeAsync();

            // Reselect last selected car, if applicable.
            if (lastSelectedCrewId != -1)
                SelectCrewById(lastSelectedCrewId);

            // Clear changes.
            this.hasChanges = false;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        #endregion

        #region Event Handlers

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

        private void CrewViewModel_ChangesMade(object sender, EventArgs e)
        {
            // Validate parameters.
            Debug.WriteLine(string.Format("YardsDesignerViewModel - YaViewModel_ChangesMade: {0}", sender.ToString()));
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

        #endregion

        #region Properties
        public ObservableCollection<DataValidationResultViewModel<CrewViewModel>> ValidationResults
        {
            get { return this.validationResultsValue; }
        }

        public GridLength ListSplitterPosition
        {
            get { return this.currentListSplitterPosition; }
            set
            {
                if (this.currentListSplitterPosition != value)
                {
                    this.currentListSplitterPosition = value;
                    this.NotifyPropertyChanged();
                    this.configuration.ListSplitterPosition = this.currentListSplitterPosition.Value;
                }
            }
        }

        public GridLength ErrorWindowSplitterPosition
        {
            get { return this.currentErrorWindowSplitterPosition; }
            set
            {
                if (this.currentErrorWindowSplitterPosition != value)
                {
                    this.currentErrorWindowSplitterPosition = value;
                    this.NotifyPropertyChanged();
                    this.configuration.ErrorWindowSplitterPosition = this.currentErrorWindowSplitterPosition.Value;
                }
            }
        }

        public CrewSortType SelectedSort
        {
            get { return this.selectedSort; }
            private set
            {
                if (this.selectedSort != value)
                {
                    this.selectedSort = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ICollectionView ViewModelsView
        {
            get { return this.viewModelsViewValue; }
        }

        public IAsyncCommand InitializeCommand
        {
            get { return this.initializeCommandValue; }
        }

        public RelayCommand<CrewSortType> SortCommand
        {
            get { return this.sortCommandValue; }
        }

        public ICommand ReverseSortCommand
        {
            get { return this.reverseSortCommandValue; }
        }
        public ICommand SaveAndAddNewCommand
        {
            get { return this.saveAndAddNewCommandValue; }
        }

        public ListSortDirection CurrentSortDirection
        {
            get { return this.currentSortDirection; }
            private set
            {
                if (this.currentSortDirection != value)
                {
                    this.currentSortDirection = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
