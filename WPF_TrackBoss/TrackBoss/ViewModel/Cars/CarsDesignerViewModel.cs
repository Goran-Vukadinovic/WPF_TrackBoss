using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using TrackBoss.Configuration;
using TrackBoss.Configuration.Enumerations;
using TrackBoss.Configuration.History;
using TrackBoss.Configuration.IO;
using TrackBoss.Configuration.Preferences.Layout;
using TrackBoss.Data;
using TrackBoss.Data.Validation;
using TrackBoss.Mvvm.Shared.Commands;
using TrackBoss.Mvvm.Shared.Messages;
using TrackBoss.Mvvm.Shared.Model;
using TrackBoss.Mvvm.Shared.ViewModel;
using TrackBoss.Mvvm.Validation.Messages;
using TrackBoss.Mvvm.Validation.ViewModel;
using TrackBoss.Mvvm.Validation.ViewModel.Rules;
using TrackBoss.Shared.Comparers;
using TrackBoss.Shared.Enumerations;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.Comparers;
using TrackBoss.ViewModel.Dialogs;
using TrackBoss.ViewModel.Locomotives;
using TrackBoss.ViewModel.RollingStocks;
using TrackBoss.ViewModel.Shared;
using TrackBoss.ViewModel.Shared.Messages;
using TrackBoss.Windsor;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cars
{
    public class CarsDesignerViewModel : GenericListBasedViewModel<CarViewModel>, IGenericSortEnabledViewModel<RollingStockSortType>, IInitializableViewModel
    {
        #region Fields
        
        private readonly RelayCommand saveAndAddNewCommandValue;

        private readonly RelayCommand duplicateCommandValue;

        private readonly RelayCommand<RollingStockSortType> sortCommandValue;

        private readonly RelayCommand reverseSortCommandValue;

        private readonly AsyncCommand initializeCommandValue;

        private readonly RelayCommand addCarTypeCommandValue;

        private readonly RelayCommand addRoadCommandValue;

        private readonly RelayCommand addOwnerCommandValue;

        private readonly RelayCommand showLocationSelectionDialogCommandValue;

        private readonly DispatcherTimer validationTimer;

        private Validator<CarViewModel> validator;

        private Conductor conductor;

        private List<CarViewModel> viewModelsHistory;

        private ObservableCollection<DataValidationResultViewModel<CarViewModel>> validationResultsValue;

        private ObservableCollection<ColorItemViewModel> colors;

        private ObservableCollection<long> lengthsValue;

        private ObservableCollection<CarTypeViewModel> carTypesValue;

        private ObservableCollection<RoadViewModel> roadsValue;

        private ObservableCollection<OwnerViewModel> ownersValue;

        private ObservableCollection<LocationViewModel> locationsValue;

        private ObservableCollection<CommodityViewModel> commoditiesValue;

        private ListCollectionView carTypesViewValue;
        
        private ListCollectionView roadsViewValue;

        private ListCollectionView ownersViewValue;

        private ListCollectionView lengthsViewValue;
        
        private ListCollectionView viewModelsViewValue;
        
        private CarTypeViewModel newCarTypeValue;

        private RoadViewModel newRoadValue;

        private OwnerViewModel newOwnerValue;
        
        private RollingStockSortType selectedSort;

        private ListSortDirection currentSortDirection;

        private GridLength currentListSplitterPosition;

        private GridLength currentErrorWindowSplitterPosition;

        private bool updatingCommoditiesSelection;

        private bool hasChanges;

        private TrackBossEntities trackBossConnection;

        private CarsDesignerLayoutConfiguration configuration;

        private string filterString;


        public delegate void ChildChangesMade(object sender, EventArgs e);
        public ChildChangesMade childChangesMade;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Default constructor. Initializes fields and prepares this object
        /// for use.
        /// </summary>
        public CarsDesignerViewModel()
        {
            // Prepare the conductor.
            this.conductor = CastleWindsor.Default.Resolve<Conductor>();
            this.conductor.Preferences.PreferencesChanged += this.Preferences_PreferencesChanged;
            this.validationTimer = new DispatcherTimer(DispatcherPriority.Input);
            this.validationTimer.Interval = new TimeSpan(0, 0, Conductor.DefaultValidationInterval);
            this.validationTimer.Tick += this.ValidationTimer_Tick;

            // Initialize fields.
            this.viewModelsHistory = new List<CarViewModel>();
            this.currentListSplitterPosition = new GridLength(Conductor.DefaultDesignerListWidth);
            this.currentErrorWindowSplitterPosition = new GridLength(Conductor.DefaultDesignerErrorWindowHeight);
            this.selectedSort = RollingStockSortType.Number;
            this.currentSortDirection = ListSortDirection.Ascending;
            this.carTypesValue = new ObservableCollection<CarTypeViewModel>();
            this.roadsValue = new ObservableCollection<RoadViewModel>();
            this.ownersValue = new ObservableCollection<OwnerViewModel>();
            this.locationsValue = new ObservableCollection<LocationViewModel>();
            this.commoditiesValue = new ObservableCollection<CommodityViewModel>();

            // Hookup command handlers.
            this.showLocationSelectionDialogCommandValue = new RelayCommand(this.ShowLocationSelectionDialogCommandExecute, this.ShowLocationSelectionDialogCommandCanExecute);
            this.sortCommandValue = new RelayCommand<RollingStockSortType>(this.SortCommandExecute, this.SortCommandCanExecute);
            this.reverseSortCommandValue = new RelayCommand(this.ReverseSortCommandExecute, this.ReverseSortCommandCanExecute);
            this.initializeCommandValue = new AsyncCommand(this.InitializeCommandExecute, this.InitializeCommandCanExecute);
            this.saveAndAddNewCommandValue = new RelayCommand(this.SaveAndAddNewCommandExecute, this.SaveAndAddNewCommandCanExecute);
            this.duplicateCommandValue = new RelayCommand(this.DuplicateCommandExecute, this.DuplicateCommandCanExecute);
            this.addCarTypeCommandValue = new RelayCommand(this.AddCarTypeCommandExecute, this.AddCarTypeCommandCanExecute);
            this.addRoadCommandValue = new RelayCommand(this.AddRoadCommandExecute, this.AddRoadCommandCanExecute);
            this.addOwnerCommandValue = new RelayCommand(this.AddOwnerCommandExecute, this.AddOwnerCommandCanExecute);

            SelectionChangedCommand = new RelayCommand<ListBox>(SelectionChanged);

            // Load lists.
            this.colors = new ObservableCollection<ColorItemViewModel>();
            this.loadDefaultColors();
            this.loadDefaultLengths();

            // Use stored or default layout configuration, whichever is applicable.
            CarsDesignerLayoutConfiguration defaultConfiguration = new CarsDesignerLayoutConfiguration();
            this.configuration = this.conductor.Preferences.LoadLayoutConfiguration<CarsDesignerLayoutConfiguration>(defaultConfiguration.Name);
            if (this.configuration == null)
            {
                this.configuration = defaultConfiguration;
                this.conductor.Preferences.SaveLayoutConfiguration(this.configuration);
            }
            this.currentListSplitterPosition = new GridLength(this.configuration.ListSplitterPosition);
            this.currentErrorWindowSplitterPosition = new GridLength(this.configuration.ErrorWindowSplitterPosition);

            // Set sort from configuration (or default).
            this.selectedSort = this.configuration.SortType;
            this.currentSortDirection = this.configuration.SortDirection;

            // Register message handlers.
            Messenger.Default.Register<ValidationSourceSelectedMessage>(this, this.ValidationSourceSelectedMessageHandler);
            Messenger.Default.Register<ShowHistoryItemMessage>(this, this.ShowHistoryItemMessageHandler);
            Messenger.Default.Register<CloseDesignersMessage>(this, this.CloseDesignersMessageHandler);
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            // If disposing perform clean-up.
            if(disposing)
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

        #region Override Methods

        protected override void invalidateAllCommands()
        {
            // Perform base invalidation of commands.
            base.invalidateAllCommands();

            // Perform invalidate on commands this object defines.
            this.sortCommandValue.InvalidateCanExecuteChanged();
            this.reverseSortCommandValue.InvalidateCanExecuteChanged();
            this.initializeCommandValue.RaiseCanExecuteChanged();
            this.saveAndAddNewCommandValue.InvalidateCanExecuteChanged();
            this.duplicateCommandValue.InvalidateCanExecuteChanged();
            this.showLocationSelectionDialogCommandValue.InvalidateCanExecuteChanged();
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

        protected override bool CancelCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy && this.hasChanges;
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
                    Title = string.Format("{0} - Cars", this.conductor.Name),
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
            this.commitHistory();
        }

        protected override void deleteCommandExecuteHelper(object obj)
        {
            //// TODO: Test. Remove!
            //CarDataObject carDataObject = (CarDataObject)this.SelectedViewModel.ToCar();
            //string xml = GenericSerializer<CarDataObject>.Serialize(carDataObject);
            //File.WriteAllText(@"C:\Development\TrackBoss\Test\Output\car.xml", xml);
            //return;
            
            // Get selected car.
            CarViewModel selectedCarViewModel = this.SelectedViewModel;
            Car selectedCar = SelectedViewModel.ToCar();

            // Verify the user wants to delete this car.
            ShowMessageBoxMessage message = new ShowMessageBoxMessage();
            message.Message = string.Format(
                                "This will delete the currently selected car:\n\n{0}\n\nAre you sure you want to do this?", selectedCar);
            message.Button = MessageBoxButton.YesNo;
            message.Icon = MessageBoxImage.Exclamation;
            message.Title = this.conductor.Name;
            Messenger.Default.Send<ShowMessageBoxMessage>(message);

            // Check result.
            if (message.Result == MessageBoxResult.Yes)
            {
                // Remove selection and remove ViewModel from list.
                this.SelectedViewModel = null;
                this.ViewModels.Remove(selectedCarViewModel);

                // Disconnect change event. This event should not fire
                // for deleted ViewModels.
                selectedCarViewModel.ChangesMade -= this.CarViewModel_ChangesMade;

                // Remove validation results pertaining to this car, if any.
                for (int i = this.validationResultsValue.Count - 1; i > -1; i--)
                {
                    DataValidationResultViewModel<CarViewModel> result = this.validationResultsValue[i];
                    if (result.Source == selectedCarViewModel)
                        this.validationResultsValue.RemoveAt(i);
                }

                // Remove history item, if applicable.
                this.conductor.HistoryManager.RemoveHistoryItem(selectedCarViewModel.UniqueId);

                // Dispose of removed ViewModel.
                selectedCarViewModel.Dispose();

                // Get car being removed.
                Car carToRemove = selectedCarViewModel.ToCar();

                // Perform clean-up on car.
                //await carToRemove.RollingStock.Photo.Clear();

                // Remove car from data context. If this is a new car, there
                // will be no currently assigned ID and nothing else to do.
                if (carToRemove.ID != 0)
                {
                    if (this.trackBossConnection.Cars.Remove(carToRemove) == null)
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

        protected override void newCommandExecuteHelper(object obj)
        {
            // Create new car.
            Car newCar = Car.Create();

            // Set default value for dwell time.
            newCar.RollingStock.RollingStockStatus.DwellTimeData.CurrentValue = this.conductor.Settings.Operations.GlobalDwellTimeMethodValue;

            // Create placeholder number for car.
            newCar.RollingStock.Number = "(New Car)";

            // Clear any current filter.
            this.FilterString = null;

            // Add new car to list.
            this.addNewCarHelper(newCar);

            // Queue validation.
            this.validationTimer.Stop();
            this.validationTimer.Start();
        }

        protected override async void saveCommandExecuteHelper(object obj)
        {
            // Perform save.
            bool cancelled = !await this.saveAsync();
            if (cancelled)
                return;

            // Commit modifications and additions (deletions are handled on-the-fly).
            this.commitHistory();

            // Store selected car so it can be reselected after save
            // (if applicable).
            long lastSelectedCarId = -1;
            if (this.SelectedViewModel != null)
                lastSelectedCarId = this.SelectedViewModel.ToCar().ID;
            
            // Perform cleanup.
            this.performViewModelCleanUp();
            
            // Destroy connection object.
            this.trackBossConnection.Dispose();
            this.trackBossConnection = null;

            // Reinitialize the data set.
            if (this.InitializeCommand.CanExecute(null))
                await this.initializeAsync();

            // Reselect last selected car, if applicable.
            if (lastSelectedCarId != -1)
                this.selectCarById(lastSelectedCarId);

            // Clear changes.
            this.hasChanges = false;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        private bool ShowLocationSelectionDialogCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy;
        }

        private void ShowLocationSelectionDialogCommandExecute(object obj)
        {
            if (!this.ShowLocationSelectionDialogCommand.CanExecute(obj))
                return;

            Location[] locations = this.locationsValue.Select(x => x.ToLocation()).ToArray();
            RollingStockViewModel selectedRollingStock = this.SelectedViewModel.RollingStock;
            LocationSelectionDialogViewModel dialogViewModel = 
                new LocationSelectionDialogViewModel(locations, selectedRollingStock, this.SelectedViewModel.DisplayText);
            ShowLocationSelectionDialogMessage.Send(dialogViewModel);
        }

        protected override bool SaveCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy && this.hasChanges;
        }

        protected override bool DeleteCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy && this.SelectedViewModel != null;
        }

        protected override void selectionChangeHelper()
        {
            // Run base processes.
            base.selectionChangeHelper();

            // If a car is currently selected, then update the selection
            // status of commodities.
            if(this.SelectedViewModel != null)
            {
                // Get selected commodities.
                int[] selectedCommodities = this.SelectedViewModel.GetAssignedCommodities();
                Debug.WriteLine("Selected commodities count: " + selectedCommodities.Length);

                // Update commodities list.
                this.updateCommoditiesSelection(selectedCommodities);
            }

            // Invalidate commands which are selection dependent.
            ((RelayCommand)this.DeleteCommand).InvalidateCanExecuteChanged();
            ((RelayCommand)this.DuplicateCommand).InvalidateCanExecuteChanged();
        }

        #endregion

        #region Command Handlers
        public ObservableCollection<CarViewModel> selectedViewModels;
        private void SelectionChanged(ListBox listBox)
        {
            if (selectedViewModels == null)
                selectedViewModels = new ObservableCollection<CarViewModel>();

            if (listBox.SelectedItems.Count > 1)
            {
                selectedViewModels.Clear();
                foreach (CarViewModel selectedViewModel in listBox.SelectedItems)
                    selectedViewModels.Add(selectedViewModel);
                IsNotMultiSelection = false;
            }
            else
            {
                selectedViewModels.Clear();
                IsNotMultiSelection = true;
            }
        }

        private bool isNotMultiSelection = true;
        public bool IsNotMultiSelection
        {
            get
            {
                return isNotMultiSelection;
            }
            set
            {
                isNotMultiSelection = value;
                this.NotifyPropertyChanged();
            }
        }

        private bool AddOwnerCommandCanExecute(object obj)
        {
            // Determine if this should be allowed.
            return this.SelectedViewModel != null && this.newOwnerValue != null &&
                !(string.IsNullOrWhiteSpace(this.newOwnerValue.FirstName) || string.IsNullOrWhiteSpace(this.newOwnerValue.LastName));
        }

        private void AddOwnerCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AddOwnerCommand.CanExecute(obj))
                return;

            // Unhook changed handler.
            this.newOwnerValue.PropertyChanged -= this.NewOwnerValue_PropertyChanged;

            // Select for the current car.
            this.SelectedViewModel.RollingStock.Owner = this.newOwnerValue;

            // Add to main list.
            this.ownersValue.Add(this.newOwnerValue);

            // Prepare next new owner.
            this.initializeNewOwner();

            // Notify property.
            this.NotifyPropertyChanged(nameof(CarsDesignerViewModel.NewOwner));
        }

        private bool AddRoadCommandCanExecute(object obj)
        {
            // Determine if this should be allowed.
            return this.SelectedViewModel != null && this.newRoadValue != null && !string.IsNullOrWhiteSpace(this.newRoadValue.Name);
        }

        private void AddRoadCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AddRoadCommand.CanExecute(obj))
                return;

            // Unhook changed handler.
            this.newRoadValue.PropertyChanged -= this.NewRoadValue_PropertyChanged;

            // Select for the current car.
            this.SelectedViewModel.RollingStock.Road = this.newRoadValue;

            // Add to main list.
            this.roadsValue.Add(this.newRoadValue);

            // Prepare next new road.
            this.initializeNewRoad();

            // Notify property.
            this.NotifyPropertyChanged(nameof(CarsDesignerViewModel.NewRoad));
        }

        private bool AddCarTypeCommandCanExecute(object obj)
        {
            // Determine if this should be allowed.
            return this.SelectedViewModel != null && this.newCarTypeValue != null && !string.IsNullOrWhiteSpace(this.newCarTypeValue.Name);
        }

        private void AddCarTypeCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AddCarTypeCommand.CanExecute(obj))
                return;

            // Unhook changed handler.
            this.newCarTypeValue.PropertyChanged -= this.NewCarTypeValue_PropertyChanged;

            // Select for the current car.
            this.SelectedViewModel.CarType = this.newCarTypeValue;

            // Add to main list.
            this.carTypesValue.Add(this.newCarTypeValue);

            // Prepare next new road.
            this.initializeNewCarType();

            // Notify property.
            this.NotifyPropertyChanged(nameof(CarsDesignerViewModel.NewCarType));
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
            CarViewModel lastSelectedViewModel = this.SelectedViewModel;

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

        private bool SortCommandCanExecute(RollingStockSortType obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private void SortCommandExecute(RollingStockSortType obj)
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
                this.SelectedViewModel = (CarViewModel)this.ViewModelsView.CurrentItem;
        }

        private bool DuplicateCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy && this.SelectedViewModel != null;
        }

        private async void DuplicateCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.DuplicateCommand.CanExecute(obj))
                return;

            // Set status.
            this.StatusModel.SetStatus("Duplicating car. Please wait.");

            try
            {
                // Get selected car.
                Car selectedCar = this.SelectedViewModel.ToCar();

                // Duplicate.
                Car clonedCar = await Car.Clone(selectedCar);
                clonedCar.RollingStock.IncrementNumber();

                // Add to list.
                this.addNewCarHelper(clonedCar);

                // Queue validation.
                this.validationTimer.Stop();
                this.validationTimer.Start();
            }
            finally
            {
                this.StatusModel.ClearStatus();
            }
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

            // Add new car.
            this.NewCommand.Execute(obj);
        }

        #endregion

        #region Private Methods

        private bool ViewModelsViewFilter(object obj)
        {
            // Get object.
            CarViewModel carViewModel = obj as CarViewModel;
            if (obj == null)
                return false;

            // Use filter string, if present.
            if (!string.IsNullOrWhiteSpace(this.filterString))
            {
                // Check number, road, and car type as these are currently the three items
                // which are visible to the user.
                string formatString;
                switch (this.SelectedSort)
                {
                    case RollingStockSortType.Road:
                        formatString = "R N T";
                        break;

                    case RollingStockSortType.Type:
                        formatString = "T N R";
                        break;
                    
                    default:
                        formatString = "N R T";
                        break;
                }

                string formattedText = carViewModel.ToString(formatString);
                formattedText = formattedText.ToLower();

                string caseInsensitiveFilterString = this.filterString.ToLower();

                return formattedText.Contains(caseInsensitiveFilterString);
            }

            // By default, allow always.
            return true;
        }

        private void updateCommoditiesSelection(int[] selectedCommodities)
        {
            // Prevent updates.
            this.updatingCommoditiesSelection = true;

            // Clear any current selection.
            foreach (CommodityViewModel commodity in this.commoditiesValue)
            {
                commodity.IsSelected = false;
                commodity.ResetChangeTracking();
                commodity.StartTrackingChanges();
            }

            // Apply any selection.
            foreach (int id in selectedCommodities)
                this.commoditiesValue.First(x => x.ID == id).IsSelected = true;

            // Re-enable updates.
            this.updatingCommoditiesSelection = false;
        }

        private void addNewCarHelper(Car newCar)
        {
            // Create ViewModel and attach event handlers.
            CarViewModel newCarViewModel = new CarViewModel(this, newCar);
            newCarViewModel.ChangesMade += this.CarViewModel_ChangesMade;
            
            // Add to list.
            this.ViewModels.Add(newCarViewModel);

            // Select new car.
            this.SelectedViewModel = newCarViewModel;

            // Mark designer as having changes.
            this.hasChanges = true;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        private void commitHistory()
        {
            // Get list of cars with changes. Order them by change date/time stamp.
            // NOTE: not using OrderByDescending because history manager reverses
            // them.
            IEnumerable<CarViewModel> modifiedViewModels = this.ViewModels.Where(x => x.HasChanges).OrderBy(x => x.LastChanged);
            foreach(CarViewModel viewModel in modifiedViewModels)
            {
                // Create history item.
                HistoryItem newHistoryItem = new HistoryItem
                {
                    UniqueIdentifier = viewModel.UniqueId,
                    ItemType = HistoryItemType.Car,
                    Text = viewModel.DisplayText,
                    Data = null,
                };

                // Add to history manager.
                this.conductor.HistoryManager.AddUpdateHistoryItem(newHistoryItem);
            }
        }

        /// <summary>
        /// Updates the unbound configuration settings.
        /// </summary>
        private void updateConfiguration()
        {
            this.configuration.SortType = this.selectedSort;
            this.configuration.SortDirection = this.currentSortDirection;
        }

        /// <summary>
        /// Updates the sorting for the ViewModels' collection view.
        /// </summary>
        private void updateSort()
        {
            this.viewModelsViewValue.SortDescriptions.Clear();
            if (this.SelectedSort == RollingStockSortType.Number)
            {
                if (this.CurrentSortDirection == ListSortDirection.Ascending)
                    this.viewModelsViewValue.CustomSort = new CarViewModelComparer();
                else
                    this.viewModelsViewValue.CustomSort = new CarViewModelReverseComparer();
            }
            else if (this.SelectedSort == RollingStockSortType.CreatedDate)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("ID", this.currentSortDirection));
            else if (this.SelectedSort == RollingStockSortType.Road)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("RollingStock.Road", this.currentSortDirection));
            else if (this.SelectedSort == RollingStockSortType.Type)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("CarType", this.currentSortDirection));
            else
                throw new NotSupportedException("The specified reverse sort is unsupported.");
        }

        /// <summary>
        /// Locates a CarViewModel by the ID of the car is represents.
        /// </summary>
        /// <param name="id">ID of the car to select.</param>
        private void selectCarById(long id)
        {
            foreach (CarViewModel carViewModel in this.ViewModels)
            {
                if(carViewModel.ID == id)
                {
                    this.SelectedViewModel = carViewModel;
                    break;
                }
            }
        }

        private bool cancelIfHasValidationErrors()
        {
            // Check and see if there are validation errors.
            int count = this.validationResultsValue.Count(x => x.Entity is DataValidationRuleViewModel);
            if (count > 0 && this.configuration.ShowDialogOnSaveIfValidationErrors)
            {
                // Create ViewModel.
                HasValidationErrorsDialogViewModel errorsDialogViewModel = new
                    HasValidationErrorsDialogViewModel(this.conductor.Name, 
                                                       "Any car with validation errors will NOT be used during build processes. Do you wish to proceed?");
                ShowHasValidationErrorsDialogMessage message = new ShowHasValidationErrorsDialogMessage(errorsDialogViewModel);

                // Display dialog.
                Messenger.Default.Send<ShowHasValidationErrorsDialogMessage>(message);

                // Get whether or not dialog should be shown again. Only do this if
                // the user did not click "Cancel" on the dialog. Setting a setting
                // if the user clicked "Cancel" on something would be weird.
                if(!errorsDialogViewModel.Cancelled)
                    this.configuration.ShowDialogOnSaveIfValidationErrors = errorsDialogViewModel.ShowDialogOnValidationErrors;

                // Return whether or not a save cancellation was requested.
                return errorsDialogViewModel.Cancelled;
            }

            // Return no cancellation requested.
            return false;
        }

        private async Task performValidation()
        {
            Action performValidation = () =>
            {
                // Run validation.
                Debug.WriteLine("Performing validation.");
                List<DataValidationResult<CarViewModel>> validationResults = new List<DataValidationResult<CarViewModel>>();
                foreach (CarViewModel carViewModel in this.ViewModels)
                {
                    List<DataValidationResult<CarViewModel>> results = this.validator.Validate(carViewModel);
                    validationResults.AddRange(results);
                }
                
                // Clear any existing results.
                if (this.validationResultsValue != null)
                    this.validationResultsValue.Clear();
                else
                    this.validationResultsValue = new ObservableCollection<DataValidationResultViewModel<CarViewModel>>();

                // If there are results, then perform addition.
                if (validationResults.Count > 0)
                {
                    Debug.WriteLine("Adding validation results.");

                    // This MUST happen on the UI thread.
                    Action addResults = () =>
                    {
                        // Add current results.
                        foreach (DataValidationResult<CarViewModel> result in validationResults)
                            this.validationResultsValue.Add(new DataValidationResultViewModel<CarViewModel>(result));
                    };
                    App.Current.Dispatcher.Invoke(addResults);
                }
                
                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(CarsDesignerViewModel.ValidationResults));
            };
            await Task.Run(performValidation);
        }

        private async Task initializeCars(TrackBossEntities connection)
        {
            // Fetch list of cars.
            List<Car> cars = null;
            await Task.Run(() => cars = connection.Cars.ToList());
            
            // Add cars to list.
            if (cars != null)
            {
                foreach (Car car in cars)
                {
                    // Prepare new ViewModel.
                    CarViewModel newCarViewModel = new CarViewModel(this, car);
                    newCarViewModel.ChangesMade += this.CarViewModel_ChangesMade;
                    this.ViewModels.Add(newCarViewModel);
                }
            }
        }

        private async Task initializeSupportingLists(TrackBossEntities connection)
        {
            // Fetch owners, roads, car types, locations, and commodities.

            List<Owner> owners = null;
            Debug.WriteLine("Initializing owners");
            await Task.Run(() => owners = connection.Owners.ToList());

            List<Road> roads = null;
            Debug.WriteLine("Initializing roads");
            await Task.Run(() => roads = connection.Roads.ToList());

            List<CarType> carTypes = null;
            Debug.WriteLine("Initializing car types");
            await Task.Run(() => carTypes = connection.CarTypes.ToList());

            List<Location> locations = null;
            Debug.WriteLine("Initializing locations");
            await Task.Run(() => locations = connection.Locations.ToList());

            List<Commodity> commodities = null;
            Debug.WriteLine("Initializing commodities");
            await Task.Run(() => commodities = connection.Commodities.ToList());

            // Add owners.
            this.ownersValue = new ObservableCollection<OwnerViewModel>();
            foreach (Owner owner in owners)
                this.ownersValue.Add(new OwnerViewModel(owner));
            this.ownersViewValue = new ListCollectionView(this.ownersValue);
            this.ownersViewValue.SortDescriptions.Add(new SortDescription(nameof(OwnerViewModel.DisplayText), ListSortDirection.Ascending));

            // Add roads.
            this.roadsValue = new ObservableCollection<RoadViewModel>();
            foreach (Road road in roads)
                this.roadsValue.Add(new RoadViewModel(road));
            this.roadsViewValue = new ListCollectionView(this.roadsValue);
            this.roadsViewValue.SortDescriptions.Add(new SortDescription(nameof(RoadViewModel.DisplayText), ListSortDirection.Ascending));

            // Add car types.
            this.carTypesValue = new ObservableCollection<CarTypeViewModel>();
            foreach (CarType carType in carTypes)
                this.carTypesValue.Add(new CarTypeViewModel(carType));
            this.carTypesViewValue = new ListCollectionView(this.carTypesValue);
            this.carTypesViewValue.SortDescriptions.Add(new SortDescription(nameof(CarTypeViewModel.DisplayText), ListSortDirection.Ascending));
            this.carTypesViewValue.IsLiveSorting = true;

            // Add locations.
            this.locationsValue = new ObservableCollection<LocationViewModel>();
            foreach (Location location in locations)
                this.locationsValue.Add(new LocationViewModel(location));

            // Add commodities.
            this.commoditiesValue = new ObservableCollection<CommodityViewModel>();
            foreach (Commodity commodity in commodities)
                this.commoditiesValue.Add(new CommodityViewModel(commodity));

            // Initialize new items for direct editing.
            this.initializeNewOwner();
            this.initializeNewRoad();
            this.initializeNewCarType();
        }

        /// <summary>
        /// Initializes this designers lists and properties and prepares the
        /// designer for use.
        /// </summary>
        private async Task initializeAsync()
        {
            try
            {
                // Prepare status.
                this.StatusModel.SetStatus("Loading cars... please wait.");
                
                // Initialize context object.
                this.trackBossConnection = new TrackBossEntities(Conductor.ConnectionString);

                // Disconnect any prior event handlers attached to the supporting
                // lists.
                Debug.WriteLine("Detaching event handlers from supporting lists.");
                if(this.commoditiesValue != null)
                {
                    foreach (CommodityViewModel commodityViewModel in this.commoditiesValue)
                        commodityViewModel.PropertyChanged -= this.CommodityViewModel_PropertyChanged;
                }

                // Initialize all supporting lists.
                Debug.WriteLine("Initializing supporting lists");
                await this.initializeSupportingLists(this.trackBossConnection);

                // Attach event handlers for supporting lists.
                foreach (CommodityViewModel commodityViewModel in this.commoditiesValue)
                    commodityViewModel.PropertyChanged += this.CommodityViewModel_PropertyChanged;

                // Initialize the list of cars.
                Debug.WriteLine("Initializing cars");
                await this.initializeCars(this.trackBossConnection);

                // Load user lists dependent on rolling stock. This MUST BE
                // done after the cars are loaded.
                Debug.WriteLine("Loading user rolling stock lengths");
                this.loadUserRollingStockLengths();

                // Create list view collection.
                Debug.WriteLine(string.Format("ViewModels.Count: {0}", this.ViewModels.Count));
                this.viewModelsViewValue = new ListCollectionView(this.ViewModels)
                {
                    IsLiveSorting = true,
                    Filter = this.ViewModelsViewFilter,
                };
                this.updateSort();

                // Select the first car.
                this.viewModelsViewValue.MoveCurrentToFirst();

                // Finally, load validation rule set.
                this.StatusModel.SetStatus("Validating cars... please wait.");
                await this.initializeValidator();
                await this.performValidation();
                
                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(CarsDesignerViewModel.ViewModelsView));
                this.NotifyPropertyChanged(nameof(CarsDesignerViewModel.ValidationResults));
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
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
                this.validator = new Validator<CarViewModel>(FileUtilities.GetFullpath(SpecialFileName.CarValidationRuleSet));
            };
            await Task.Run(initializeValidator);
        }

        private void saveCars(TrackBossEntities connection)
        {
            // New cars will have changes but will also have an ID of zero.
            IEnumerable<CarViewModel> newViewModels = this.ViewModels.Where(x => x.ID == 0);
            IEnumerable<Car> allCars = this.ViewModels.Select(x => x.ToCar());
            IEnumerable<Car> newCars = newViewModels.Select(x => x.ToCar());
            
            // Add new cars.
            foreach (Car newCar in newCars.ToList())
                connection.Cars.Add(newCar);
        }

        private void saveOwners(TrackBossEntities connection)
        {
            // New cars will have changes but will also have an ID of zero.
            IEnumerable<Owner> modifiedOwners = this.ownersValue.Where(x => x.HasChanges).Select(x => x.ToOwner());
            IEnumerable<Owner> newOwners = modifiedOwners.Where(x => x.ID == 0);
            modifiedOwners = modifiedOwners.Except(newOwners);

            // Add new owners.
            foreach (Owner newOwner in newOwners)
                connection.Owners.Add(newOwner);
        }

        private void saveRoads(TrackBossEntities connection)
        {
            // New cars will have changes but will also have an ID of zero.
            IEnumerable<Road> modifiedRoads = this.roadsValue.Where(x => x.HasChanges).Select(x => x.ToRoad());
            IEnumerable<Road> newRoads = modifiedRoads.Where(x => x.ID == 0);
            modifiedRoads = modifiedRoads.Except(newRoads);

            // Add new owners.
            foreach (Road newRoad in newRoads)
                connection.Roads.Add(newRoad);
        }

        private void saveCarTypes(TrackBossEntities connection)
        {
            // New cars will have changes but will also have an ID of zero.
            IEnumerable<CarType> modifiedCarTypes = this.carTypesValue.Where(x => x.HasChanges).Select(x => x.ToCarType());
            IEnumerable<CarType> newCarTypes = modifiedCarTypes.Where(x => x.ID == 0);
            modifiedCarTypes = modifiedCarTypes.Except(newCarTypes);

            // Add new owners.
            foreach (CarType newCarType in newCarTypes)
                connection.CarTypes.Add(newCarType);
        }

        /// <summary>
        /// Saves any pending changes to the database after verifying no 
        /// validation errors are present.
        /// </summary>
        /// <returns>true if the save completed successfully and is not
        /// cancelled.</returns>
        private async Task<bool> saveAsync()
        {
            try
            {
                // Warn on validation errors.
                if (this.cancelIfHasValidationErrors())
                    return false;

                // Prepare status.
                this.StatusModel.SetStatus("Saving changes... please wait.");

                // Perform save.
                Action save = () =>
                {
                    // Save supporting lists.
                    this.saveOwners(this.trackBossConnection);
                    this.saveRoads(this.trackBossConnection);
                    this.saveCarTypes(this.trackBossConnection);

                    // Save cars.
                    this.saveCars(this.trackBossConnection);

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

        /// <summary>
        /// Loads the default lengths available for rolling stock.
        /// </summary>
        private void loadDefaultLengths()
        {
            // Create lengths collection.
            long[] vendorLengths = { 20, 30, 40, 50, 54, 60, 70, 80, 89 };
            this.lengthsValue = new ObservableCollection<long>(vendorLengths);

            // Create lengths collection view.
            this.lengthsViewValue = new ListCollectionView(this.lengthsValue);
            this.lengthsViewValue.CustomSort = new LongComparer();
        }

        /// <summary>
        /// Loads the set of "default" colors.
        /// </summary>
        private void loadDefaultColors()
        {
            // Get colors.
            string[] colorNames = {"Black",
                                   "Blue",
                                   "Red",
                                   "Brown",
                                   "Gray",
                                   "Silver",
                                   "Green",
                                   "Orange",
                                   "Purple",
                                   "Tan",
                                   "White",
                                   "Yellow" };

            // Instantiate color collection.
            for (int i = 0; i < colorNames.Length; i++)
            {
                // Create new color item.
                ColorItemModel colorItem = new ColorItemModel(colorNames[i]);
                ColorItemViewModel viewModel = new ColorItemViewModel(colorItem);

                // Add.
                this.colors.Add(viewModel);
            }
        }

        private void loadUserRollingStockLengths()
        {
            // Make sure there is user data.
            if (this.ViewModels.Count == 0)
                return;
            
            // Add lengths.
            IEnumerable<long> userLengths = this.ViewModels.Where(x => x.RollingStock.ScaleLength.HasValue).Select(x => x.RollingStock.ScaleLength.Value);
            foreach (long scaleLength in userLengths)
            {
                // I could make an argument that the > operator should
                // actually have the minimum coupler length as its rvalue.
                if (!this.lengthsValue.Contains(scaleLength) && scaleLength > 0L)
                    this.lengthsValue.Add(scaleLength);
            }
        }

        private void performViewModelCleanUp()
        {
            // Dispose of existing data set.
            foreach (CarViewModel carViewModel in this.ViewModels)
            {
                carViewModel.ChangesMade -= this.CarViewModel_ChangesMade;
                carViewModel.Dispose();
            }
            this.ViewModels.Clear();

            // Dispose of existing supporting lists.
            this.ownersValue.Clear();
            this.roadsValue.Clear();

            // Car types must be disposed of properly. This includes any newly
            // created one which may not be a part of the main list yet.
            if (this.NewCarType != null)
            {
                if (!this.carTypesValue.Contains(this.NewCarType))
                    this.NewCarType.Dispose();
            }
            foreach (CarTypeViewModel carType in this.carTypesValue)
                carType.Dispose();
            this.carTypesValue.Clear();

            // Locations must be disposed of properly.
            foreach (LocationViewModel location in this.locationsValue)
                location.Dispose();
            this.locationsValue.Clear();
        }

        private void initializeNewOwner()
        {
            // Create new object.
            Owner newOwner = new Owner();

            // Create ViewModel and hook-up.
            this.newOwnerValue = new OwnerViewModel(newOwner);
            this.newOwnerValue.PropertyChanged += this.NewOwnerValue_PropertyChanged;
        }

        private void initializeNewRoad()
        {
            // Create new object.
            Road newRoad = new Road();

            // Create ViewModel and hook-up.
            this.newRoadValue = new RoadViewModel(newRoad);
            this.newRoadValue.PropertyChanged += this.NewRoadValue_PropertyChanged;
        }

        private void initializeNewCarType()
        {
            // Create new object.
            CarType newCarType = new CarType();

            // Create ViewModel and hook-up.
            this.newCarTypeValue = new CarTypeViewModel(newCarType);
            this.newCarTypeValue.PropertyChanged += this.NewCarTypeValue_PropertyChanged;
        }

        #endregion

        #region Event Handlers
        
        private void Preferences_PreferencesChanged(object sender, EventArgs<string> e)
        {
            Debug.WriteLine(e.Value);
            if (e.Value == nameof(this.conductor.Preferences.CarTypeDisplayOption))
                this.carTypesViewValue.Refresh();
        }

        private async void ValidationTimer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("CarsDesignerViewModel - ValidationTimer_Tick");

            // Deactivate timer.
            this.validationTimer.Stop();
            
            // Perform validation.
            await this.performValidation();
        }

        private void CarViewModel_ChangesMade(object sender, EventArgs e)
        {
            // Validate parameters.
            Debug.WriteLine(string.Format("CarsDesignerViewModel - CarViewModel_ChangesMade: {0}", sender.ToString()));
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


            if (this.childChangesMade != null)
            {
                this.childChangesMade(sender, e);
            }
        }

        private void NewOwnerValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Invalidate command.
            this.addOwnerCommandValue.InvalidateCanExecuteChanged();
        }

        private void NewRoadValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Invalidate command.
            this.addRoadCommandValue.InvalidateCanExecuteChanged();
        }

        private void NewCarTypeValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Invalidate command.
            this.addCarTypeCommandValue.InvalidateCanExecuteChanged();
        }

        private void CommodityViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.updatingCommoditiesSelection)
                return;

            // Update selection status if there is a car currently selected.
            if (e.PropertyName == nameof(CommodityViewModel.IsSelected) && this.SelectedViewModel != null)
                this.SelectedViewModel.UpdateCommodityStatusCommand.Execute(sender);
        }

        #endregion

        #region Message Handlers

        private async void CloseDesignersMessageHandler(CloseDesignersMessage message)
        {
            // Validate message.
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // Handled check required, for now. See note in CloseDesignersMessage's
            // description.
            if (message.Handled)
                return;

            // Mark message as handled.
            message.Handled = true;

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
            ShowMessageBoxMessage prompt = new
                ShowMessageBoxMessage()
            {
                Title = string.Format("{0} - Cars", this.conductor.Name),
                Button = MessageBoxButton.YesNo,
                Icon = MessageBoxImage.Question,
                Message = "There are unsaved changes. Do you wish to save them?",
            };

            // Send message.
            Messenger.Default.Send<ShowMessageBoxMessage>(prompt);

            // Check result.
            if (prompt.Result == MessageBoxResult.No)
                return;

            // Call save processes.

            // Perform save.
            message.Cancel = !await this.saveAsync();
            if (!message.Cancel)
            {
                // Commit modifications and additions (deletions are handled on-the-fly).
                this.commitHistory();
            }
        }

        private void ValidationSourceSelectedMessageHandler(ValidationSourceSelectedMessage message)
        {
            // Validate message.
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // Make sure there is something to do.
            if (message.Handled)
                return;

            // Make sure this object should process the message.
            if(message.Source is CarViewModel)
            {
                // Mark message as handled.
                message.Handled = true;

                // Unset any existing filter so all cars are visible.
                this.FilterString = null;

                // Attempt to locate car. This is technically redundant, but 
                // prevents the case that this object is handling a message
                // for a prior or disposed object (I hope).
                CarViewModel sourceCarViewModel = (CarViewModel)message.Source;
                int index = this.ViewModels.IndexOf(sourceCarViewModel);
                if (index == -1)
                    throw new InvalidOperationException("The car is not in the list.");

                // Select the car.
                this.SelectedViewModel = sourceCarViewModel;

                // Perform navigation to data point.
                NavigateToDataPointMessage navigateToDataPointMessage = new
                    NavigateToDataPointMessage()
                    {
                        Id = message.Id,
                    };
                Messenger.Default.Send(navigateToDataPointMessage);
            }
        }
        
        private void ShowHistoryItemMessageHandler(ShowHistoryItemMessage message)
        {
            // Validate message.
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // Make sure there is something to do.
            if (message.Handled)
                return;

            // Make sure this object should handle the message.
            if (message.HistoryItem.ItemType == HistoryItemType.Car)
            {
                // Attempt to locate item.
                foreach(CarViewModel carViewModel in this.ViewModels)
                {
                    if(carViewModel.UniqueId == (string)message.HistoryItem.UniqueIdentifier)
                    {
                        // Set message as handled.
                        message.Handled = true;

                        // Select item.
                        this.SelectedViewModel = carViewModel;

                        // We're done.
                        break;
                    }
                }
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<ColorItemViewModel> Colors
        {
            get { return this.colors; }
        }

        public ObservableCollection<LocationViewModel> Locations
        {
            get { return this.locationsValue; }
        }

        public ObservableCollection<CommodityViewModel> Commodities
        {
            get { return this.commoditiesValue; }
        }

        public ObservableCollection<DataValidationResultViewModel<CarViewModel>> ValidationResults
        {
            get { return this.validationResultsValue; }
        }

        public ICollectionView LengthsView
        {
            get { return this.lengthsViewValue; }
        }

        public ICollectionView ViewModelsView
        {
            get { return this.viewModelsViewValue; }
        }

        public ICollectionView RoadsView
        {
            get { return this.roadsViewValue; }
        }

        public ICollectionView OwnersView
        {
            get { return this.ownersViewValue; }
        }

        public ICollectionView CarTypesView
        {
            get { return this.carTypesViewValue; }
        }

        public ICommand ShowLocationSelectionDialogCommand
        {
            get { return this.showLocationSelectionDialogCommandValue; }
        }

        public ICommand SaveAndAddNewCommand
        {
            get { return this.saveAndAddNewCommandValue; }
        }

        public ICommand DuplicateCommand
        {
            get { return this.duplicateCommandValue; }
        }

        public RelayCommand<RollingStockSortType> SortCommand
        {
            get { return this.sortCommandValue; }
        }

        public ICommand ReverseSortCommand
        {
            get { return this.reverseSortCommandValue; }
        }

        public IAsyncCommand InitializeCommand
        {
            get { return this.initializeCommandValue; }
        }

        public ICommand AddCarTypeCommand
        {
            get { return this.addCarTypeCommandValue; }
        }

        public ICommand AddRoadCommand
        {
            get { return this.addRoadCommandValue; }
        }

        public ICommand AddOwnerCommand
        {
            get { return this.addOwnerCommandValue; }
        }

        public ICommand SelectionChangedCommand { get; private set; }

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

        public string FilterString
        {
            get { return this.filterString; }
            set
            {
                if(this.filterString != value)
                {
                    // Update and notify.
                    this.filterString = value;
                    this.NotifyPropertyChanged();

                    // Apply filter.
                    this.viewModelsViewValue.Refresh();
                }
            }
        }

        public RollingStockSortType SelectedSort
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

        public bool LocationSectionExpanded
        {
            get { return this.configuration.LocationSectionExpanded; }
            set
            {
                if (this.configuration.LocationSectionExpanded != value)
                {
                    this.configuration.LocationSectionExpanded = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool CommoditiesSectionExpanded
        {
            get { return this.configuration.CommoditiesSectionExpanded; }
            set
            {
                if (this.configuration.CommoditiesSectionExpanded != value)
                {
                    this.configuration.CommoditiesSectionExpanded = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool PhotoSectionExpanded
        {
            get { return this.configuration.PhotoSectionExpanded; }
            set
            {
                if (this.configuration.PhotoSectionExpanded != value)
                {
                    this.configuration.PhotoSectionExpanded = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool MaintenanceSectionExpanded
        {
            get { return this.configuration.MaintenanceSectionExpanded; }
            set
            {
                if (this.configuration.MaintenanceSectionExpanded != value)
                {
                    this.configuration.MaintenanceSectionExpanded = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool CommentsSectionExpanded
        {
            get { return this.configuration.CommentsSectionExpanded; }
            set
            {
                if (this.configuration.CommentsSectionExpanded != value)
                {
                    this.configuration.CommentsSectionExpanded = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool ScaleDataSectionExpanded
        {
            get { return this.configuration.ScaleDataSectionExpanded; }
            set
            {
                if (this.configuration.ScaleDataSectionExpanded != value)
                {
                    this.configuration.ScaleDataSectionExpanded = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public CarTypeViewModel NewCarType
        {
            get { return this.newCarTypeValue; }
            set
            {
                if(this.newCarTypeValue != value)
                {
                    this.newCarTypeValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public OwnerViewModel NewOwner
        {
            get { return this.newOwnerValue; }
            set
            {
                if (this.newOwnerValue != value)
                {
                    this.newOwnerValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public RoadViewModel NewRoad
        {
            get { return this.newRoadValue; }
            set
            {
                if (this.newRoadValue != value)
                {
                    this.newRoadValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
