using GalaSoft.MvvmLight.Messaging;
using Syncfusion.UI.Xaml.Diagram;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using TrackBoss.Model.Enumerations;
using TrackBoss.Mvvm.Shared.Commands;
using TrackBoss.Mvvm.Shared.Messages;
using TrackBoss.Mvvm.Validation.ViewModel;
using TrackBoss.Shared.Enumerations;
using TrackBoss.Shared.Events;
using TrackBoss.Shared.Extensions;
using TrackBoss.ViewModel.Cars;
using TrackBoss.ViewModel.Cities;
using TrackBoss.ViewModel.RollingStocks;
using TrackBoss.ViewModel.Shared;
using TrackBoss.Windsor;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace TrackBoss.ViewModel.Yards
{
    public class YardsDesignerViewModel : GenericListBasedViewModel<YardViewModel>, IInitializableViewModel
    {
        #region Fields
        private readonly RelayCommand saveAndAddNewCommandValue;

        private readonly RelayCommand<CitySortType> sortCommandValue;
        private readonly RelayCommand reverseSortCommandValue;
        private readonly AsyncCommand initializeCommandValue;

        private readonly RelayCommand<YardFilterType> filterCommandValue;

        private readonly RelayCommand newTrackCommandValue;
        private readonly RelayCommand newMultiSwitchCommandValue;
        private readonly RelayCommand deleteSubItemCommandValue;

        private int selTrackTypeIdx = -1;

        private Conductor conductor;
        private List<CityViewModel> viewModelsHistory;
        private Validator<YardViewModel> validator;
        private readonly DispatcherTimer validationTimer;
        private ObservableCollection<DataValidationResultViewModel<YardViewModel>> validationResultsValue;

        private ObservableCollection<CityViewModel> citiesValue;
        private ObservableCollection<RoadViewModel> roadsValue;
        private ObservableCollection<CarTypeViewModel> carTypesValue;
        private List<Train> trains;

        private CitySortType selectedSort;
        private ListSortDirection currentSortDirection;

        private YardFilterType selectedFilter;

        private GridLength currentErrorWindowSplitterPosition;
        private GridLength currentListSplitterPosition;

        private YardsDesignerLayoutConfiguration configuration;

        private ListCollectionView viewModelsViewValue;

        private TrackBossEntities trackBossConnection;

        private ListCollectionView printersViewValue;
        private ObservableCollection<PrinterViewModel> printersValue;

        private readonly SortedDictionary<DwellTimeMethod, string> printOptionsValue;
        private readonly SortedDictionary<PrintListOrder, string> printOrdersValue;

        private readonly SortedDictionary<MultiSwitcherFunction, string> multiSwitcherFunctionsValue;
        private readonly SortedDictionary<MultiSwitcherOption, string> multiSwitcherOptionsValue;

        private bool hasChanges;

        private bool updatingRoadsSelection;
        private bool updatingCartypesSelection;
        #endregion

        #region Constructor(s)
        public YardsDesignerViewModel()
        {
            // Prepare the conductor.
            this.conductor = CastleWindsor.Default.Resolve<Conductor>();
            this.conductor.Preferences.PreferencesChanged += this.Preferences_PreferencesChanged;
            this.validationTimer = new DispatcherTimer(DispatcherPriority.Input);
            this.validationTimer.Interval = new TimeSpan(0, 0, Conductor.DefaultValidationInterval);
            this.validationTimer.Tick += this.ValidationTimer_Tick;

            // Initialize fields.
            this.viewModelsHistory = new List<CityViewModel>();
            this.currentListSplitterPosition = new GridLength(Conductor.DefaultDesignerListWidth);
            this.currentErrorWindowSplitterPosition = new GridLength(Conductor.DefaultDesignerErrorWindowHeight);

            // Hookup command handlers.
            this.sortCommandValue = new RelayCommand<CitySortType>(this.SortCommandExecute, this.SortCommandCanExecute);
            this.reverseSortCommandValue = new RelayCommand(this.ReverseSortCommandExecute, this.ReverseSortCommandCanExecute);
            this.initializeCommandValue = new AsyncCommand(this.InitializeCommandExecute, this.InitializeCommandCanExecute);

            this.filterCommandValue = new RelayCommand<YardFilterType>(this.FilterCommandExecute, this.FilterCommandCanExecute);

            this.saveAndAddNewCommandValue = new RelayCommand(this.SaveAndAddNewCommandExecute, this.SaveAndAddNewCommandCanExecute);

            this.newTrackCommandValue = new RelayCommand(this.NewTrackCommandExecute, this.NewTrackCommandCanExecute);
            this.newMultiSwitchCommandValue = new RelayCommand(this.NewMultiSwitchCommandExecute, this.NewMultiSwitchCommandCanExecute);
            this.deleteSubItemCommandValue = new RelayCommand(this.DeleteSubItemCommandExecute, this.DeleteSubItemCommandCanExecute);

            // Use stored or default layout configuration, whichever is applicable.
            YardsDesignerLayoutConfiguration defaultConfiguration = new YardsDesignerLayoutConfiguration();
            this.configuration = this.conductor.Preferences.LoadLayoutConfiguration<YardsDesignerLayoutConfiguration>(defaultConfiguration.Name);
            if (this.configuration == null)
            {
                this.configuration = defaultConfiguration;
                this.conductor.Preferences.SaveLayoutConfiguration(this.configuration);
            }
            this.currentListSplitterPosition = new GridLength(this.configuration.ListSplitterPosition);
            this.currentErrorWindowSplitterPosition = new GridLength(this.configuration.ErrorWindowSplitterPosition);

            // Prepare dictionaries.
            this.printOptionsValue = EnumExtensions.GetDescriptionsDictionary<DwellTimeMethod>();
            this.printOrdersValue = EnumExtensions.GetDescriptionsDictionary<PrintListOrder>();

            this.multiSwitcherFunctionsValue = EnumExtensions.GetDescriptionsDictionary<MultiSwitcherFunction>();
            this.multiSwitcherOptionsValue = EnumExtensions.GetDescriptionsDictionary<MultiSwitcherOption>();

            this.citiesValue = new ObservableCollection<CityViewModel>();
            this.roadsValue = new ObservableCollection<RoadViewModel>();
            this.carTypesValue = new ObservableCollection<CarTypeViewModel>();
        }
        #endregion

        #region Public Methods
        public void updateRoadsTrackSelection(Road[] selectedRoads)
        {
            // Prevent updates.
            this.updatingRoadsSelection = true;

            // Clear any current selection.
            foreach (RoadViewModel road in this.Roads)
            {
                road.IsSelected = false;
                road.ResetChangeTracking();
                road.StartTrackingChanges();
            }

            // Apply any selection.
            foreach (Road road in selectedRoads)
                this.Roads.First(x => x.ToRoad() == road).IsSelected = true;

            // Re-enable updates.
            this.updatingRoadsSelection = false;
        }

        public void updateCartypesSelection(CarType[] selectedCartypes)
        {
            // Prevent updates.
            this.updatingCartypesSelection = true;

            // Clear any current selection.
            foreach (CarTypeViewModel cartype in this.CarTypes)
            {
                cartype.IsSelected = false;
                cartype.ResetChangeTracking();
                cartype.StartTrackingChanges();
            }

            // Apply any selection.
            foreach (CarType cartype in selectedCartypes)
                this.CarTypes.First(x => x.ToCarType() == cartype).IsSelected = true;

            // Re-enable updates.
            this.updatingCartypesSelection = false;
        }
        #endregion

        #region Private Methods
        private async Task performValidation()
        {
            Action performValidation = () =>
            {
                // Run validation.
                Debug.WriteLine("Performing validation.");
                List<DataValidationResult<YardViewModel>> validationResults = new List<DataValidationResult<YardViewModel>>();
                foreach (YardViewModel YardViewModel in this.ViewModels)
                {
                    List<DataValidationResult<YardViewModel>> results = this.validator.Validate(YardViewModel);
                    validationResults.AddRange(results);
                }

                // Clear any existing results.
                if (this.validationResultsValue != null)
                    this.validationResultsValue.Clear();
                else
                    this.validationResultsValue = new ObservableCollection<DataValidationResultViewModel<YardViewModel>>();

                // If there are results, then perform addition.
                if (validationResults.Count > 0)
                {
                    Debug.WriteLine("Adding validation results.");

                    // This MUST happen on the UI thread.
                    Action addResults = () =>
                    {
                        // Add current results.
                        foreach (DataValidationResult<YardViewModel> result in validationResults)
                            this.validationResultsValue.Add(new DataValidationResultViewModel<YardViewModel>(result));
                    };
                    App.Current.Dispatcher.Invoke(addResults);
                }

                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(YardsDesignerViewModel.ValidationResults));
            };
            await Task.Run(performValidation);
        }

        private bool ViewModelsViewFilter(object obj)
        {
            // By default, allow always.
            return true;
        }

        private void addNewYardHelper(Yard newYard)
        {
            // Create ViewModel and attach event handlers.
            YardViewModel newYardViewModel = new YardViewModel(this, newYard);
            newYardViewModel.ChangesMade += this.YardViewModel_ChangesMade;

            // Add to list.
            this.ViewModels.Add(newYardViewModel);

            // Select new yard.
            this.SelectedViewModel = newYardViewModel;

            // Mark designer as having changes.
            this.hasChanges = true;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        private async Task initializeAsync()
        {
            try
            {
                // Prepare status.
                this.StatusModel.SetStatus("Loading yards... please wait.");

                // Initialize context object.
                this.trackBossConnection = new TrackBossEntities(Conductor.ConnectionString);

                if (this.roadsValue != null)
                { 
                    foreach (RoadViewModel railroadViewModel in this.roadsValue)
                        railroadViewModel.PropertyChanged -= this.RoadViewModel_PropertyChanged;
                }

                if (this.carTypesValue != null)
                {
                    foreach (CarTypeViewModel carTypeViewModel in this.carTypesValue)
                        carTypeViewModel.PropertyChanged -= this.CarTypeViewModel_PropertyChanged;
                }

                //// Initialize all supporting lists.
                //Debug.WriteLine("Initializing supporting lists");
                await this.initializeSupportingLists(this.trackBossConnection);

                //// Attach event handlers for supporting lists.
                foreach (RoadViewModel roadViewModel in this.roadsValue)
                    roadViewModel.PropertyChanged += this.RoadViewModel_PropertyChanged;

                foreach (CarTypeViewModel carTypeViewModel in this.carTypesValue)
                    carTypeViewModel.PropertyChanged += this.CarTypeViewModel_PropertyChanged;

                // Initialize the list of yards.
                Debug.WriteLine("Initializing yards");
                await this.initializeYards(this.trackBossConnection);

                //// Load user lists dependent on rolling stock. This MUST BE
                //// done after the cars are loaded.
                //Debug.WriteLine("Loading user rolling stock lengths");
                //this.loadUserRollingStockLengths();

                SelectedFilter = YardFilterType.All;

                // Create list view collection.
                Debug.WriteLine(string.Format("ViewModels.Count: {0}", this.ViewModels.Count));
                this.viewModelsViewValue = new ListCollectionView(this.ViewModels)
                {
                    IsLiveSorting = true,
                    Filter = this.ViewModelsViewFilter,
                    //    CustomSort = new CityViewModelComparer(),
                };
                this.updateSort();

                this.viewModelsViewValue.MoveCurrentToFirst();

                // Finally, load validation rule set.
                this.StatusModel.SetStatus("Validating yards... please wait.");
                await this.initializeValidator();
                await this.performValidation();

                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(YardsDesignerViewModel.ViewModelsView));
                this.NotifyPropertyChanged(nameof(YardsDesignerViewModel.ValidationResults));
            }
            finally
            {
                // Always restore status to default.
                this.StatusModel.ClearStatus();
            }
        }

        private async Task initializeYards(TrackBossEntities connection)
        {
            // Fetch list of cars.
            List<Yard> yards = null;
            await Task.Run(() => yards = connection.Yards.ToList());

            // Add yards to list.
            if (yards != null)
            {
                foreach (Yard yard in yards)
                {
                    // Prepare new ViewModel.
                    YardViewModel newYardViewModel = new YardViewModel(this, yard);
                    newYardViewModel.ChangesMade += this.YardViewModel_ChangesMade;
                    this.ViewModels.Add(newYardViewModel);
                }
            }
        }

        private async Task initializeValidator()
        {
            Action initializeValidator = () =>
            {
                // Prepare validator.
                Debug.WriteLine("Initializing validator.");
                this.validator = new Validator<YardViewModel>(FileUtilities.GetFullpath(SpecialFileName.YardValidationRuleSet));
            };
            await Task.Run(initializeValidator);
        }

        private async Task initializeSupportingLists(TrackBossEntities connection)
        {
            // Fetch locations.
            List<Location> locations = null;
            Debug.WriteLine("Initializing locations");
            await Task.Run(() => locations = connection.Locations.ToList());

        
            List<City> cities = null;
            await Task.Run(() => cities = connection.Cities.ToList());

            this.citiesValue = new ObservableCollection<CityViewModel>();
            foreach (City city in cities)
            {
                CityViewModel cityViewModel = new CityViewModel(city);
                cityViewModel.ChangesMade += YardViewModel_ChangesMade;
                this.citiesValue.Add(cityViewModel);
            }


            List<Road> roads = null;
            await Task.Run(() => roads = connection.Roads.ToList());

            this.roadsValue = new ObservableCollection<RoadViewModel>();
            foreach (Road road in roads)
            {
                RoadViewModel roadViewModel = new RoadViewModel(road);
                this.roadsValue.Add(roadViewModel);
            }


            List<CarType> carTypes = null;
            await Task.Run(() => carTypes = connection.CarTypes.ToList());

            this.carTypesValue = new ObservableCollection<CarTypeViewModel>();
            foreach (CarType carType in carTypes)
            {
                CarTypeViewModel carTypeViewModel = new CarTypeViewModel(carType);
                this.carTypesValue.Add(carTypeViewModel);
            }


            await Task.Run(() => trains = connection.Trains.ToList());

            List<Printer> printers = null;
            Debug.WriteLine("Initializing printer");
            await Task.Run(() => printers = connection.Printers.ToList());

            this.printersValue = new ObservableCollection<PrinterViewModel>();
            foreach (Printer printer in printers)
                this.printersValue.Add(new PrinterViewModel(printer));
            this.printersViewValue = new ListCollectionView(this.printersValue);
        }

        private void RoadViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.updatingRoadsSelection)
                return;

            // Update selection status if there is a car currently selected.
            if (e.PropertyName == nameof(RoadViewModel.IsSelected) && this.SelectedViewModel != null)
                this.SelectedViewModel.UpdateRoadStatusCommand.Execute(sender);
        }

        private void CarTypeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.updatingCartypesSelection)
                return;

            // Update selection status if there is a car currently selected.
            if (e.PropertyName == nameof(CarTypeViewModel.IsSelected) && this.SelectedViewModel != null)
                this.SelectedViewModel.UpdateCartypeStatusCommand.Execute(sender);
        }
        #endregion

        #region Command Handlers
        public void invalidateAllCommands_Refresh()
        {
            invalidateAllCommands();
        }

        protected override void invalidateAllCommands()
        {
            // Perform base invalidation of commands.
            base.invalidateAllCommands();

            // Perform invalidate on commands this object defines.
            this.sortCommandValue.InvalidateCanExecuteChanged();
            this.reverseSortCommandValue.InvalidateCanExecuteChanged();
            this.initializeCommandValue.RaiseCanExecuteChanged();
            this.newTrackCommandValue.InvalidateCanExecuteChanged();
            this.newMultiSwitchCommandValue.InvalidateCanExecuteChanged();
            this.deleteSubItemCommandValue.InvalidateCanExecuteChanged();
            this.saveAndAddNewCommandValue.InvalidateCanExecuteChanged();
            //this.duplicateCommandValue.InvalidateCanExecuteChanged();
            //this.showLocationSelectionDialogCommandValue.InvalidateCanExecuteChanged();
        }
        protected override void selectionChangeHelper()
        {
            // Run base processes.
            base.selectionChangeHelper();

            if (this.SelectedViewModel != null)
            {
                YardItem yardItem;
                SelectedViewModel.selectedIndex = 0;

                SelectedViewModel.YardItems.Clear();

                if (SelectedFilter == YardFilterType.All || SelectedFilter == YardFilterType.Characteristics)
                {
                    yardItem = new YardItem();
                    yardItem.DisplayText = "General";
                    yardItem.iconType = IconType.General;
                    SelectedViewModel.YardItems.Add(yardItem);
                }

                Yard yard = this.SelectedViewModel.ToYard();
                if (SelectedFilter == YardFilterType.All || SelectedFilter == YardFilterType.Tracks
                    || SelectedFilter == YardFilterType.None)
                {
                    foreach (var item in yard.YardTracks)
                    {
                        yardItem = new YardItem();
                        yardItem.DisplayText = item.ToString();
                        yardItem.iconType = IconType.Track;
                        yardItem.YardTrack = item;

                        bool bAdd = false;
                        if (SelectedFilter == YardFilterType.None)
                        {
                            if (this.SelTrackTypeIdx == 0 && item.Track.TrackTypeName.Equals("Arrival"))// Arrival
                                bAdd = true;
                            else if (this.SelTrackTypeIdx == 1 && item.Track.TrackTypeName.Equals("Block"))// Block
                                bAdd = true;
                            else if (this.SelTrackTypeIdx == 2 && item.Track.TrackTypeName.Equals("Caboose"))// Caboose
                                bAdd = true;
                            else if (this.SelTrackTypeIdx == 3 && item.Track.TrackTypeName.Equals("Departure"))// Depature
                                bAdd = true;
                            else if (this.SelTrackTypeIdx == 4 && item.Track.TrackTypeName.Equals("Engine Service"))// Engine Service
                                bAdd = true;
                            else if (this.SelTrackTypeIdx == 5 && item.Track.TrackTypeName.Equals("Interchange"))// Interchange
                                bAdd = true;
                            else if (this.SelTrackTypeIdx == 6 && item.Track.TrackTypeName.Equals("Passenger"))// Passenger
                                bAdd = true;
                            else if (this.SelTrackTypeIdx == 7 && item.Track.TrackTypeName.Equals("Staging"))// Staging
                                bAdd = true;
                            else if (this.SelTrackTypeIdx == 8 && item.Track.TrackTypeName.Equals("Storage"))// Storage
                                bAdd = true;
                            else if (this.SelTrackTypeIdx == 9 && item.Track.TrackTypeName.Equals("Thru"))// Thru
                                bAdd = true;
                            else if (this.SelTrackTypeIdx == 10 && item.Track.TrackTypeName.Equals("Train Select"))// Train Assigned
                                bAdd = true;
                        }
                        else
                            bAdd = true;

                        if(bAdd)
                            SelectedViewModel.YardItems.Add(yardItem);
                    }
                }

                if (SelectedFilter == YardFilterType.All || SelectedFilter == YardFilterType.MultiSwitchers)
                {
                    foreach (var multiSwitcher in yard.MultiSwitchers)
                    {
                        yardItem = new YardItem();
                        yardItem.DisplayText = multiSwitcher.Switcher.Nickname;
                        yardItem.iconType = IconType.MultiSwitcher;
                        yardItem.MultiSwitcher = multiSwitcher;

                        SelectedViewModel.YardItems.Add(yardItem);
                    }
                }

                if (SelectedFilter == YardFilterType.All)
                {
                    yardItem = new YardItem();
                    yardItem.DisplayText = "Print Scheme";
                    yardItem.iconType = IconType.PrintScheme;
                    SelectedViewModel.YardItems.Add(yardItem);
                }
            }
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
                this.SelectedViewModel = (YardViewModel)this.ViewModelsView.CurrentItem;
        }

        private bool NewTrackCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private void NewTrackCommandExecute(object commandParam)
        {
            // Make sure this is allowed.
            if (!this.NewTrackCommand.CanExecute(commandParam))
                return;

            // Create new track.
            Track newTrack = null;

            switch (commandParam.ToString())
            {
                case "ArrivalTrack":
                    newTrack = Track.Create(TrackType.Arrival);
                    break;
                case "BlockTrack":
                    newTrack = Track.Create(TrackType.Block);
                    break;
                case "CabooseTrack":
                    newTrack = Track.Create(TrackType.Caboose);
                    break;
                case "DepatureTrack":
                    newTrack = Track.Create(TrackType.Departure);
                    break;
                case "EngineServiceTrack":
                    newTrack = Track.Create(TrackType.EngineService);
                    break;
                case "InterchangeTrack":
                    newTrack = Track.Create(TrackType.Interchange);
                    break;
                case "PassengerTrack":
                    newTrack = Track.Create(TrackType.Passenger);
                    break;
                case "StagingTrack":
                    newTrack = Track.Create(TrackType.Staging);
                    break;
                case "StorageTrack":
                    newTrack = Track.Create(TrackType.Storage);
                    break;
                case "TrainAssignedTrack":
                    newTrack = Track.Create(TrackType.TrainSelect);
                    break;
            }

            addNewTrackHelper(newTrack);

            // Queue validation.
            this.validationTimer.Stop();
            this.validationTimer.Start();
        }

        private void addNewTrackHelper(Track newTrack)
        {
            YardTrack newYardTrack = YardTrack.Create(SelectedViewModel.YARD, newTrack);
            SelectedViewModel.YARD.YardTracks.Add(newYardTrack);

            YardItem yardItem = new YardItem();
            yardItem.DisplayText = newYardTrack.ToString();
            yardItem.iconType = IconType.Track;
            yardItem.YardTrack = newYardTrack;
            SelectedViewModel.YardItems.Add(yardItem);

            // Select new item.
            this.SelectedViewModel.selectedIndex = SelectedViewModel.YardItems.Count - 1;

            // Mark designer as having changes.
            this.hasChanges = true;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        private bool NewMultiSwitchCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private void addNewMultiSwitcherHelper()
        {
            MultiSwitcher newMultiSwitcher = MultiSwitcher.Create(SelectedViewModel.YARD);

            var enumerator = PrintersValue.GetEnumerator();
            enumerator.MoveNext(); // sets it to the first element
            var firstElement = enumerator.Current;
            newMultiSwitcher.Switcher.AliasPrinter.Printer = (firstElement as PrinterViewModel).ToPrinter();


            SelectedViewModel.YARD.MultiSwitchers.Add(newMultiSwitcher);

            YardItem yardItem = new YardItem();
            yardItem.DisplayText = newMultiSwitcher.ToString();
            yardItem.iconType = IconType.MultiSwitcher;
            yardItem.MultiSwitcher = newMultiSwitcher;
            SelectedViewModel.YardItems.Add(yardItem);


            // Select new item.
            this.SelectedViewModel.selectedIndex = SelectedViewModel.YardItems.Count - 1;

            // Mark designer as having changes.
            this.hasChanges = true;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        private void NewMultiSwitchCommandExecute(object commandParam)
        {
            // Make sure this is allowed.
            if (!this.NewTrackCommand.CanExecute(commandParam))
                return;

            addNewMultiSwitcherHelper();

            // Queue validation.
            this.validationTimer.Stop();
            this.validationTimer.Start();
        }

        private bool DeleteSubItemCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            if (this.SelectedViewModel == null || this.SelectedViewModel.YardItems.Count <= 0)
                return false;

            return this.SelectedViewModel.EnableDeleteSubItem;
        }

        private void DeleteSubItemCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.DeleteSubItemCommand.CanExecute(obj))
                return;

            if(SelectedViewModel.YardItems[SelectedViewModel.selectedIndex].iconType == IconType.MultiSwitcher)
                SelectedViewModel.YARD.MultiSwitchers.Remove(SelectedViewModel.YardItems[SelectedViewModel.selectedIndex].MultiSwitcher);
            else if (SelectedViewModel.YardItems[SelectedViewModel.selectedIndex].iconType == IconType.Track)
                SelectedViewModel.YARD.YardTracks.Remove(SelectedViewModel.YardItems[SelectedViewModel.selectedIndex].YardTrack);

            SelectedViewModel.YardItems.RemoveAt(SelectedViewModel.selectedIndex);
            

            // Select new item.
            this.SelectedViewModel.selectedIndex = 0;

            // Mark designer as having changes.
            this.hasChanges = true;

            // Invalidate commands.
            this.invalidateAllCommands();

            // Queue validation.
            this.validationTimer.Stop();
            this.validationTimer.Start();
        }

        private void updateConfiguration()
        {
            this.configuration.SortType = this.selectedSort;
            this.configuration.SortDirection = this.currentSortDirection;
        }

        private bool SortCommandCanExecute(CitySortType obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private void SortCommandExecute(CitySortType obj)
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
            YardViewModel lastSelectedViewModel = this.SelectedViewModel;

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
            if (this.SelectedSort == CitySortType.Name)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("DisplayText", this.currentSortDirection));
            else if (this.SelectedSort == CitySortType.CreatedDate)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("ID", this.currentSortDirection));
            else
                throw new NotSupportedException("The specified reverse sort is unsupported.");
        }

        protected override bool SaveCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy && this.hasChanges;
        }

        protected override bool DeleteCommandCanExecute(object obj)
        {
            return !this.StatusModel.IsBusy && this.SelectedViewModel != null;
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

            // Add new city.
            this.NewCommand.Execute(obj);
        }

        private bool FilterCommandCanExecute(YardFilterType obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private void FilterCommandExecute(YardFilterType obj)
        {
            // Make sure this is allowed.
            if (!this.SortCommand.CanExecute(obj))
                return;
            SelectedFilter = obj;

            SelTrackTypeIdx = -1;
        }

        protected override void newCommandExecuteHelper(object obj)
        {
            // Create new yard.
            Yard newYard = Yard.Create();
            newYard.Site.Location.Sites.Add(newYard.Site);

            // Add new car to list.
            this.addNewYardHelper(newYard);

            // Queue validation.
            this.validationTimer.Stop();
            this.validationTimer.Start();
        }

        private void performViewModelCleanUp()
        {
            // Dispose of existing data set.
            foreach (YardViewModel yardViewModel in this.ViewModels)
            {
                yardViewModel.ChangesMade -= this.YardViewModel_ChangesMade;
                yardViewModel.Dispose();
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
            // Get selected yard.
            YardViewModel selectedYardViewModel = this.SelectedViewModel;
            Yard selectedYard = SelectedViewModel.ToYard();

            // Verify the user wants to delete this yard.
            ShowMessageBoxMessage message = new ShowMessageBoxMessage();
            message.Message = string.Format(
                                "This will delete the currently selected car:\n\n{0}\n\nAre you sure you want to do this?", selectedYard);
            message.Button = MessageBoxButton.YesNo;
            message.Icon = MessageBoxImage.Exclamation;
            message.Title = this.conductor.Name;
            Messenger.Default.Send<ShowMessageBoxMessage>(message);

            // Check result.
            if (message.Result == MessageBoxResult.Yes)
            {
                // Remove selection and remove ViewModel from list.
                this.SelectedViewModel = null;
                this.ViewModels.Remove(selectedYardViewModel);

                // Disconnect change event. This event should not fire
                // for deleted ViewModels.
                selectedYardViewModel.ChangesMade -= this.YardViewModel_ChangesMade;

                // Remove validation results pertaining to this car, if any.
                for (int i = this.validationResultsValue.Count - 1; i > -1; i--)
                {
                    DataValidationResultViewModel<YardViewModel> result = this.validationResultsValue[i];
                    if (result.Source == selectedYardViewModel)
                        this.validationResultsValue.RemoveAt(i);
                }

                // Remove history item, if applicable.
                //this.conductor.HistoryManager.RemoveHistoryItem(selectedCrewViewModel.UniqueId);

                // Dispose of removed ViewModel.
                selectedYardViewModel.Dispose();

                // Get yard being removed.
                Yard yardToRemove = selectedYardViewModel.ToYard();
                Site siteToRemove = selectedYardViewModel.ToYard().Site;
                Location locationToRemove = selectedYardViewModel.ToYard().Site.Location;

                // Perform clean-up on city.
                //await carToRemove.RollingStock.Photo.Clear();

                // Remove car from data context. If this is a new crew, there
                // will be no currently assigned ID and nothing else to do.
                if (yardToRemove.ID != 0)
                {
                    if (this.trackBossConnection.Yards.Remove(yardToRemove) != null)
                    {
                        this.trackBossConnection.Sites.Remove(siteToRemove);
                        this.trackBossConnection.Locations.Remove(locationToRemove);
                    }
                }

                // Set changes state.
                this.hasChanges = true;

                // Invalidate commands.
                this.invalidateAllCommands();
            }
        }

        private void SelectYardById(long id)
        {
            foreach (YardViewModel yardViewModel in this.ViewModels)
            {
                if (yardViewModel.ID == id)
                {
                    this.SelectedViewModel = yardViewModel;
                    break;
                }
            }
        }

        private void saveLocation(TrackBossEntities connection)
        {
            // New locations will have changes but will also have an ID of zero.
            IEnumerable<Location> modifiedLocations = this.ViewModels.Where(x => x.HasChanges).Select(x => x.ToYard().Site.Location);
            IEnumerable<Location> newLocations = modifiedLocations.Where(x => x.ID == 0);
            modifiedLocations = modifiedLocations.Except(newLocations);

            // Add new owners.
            foreach (Location newLocation in newLocations)
                connection.Locations.Add(newLocation);
        }
        private void saveSite(TrackBossEntities connection)
        {
            // New sites will have changes but will also have an ID of zero.
            IEnumerable<Site> modifiedSites = this.ViewModels.Where(x => x.HasChanges).Select(x => x.ToYard().Site);
            IEnumerable<Site> newSites = modifiedSites.Where(x => x.ID == 0);
            modifiedSites = modifiedSites.Except(newSites);

            // Add new owners.
            foreach (Site newSite in newSites)
                connection.Sites.Add(newSite);
        }

        private void saveYards(TrackBossEntities connection)
        {
            // New yards will have changes but will also have an ID of zero.
            IEnumerable<YardViewModel> newViewModels = this.ViewModels.Where(x => x.ID == 0);
            IEnumerable<Yard> allYards = this.ViewModels.Select(x => x.ToYard());
            IEnumerable<Yard> newYards = newViewModels.Select(x => x.ToYard());

            // Add new owners.
            foreach (Yard newYard in newYards)
                connection.Yards.Add(newYard);
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
                    //this.saveLocation(this.trackBossConnection);
                    //this.saveSite(this.trackBossConnection);
                    this.saveYards(this.trackBossConnection);

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
            long lastSelectedYardId = -1;
            if (this.SelectedViewModel != null)
                lastSelectedYardId = this.SelectedViewModel.ToYard().ID;

            // Perform cleanup.
            this.performViewModelCleanUp();

            // Destroy connection object.
            this.trackBossConnection.Dispose();
            this.trackBossConnection = null;

            // Reinitialize the data set.
            if (this.InitializeCommand.CanExecute(null))
                await this.initializeAsync();

            // Reselect last selected car, if applicable.
            if (lastSelectedYardId != -1)
                SelectYardById(lastSelectedYardId);

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

        private void YardViewModel_ChangesMade(object sender, EventArgs e)
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
        public ObservableCollection<DataValidationResultViewModel<YardViewModel>> ValidationResults
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

        public CitySortType SelectedSort
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

        public YardFilterType SelectedFilter
        {
            get { return this.selectedFilter; }
            private set
            {
                if (this.selectedFilter != value)
                {
                    this.selectedFilter = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public int SelTrackTypeIdx
        {
            get { return this.selTrackTypeIdx; }
            set
            {
                this.selTrackTypeIdx = value;
                this.NotifyPropertyChanged();

                if(value != -1)
                    SelectedFilter = YardFilterType.None;

                // Apply filter.
                this.viewModelsViewValue.Refresh();
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

        public RelayCommand<CitySortType> SortCommand
        {
            get { return this.sortCommandValue; }
        }

        public ICommand ReverseSortCommand
        {
            get { return this.reverseSortCommandValue; }
        }

        public RelayCommand<YardFilterType> FilterCommand
        {
            get { return this.filterCommandValue; }
        }

        public ICommand SaveAndAddNewCommand
        {
            get { return this.saveAndAddNewCommandValue; }
        }

        public ICommand NewTrackCommand
        {
            get { return this.newTrackCommandValue; }
        }

        public ICommand NewMultiSwitchCommand
        {
            get { return this.newMultiSwitchCommandValue; }
        }

        public ICommand DeleteSubItemCommand
        {
            get { return this.deleteSubItemCommandValue; }
        }

        public ObservableCollection<CityViewModel> Cities
        {
            get { return this.citiesValue; }
        }

        public ObservableCollection<RoadViewModel> Roads
        {
            get { return this.roadsValue; }
        }

        public ObservableCollection<CarTypeViewModel> CarTypes
        {
            get { return this.carTypesValue; }
        }


        public List<Train> Trains
        {
            get { return this.trains; }
        }

        public ICollectionView PrintersValue
        {
            get
            {
                return this.printersViewValue;
            }
        }

        public SortedDictionary<DwellTimeMethod, string> PrintOptions
        {
            get { return this.printOptionsValue; }
        }

        public SortedDictionary<PrintListOrder, string> PrintOrders
        {
            get { return this.printOrdersValue; }
        }

        public SortedDictionary<MultiSwitcherFunction, string> MultiSwitcherFunctions
        {
            get { return this.multiSwitcherFunctionsValue; }
        }

        public SortedDictionary<MultiSwitcherOption, string> MultiSwitcherOptions
        {
            get { return this.multiSwitcherOptionsValue; }
        }

        
        #endregion
    }
}
