using GalaSoft.MvvmLight.Messaging;
using Syncfusion.UI.Xaml.Diagram;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
using TrackBoss.Configuration.History;
using TrackBoss.Configuration.IO;
using TrackBoss.Configuration.Preferences.Layout;
using TrackBoss.Data;
using TrackBoss.Data.Enumerations;
using TrackBoss.Data.Validation;
using TrackBoss.Model.Enumerations;
using TrackBoss.Mvvm.Shared.Commands;
using TrackBoss.Mvvm.Shared.Messages;
using TrackBoss.Mvvm.Validation.Messages;
using TrackBoss.Mvvm.Validation.ViewModel;
using TrackBoss.Mvvm.Validation.ViewModel.Rules;
using TrackBoss.Shared.Enumerations;
using TrackBoss.Shared.Events;
using TrackBoss.Shared.Extensions;
using TrackBoss.ViewModel.Cars;
using TrackBoss.ViewModel.Crew;
using TrackBoss.ViewModel.RollingStocks;
using TrackBoss.ViewModel.Shared;
using TrackBoss.ViewModel.Shared.Messages;
using TrackBoss.ViewModel.Yards;
using TrackBoss.Windsor;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cities
{
    public class CitiesDesignerViewModel : GenericListBasedViewModel<CityViewModel>, IInitializableViewModel
    {
        #region Fields

        private readonly RelayCommand saveAndAddNewCommandValue;

        private readonly RelayCommand newSpurCommandValue;
        private readonly RelayCommand newIndustryCommandValue;
        private readonly RelayCommand deleteSubItemCommandValue;

        private readonly RelayCommand<CitySortType> sortCommandValue;

        private readonly RelayCommand reverseSortCommandValue;

        private readonly AsyncCommand initializeCommandValue;


        private readonly RelayCommand<CityFilterType> filterCommandValue;


        private readonly RelayCommand addGroupCommandValue;
        private readonly RelayCommand addSubdivisionCommandValue;


        private ObservableCollection<DataValidationResultViewModel<CityViewModel>> validationResultsValue;

        private readonly DispatcherTimer validationTimer;

        private Validator<CityViewModel> validator;

        private Conductor conductor;

        private List<CityViewModel> viewModelsHistory;

        private CitySortType selectedSort;
        private ListSortDirection currentSortDirection;

        private CityFilterType selectedFilter;

        private GridLength currentListSplitterPosition;

        private GridLength currentErrorWindowSplitterPosition;

        private TrackBossEntities trackBossConnection;

        private CitiesDesignerLayoutConfiguration configuration;
        
        private ListCollectionView viewModelsViewValue;

        private ObservableCollection<LocationViewModel> locationsValue;

        private ObservableCollection<CityGroupViewModel> cityGroupsValue;

        private ObservableCollection<SubdivisionViewModel> subDivisionsValue;

        private ObservableCollection<RailroadViewModel> railRoadsValue;

        private ObservableCollection<RailroadViewModel> railRoadsTrackValue;

        private ObservableCollection<CarTypeViewModel> carTypesValue;

        private ObservableCollection<CarGroupViewModel> carGroupsValue;

        private ObservableCollection<CommodityViewModel> commoditiesValue;

        private ObservableCollection<SpurViewModel> spursValue;

        private ObservableCollection<RoadViewModel> roadsValue;

        private ListCollectionView printersViewValue;
        private ObservableCollection<PrinterViewModel> printersValue;

        private readonly SortedDictionary<DwellTimeMethod, string> printOptionsValue;
        private readonly SortedDictionary<PrintListOrder, string> printOrdersValue;

        private readonly ObservableCollection<Difficulty> kammFactorDifficulties;


        private CityGroupViewModel newGroupValue;
        private SubdivisionViewModel newSubdivisionValue;

        private bool updatingSpursSelection;
        private bool updatingRailroadsSelection;
        private bool updatingRailroadsTrackSelection;
        private bool updatingCartypesSelection;
        private bool updatingCommoditiesSelection;
        private bool updatingRoadsSelection;


        private bool hasChanges;


        #endregion

        #region Constructor(s)

        /// <summary>
        /// Default constructor. Initializes fields and prepares this object
        /// for use.
        /// </summary>
        public CitiesDesignerViewModel()
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
            this.selectedSort = CitySortType.Name;
            this.currentSortDirection = ListSortDirection.Ascending;
            this.locationsValue = new ObservableCollection<LocationViewModel>();
            this.cityGroupsValue = new ObservableCollection<CityGroupViewModel>();
            this.subDivisionsValue = new ObservableCollection<SubdivisionViewModel>();

            this.commoditiesValue = new ObservableCollection<CommodityViewModel>();
            this.roadsValue = new ObservableCollection<RoadViewModel>();

            // Hookup command handlers.
            this.sortCommandValue = new RelayCommand<CitySortType>(this.SortCommandExecute, this.SortCommandCanExecute);
            this.reverseSortCommandValue = new RelayCommand(this.ReverseSortCommandExecute, this.ReverseSortCommandCanExecute);
            this.initializeCommandValue = new AsyncCommand(this.InitializeCommandExecute, this.InitializeCommandCanExecute);
            this.saveAndAddNewCommandValue = new RelayCommand(this.SaveAndAddNewCommandExecute, this.SaveAndAddNewCommandCanExecute);

            this.newSpurCommandValue = new RelayCommand(this.NewSpurCommandExecute, this.NewSpurCommandCanExecute);
            this.newIndustryCommandValue = new RelayCommand(this.NewIndustryCommandExecute, this.NewIndustryCommandCanExecute);
            this.deleteSubItemCommandValue = new RelayCommand(this.DeleteSubItemCommandExecute, this.DeleteSubItemCommandCanExecute);
            //this.addCarTypeCommandValue = new RelayCommand(this.AddCarTypeCommandExecute, this.AddCarTypeCommandCanExecute);
            //this.addRoadCommandValue = new RelayCommand(this.AddRoadCommandExecute, this.AddRoadCommandCanExecute);
            //this.addOwnerCommandValue = new RelayCommand(this.AddOwnerCommandExecute, this.AddOwnerCommandCanExecute);

            this.filterCommandValue = new RelayCommand<CityFilterType>(this.FilterCommandExecute, this.FilterCommandCanExecute);


            this.addGroupCommandValue = new RelayCommand(this.AddGroupCommandExecute, this.AddGroupCommandCanExecute);
            this.addSubdivisionCommandValue = new RelayCommand(this.AddSubdivisionCommandExecute, this.AddSubdivisionCommandCanExecute);

            kammFactorDifficulties = new ObservableCollection<Difficulty>();
            // Load lists.
            //this.colors = new ObservableCollection<ColorItemViewModel>();
            //this.loadDefaultColors();
            //this.loadDefaultLengths();

            // Use stored or default layout configuration, whichever is applicable.
            CitiesDesignerLayoutConfiguration defaultConfiguration = new CitiesDesignerLayoutConfiguration();
            this.configuration = this.conductor.Preferences.LoadLayoutConfiguration<CitiesDesignerLayoutConfiguration>(defaultConfiguration.Name);
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

            // Prepare dictionaries.
            this.printOptionsValue = EnumExtensions.GetDescriptionsDictionary<DwellTimeMethod>();
            this.printOrdersValue = EnumExtensions.GetDescriptionsDictionary<PrintListOrder>();
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            // If disposing perform clean-up.
            if (disposing)
            {
                // Clean-up ViewModels.
                //this.performViewModelCleanUp();

                // Unhook events.
                //this.conductor.Preferences.PreferencesChanged -= this.Preferences_PreferencesChanged;

                // Unhook message handlers.
                Messenger.Default.Unregister(this);
            }

            // Call base dispose processes last.
            base.Dispose(disposing);
        }

        #endregion

        #region public
        public void updateSpursSelection(Spur[] selectedSpurs)
        {
            // Prevent updates.
            this.updatingSpursSelection = true;

            // Clear any current selection.
            foreach (SpurViewModel spur in this.Spurs)
            {
                spur.IsSelected = false;
                spur.ResetChangeTracking();
                spur.StartTrackingChanges();
            }

            // Apply any selection.
            foreach (Spur spur in selectedSpurs)
                this.Spurs.First(x => x.CurSpur == spur).IsSelected = true;

            // Re-enable updates.
            this.updatingSpursSelection = false;
        }

        public void updateRailroadsSelection(Railroad[] selectedRailroads)
        {
            // Prevent updates.
            this.updatingRailroadsSelection = true;

            // Clear any current selection.
            foreach (RailroadViewModel railroad in this.Railroads)
            {
                railroad.IsSelected = false;
                railroad.ResetChangeTracking();
                railroad.StartTrackingChanges();
            }

            // Apply any selection.
            foreach (Railroad railroad in selectedRailroads)
                this.Railroads.First(x => x.ToRailroad() == railroad).IsSelected = true;

            // Re-enable updates.
            this.updatingRailroadsSelection = false;
        }

        public void updateRailroadsTrackSelection(Railroad[] selectedRailroads)
        {
            // Prevent updates.
            this.updatingRailroadsTrackSelection = true;

            // Clear any current selection.
            foreach (RailroadViewModel railroad in this.RailroadsTrack)
            {
                railroad.IsSelected = false;
                railroad.ResetChangeTracking();
                railroad.StartTrackingChanges();
            }

            // Apply any selection.
            foreach (Railroad railroad in selectedRailroads)
                this.RailroadsTrack.First(x => x.ToRailroad() == railroad).IsSelected = true;

            // Re-enable updates.
            this.updatingRailroadsTrackSelection = false;
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

        public void updateCommoditiesSelection(Commodity[] selectedCommodities)
        {
            // Prevent updates.
            this.updatingCommoditiesSelection = true;

            // Clear any current selection.
            foreach (CommodityViewModel commodity in this.Commodities)
            {
                commodity.IsSelected = false;
                commodity.ResetChangeTracking();
                commodity.StartTrackingChanges();
            }

            // Apply any selection.
            foreach (Commodity commodity in selectedCommodities)
                this.Commodities.First(x => x.ToCommodity() == commodity).IsSelected = true;

            // Re-enable updates.
            this.updatingCommoditiesSelection = false;
        }

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
        
        #endregion

        #region Private Methods

        private void addNewCityHelper(City newCity)
        {
            // Create ViewModel and attach event handlers.
            CityViewModel newCityViewModel = new CityViewModel(this, newCity);
            newCityViewModel.ChangesMade += this.CityViewModel_ChangesMade;

            // Add to list.
            this.ViewModels.Add(newCityViewModel);

            // Select new car.
            this.SelectedViewModel = newCityViewModel;

            // Mark designer as having changes.
            this.hasChanges = true;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        private void warnIfHasValidationErrors()
        {
            // Check and see if there are validation errors.
            int count = this.validationResultsValue.Count(x => x.Entity is DataValidationRuleViewModel);
            if (count > 0)
            {
                // Create message.
                ShowMessageBoxMessage message = new
                    ShowMessageBoxMessage()
                    {
                        Title = this.conductor.Name,
                        Icon = MessageBoxImage.Warning,
                        Button = MessageBoxButton.OK,
                        Message = "WARNING: There are validation errors.\n\nAny city with validation errors will NOT be used during the build process."
                    };

                // Show message.
                Messenger.Default.Send<ShowMessageBoxMessage>(message);
            }
        }

        private async Task performValidation()
        {
            Action performValidation = () =>
            {
                // Run validation.
                Debug.WriteLine("Performing validation.");
                List<DataValidationResult<CityViewModel>> validationResults = new List<DataValidationResult<CityViewModel>>();
                foreach (CityViewModel CityViewModel in this.ViewModels)
                {
                    List<DataValidationResult<CityViewModel>> results = this.validator.Validate(CityViewModel);
                    validationResults.AddRange(results);
                }

                // Clear any existing results.
                if (this.validationResultsValue != null)
                    this.validationResultsValue.Clear();
                else
                    this.validationResultsValue = new ObservableCollection<DataValidationResultViewModel<CityViewModel>>();

                // If there are results, then perform addition.
                if (validationResults.Count > 0)
                {
                    Debug.WriteLine("Adding validation results.");

                    // This MUST happen on the UI thread.
                    Action addResults = () =>
                    {
                        // Add current results.
                        foreach (DataValidationResult<CityViewModel> result in validationResults)
                            this.validationResultsValue.Add(new DataValidationResultViewModel<CityViewModel>(result));
                    };
                    App.Current.Dispatcher.Invoke(addResults);
                }

                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(CitiesDesignerViewModel.ValidationResults));
            };
            await Task.Run(performValidation);
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
                this.StatusModel.SetStatus("Loading cities... please wait.");

                // Initialize context object.
                this.trackBossConnection = new TrackBossEntities(Conductor.ConnectionString);


                if (this.spursValue != null)
                {
                    foreach (SpurViewModel spurViewModel in this.spursValue)
                        spurViewModel.PropertyChanged -= this.SpurViewModel_PropertyChanged;
                }

                if (this.railRoadsValue != null)
                {
                    foreach (RailroadViewModel railroadViewModel in this.railRoadsValue)
                        railroadViewModel.PropertyChanged -= this.RailRoadViewModel_PropertyChanged;
                }

                if (this.railRoadsTrackValue != null)
                {
                    foreach (RailroadViewModel railroadViewModel in this.railRoadsTrackValue)
                        railroadViewModel.PropertyChanged -= this.RailRoadTrackViewModel_PropertyChanged;
                }
                
                if (this.carTypesValue != null)
                {
                    foreach (CarTypeViewModel carTypeViewModel in this.carTypesValue)
                        carTypeViewModel.PropertyChanged -= this.CarTypeViewModel_PropertyChanged;
                }

                if (this.commoditiesValue != null)
                {
                    foreach (CommodityViewModel comodityViewModel in this.commoditiesValue)
                        comodityViewModel.PropertyChanged -= this.CommodityViewModel_PropertyChanged;
                }

                if (this.roadsValue != null)
                {
                    foreach (RoadViewModel roadViewModel in this.roadsValue)
                        roadViewModel.PropertyChanged -= this.RoadViewModel_PropertyChanged;
                }
                


                //// Initialize all supporting lists.
                //Debug.WriteLine("Initializing supporting lists");
                await this.initializeSupportingLists(this.trackBossConnection);

                //// Attach event handlers for supporting lists.
                

                foreach (SpurViewModel spurViewModel in this.spursValue)
                    spurViewModel.PropertyChanged += this.SpurViewModel_PropertyChanged;

                foreach (RailroadViewModel railroadViewModel in this.railRoadsValue)
                    railroadViewModel.PropertyChanged += this.RailRoadViewModel_PropertyChanged;

                foreach (RailroadViewModel railroadViewModel in this.railRoadsTrackValue)
                    railroadViewModel.PropertyChanged += this.RailRoadTrackViewModel_PropertyChanged;
                

                foreach (CarTypeViewModel carTypeViewModel in this.carTypesValue)
                    carTypeViewModel.PropertyChanged += this.CarTypeViewModel_PropertyChanged;

                foreach (CommodityViewModel comodityViewModel in this.commoditiesValue)
                    comodityViewModel.PropertyChanged += this.CommodityViewModel_PropertyChanged;

                foreach (RoadViewModel roadViewModel in this.roadsValue)
                    roadViewModel.PropertyChanged += this.RoadViewModel_PropertyChanged;

                // Initialize the list of cities.
                Debug.WriteLine("Initializing cities");
                await this.initializeCities(this.trackBossConnection);

                //// Load user lists dependent on rolling stock. This MUST BE
                //// done after the cars are loaded.
                //Debug.WriteLine("Loading user rolling stock lengths");
                //this.loadUserRollingStockLengths();

                SelectedFilter = CityFilterType.All;

                // Create list view collection.
                Debug.WriteLine(string.Format("ViewModels.Count: {0}", this.ViewModels.Count));
                this.viewModelsViewValue = new ListCollectionView(this.ViewModels)
                {
                    IsLiveSorting = true,
                //    CustomSort = new CityViewModelComparer(),
                };
                this.viewModelsViewValue.MoveCurrentToFirst();

                // Finally, load validation rule set.
                this.StatusModel.SetStatus("Validating cities... please wait.");
                await this.initializeValidator();
                await this.performValidation();

                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(CitiesDesignerViewModel.ViewModelsView));
                this.NotifyPropertyChanged(nameof(CitiesDesignerViewModel.ValidationResults));
            }
            catch(Exception ex)
            {
                int i = 0;
                i++;
            }
            finally
            {
                // Always restore status to default.
                this.StatusModel.ClearStatus();
            }
        }

        private async Task initializeSupportingLists(TrackBossEntities connection)
        {
            // Fetch locations.
            List<Location> locations = null;
            Debug.WriteLine("Initializing locations");
            await Task.Run(() => locations = connection.Locations.ToList());

            // Add locations.
            this.locationsValue = new ObservableCollection<LocationViewModel>();
            foreach (Location location in locations)
                this.locationsValue.Add(new LocationViewModel(location));

            // Fetch Groups.
            List<CityGroup> groups = null;
            Debug.WriteLine("Initializing groups");
            await Task.Run(() => groups = connection.CityGroups.ToList());

            // Add Groups.
            this.cityGroupsValue = new ObservableCollection<CityGroupViewModel>();
            foreach (CityGroup group in groups)
                this.cityGroupsValue.Add(new CityGroupViewModel(group));

            // Fetch Subdivisions.
            List<Subdivision> subdivisions = null;
            await Task.Run(() => subdivisions = connection.Subdivisions.ToList());

            // Add Subdivisions.
            this.subDivisionsValue = new ObservableCollection<SubdivisionViewModel>();
            foreach (Subdivision subdivision in subdivisions)
                this.subDivisionsValue.Add(new SubdivisionViewModel(subdivision));

            // Add commodities.
            List<Commodity> commodities = null;
            Debug.WriteLine("Initializing commodities");
            await Task.Run(() => commodities = connection.Commodities.ToList());

            this.commoditiesValue = new ObservableCollection<CommodityViewModel>();
            foreach (Commodity commodity in commodities)
            {
                CommodityViewModel commodityViewModel = new CommodityViewModel(commodity);
                this.commoditiesValue.Add(commodityViewModel);
            }

            // Add roads
            List<Road> roads = null;
            Debug.WriteLine("Initializing roads");
            await Task.Run(() => roads = connection.Roads.ToList());

            this.roadsValue = new ObservableCollection<RoadViewModel>();
            foreach (Road road in roads)
            {
                RoadViewModel roadViewModel = new RoadViewModel(road);
                this.roadsValue.Add(roadViewModel);
            }

            // Fetch Railroads.
            List<Railroad> railRoads = null;
            await Task.Run(() => railRoads = connection.Railroads.ToList());

            this.railRoadsValue = new ObservableCollection<RailroadViewModel>();
            this.railRoadsTrackValue = new ObservableCollection<RailroadViewModel>();

            foreach (Railroad railRoad in railRoads)
            {
                RailroadViewModel railroadViewModel = new RailroadViewModel(railRoad);
                this.railRoadsValue.Add(railroadViewModel);
            }

            foreach (Railroad railRoad in railRoads)
            {
                RailroadViewModel railroadViewModel = new RailroadViewModel(railRoad);
                this.railRoadsTrackValue.Add(railroadViewModel);
            }

            // Fetch CarTypes
            List<CarType> carTypes = null;
            await Task.Run(() => carTypes = connection.CarTypes.ToList());

            this.carTypesValue = new ObservableCollection<CarTypeViewModel>();
            foreach (CarType carType in carTypes)
            {
                CarTypeViewModel carTypeViewModel = new CarTypeViewModel(carType);
                this.carTypesValue.Add(carTypeViewModel);
            }

            // Fetch CarGroups
            List<CarGroup> carGroups = null;
            await Task.Run(() => carGroups = connection.CarGroups.ToList());

            this.carGroupsValue = new ObservableCollection<CarGroupViewModel>();
            foreach (CarGroup carGroup in carGroups)
            {
                CarGroupViewModel carGroupViewModel = new CarGroupViewModel(carGroup);
                carGroupViewModel.ChangesMade += CityViewModel_ChangesMade;
                this.carGroupsValue.Add(carGroupViewModel);
            }

            List<Spur> spurs = null;
            Debug.WriteLine("Initializing spurs");
            await Task.Run(() => spurs = connection.Spurs.ToList());

            // Add spurs.
            this.spursValue = new ObservableCollection<SpurViewModel>();
            foreach (Spur spur in spurs)
            {
                SpurViewModel spurModel = new SpurViewModel(spur);
                this.spursValue.Add(spurModel);
            }
            

            List<Printer> printers = null;
            Debug.WriteLine("Initializing printer");
            await Task.Run(() => printers = connection.Printers.ToList());

            this.printersValue = new ObservableCollection<PrinterViewModel>();
            foreach (Printer printer in printers)
                this.printersValue.Add(new PrinterViewModel(printer));
            this.printersViewValue = new ListCollectionView(this.printersValue);


            List<Difficulty> difficulties = null;
            await Task.Run(() => difficulties = connection.Difficulties.ToList());
            foreach (Difficulty difficulty in difficulties)
                kammFactorDifficulties.Add(difficulty);

            this.initializeNewGroup();
            this.initializeNewSubdivision();
        }

        private void SpurViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.updatingSpursSelection)
                return;

            // Update selection status if there is a car currently selected.
            if (e.PropertyName == nameof(CommodityViewModel.IsSelected) && this.SelectedViewModel != null)
                this.SelectedViewModel.UpdateSpurStatusCommand.Execute(sender);
        }

        private void RailRoadViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.updatingRailroadsSelection)
                return;

            // Update selection status if there is a car currently selected.
            if (e.PropertyName == nameof(RailroadViewModel.IsSelected) && this.SelectedViewModel != null)
                this.SelectedViewModel.UpdateRailroadStatusCommand.Execute(sender);
        }

        private void RailRoadTrackViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.updatingRailroadsTrackSelection)
                return;

            // Update selection status if there is a car currently selected.
            if (e.PropertyName == nameof(RailroadViewModel.IsSelected) && this.SelectedViewModel != null)
                this.SelectedViewModel.UpdateRailroadTrackStatusCommand.Execute(sender);
        }
        
        private void CarTypeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.updatingCartypesSelection)
                return;

            // Update selection status if there is a car currently selected.
            if (e.PropertyName == nameof(CarTypeViewModel.IsSelected) && this.SelectedViewModel != null)
                this.SelectedViewModel.UpdateCartypeStatusCommand.Execute(sender);
        }

        private void CommodityViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.updatingCommoditiesSelection)
                return;

            // Update selection status if there is a car currently selected.
            if (e.PropertyName == nameof(CommodityViewModel.IsSelected) && this.SelectedViewModel != null)
                this.SelectedViewModel.UpdateCommodityStatusCommand.Execute(sender);
        }

        private void RoadViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.updatingRoadsSelection)
                return;

            // Update selection status if there is a car currently selected.
            if (e.PropertyName == nameof(RoadViewModel.IsSelected) && this.SelectedViewModel != null)
                this.SelectedViewModel.UpdateRoadStatusCommand.Execute(sender);
        }
        
        private async Task initializeValidator()
        {
            Action initializeValidator = () =>
            {
                // Prepare validator.
                Debug.WriteLine("Initializing validator.");
                this.validator = new Validator<CityViewModel>(FileUtilities.GetFullpath(SpecialFileName.CityValidationRuleSet));
            };
            await Task.Run(initializeValidator);
        }

        private void performViewModelCleanUp()
        {
            // Dispose of existing data set.
            foreach (CityViewModel CityViewModel in this.ViewModels)
            {
                CityViewModel.ChangesMade -= this.CityViewModel_ChangesMade;
                CityViewModel.Dispose();
            }
            this.ViewModels.Clear();

            // Dispose of existing supporting lists.

        }

        private void commitHistory()
        {
            // Get list of cars with changes. Order them by change date/time stamp.
            // NOTE: not using OrderByDescending because history manager reverses
            // them.
            IEnumerable<CityViewModel> modifiedViewModels = this.ViewModels.Where(x => x.HasChanges).OrderBy(x => x.LastChanged);
            foreach (CityViewModel viewModel in modifiedViewModels)
            {
                // Create history item.
                HistoryItem newHistoryItem = new HistoryItem
                {
                    UniqueIdentifier = viewModel.UniqueId,
                    ItemType = HistoryItemType.City,
                    Text = viewModel.DisplayText,
                    Data = null,
                };

                // Add to history manager.
                this.conductor.HistoryManager.AddUpdateHistoryItem(newHistoryItem);
            }
        }

        /// <summary>
        /// Saves any pending changes to the database after verifying no 
        /// validation errors are present.
        /// </summary>
        private async Task<bool> saveAsync()
        {
            try
            {
                // Warn on validation errors.
                this.warnIfHasValidationErrors();

                // Prepare status.
                this.StatusModel.SetStatus("Saving changes... please wait.");

                // Perform save.
                Action save = () =>
                {
                    // Save supporting lists.
                    this.saveGroup(this.trackBossConnection);
                    this.saveSubdivision(this.trackBossConnection);

                    // Save cities.
                    this.saveCities(this.trackBossConnection);

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

        private void saveGroup(TrackBossEntities connection)
        {
            // New cars will have changes but will also have an ID of zero.
            IEnumerable<CityGroup> modifiedGroups = this.cityGroupsValue.Where(x => x.HasChanges).Select(x => x.ToCityGroup());
            IEnumerable<CityGroup> newGroups = modifiedGroups.Where(x => x.ID == 0);
            modifiedGroups = modifiedGroups.Except(newGroups);

            // Add new owners.
            foreach (CityGroup newGroup in newGroups)
                connection.CityGroups.Add(newGroup);
        }

        private void saveSubdivision(TrackBossEntities connection)
        {
            // New cars will have changes but will also have an ID of zero.
            IEnumerable<Subdivision> modifiedSubdivisions = this.subDivisionsValue.Where(x => x.HasChanges).Select(x => x.ToSubdivision());
            IEnumerable<Subdivision> newSubdivisions = modifiedSubdivisions.Where(x => x.ID == 0);
            modifiedSubdivisions = modifiedSubdivisions.Except(newSubdivisions);

            // Add new owners.
            foreach (Subdivision newSubdivision in newSubdivisions)
                connection.Subdivisions.Add(newSubdivision);
        }

        private void saveCities(TrackBossEntities connection)
        {
            List<City> cities = null;
            cities = connection.Cities.ToList();

            List<Spur> spurs = null;
            spurs = connection.Spurs.ToList();

            // New cars will have changes but will also have an ID of zero.
            IEnumerable<CityViewModel> newViewModels = this.ViewModels.Where(x => x.ID == 0);
            IEnumerable<City> allCities = this.ViewModels.Select(x => x.ToCity());
            IEnumerable<City> newCities = newViewModels.Select(x => x.ToCity());

            // Add new cities.
            foreach (City newCity in newCities)
                connection.Cities.Add(newCity);
        }

        private async Task initializeCities(TrackBossEntities connection)
        {
            // Fetch list of cities.
            List<City> cities = null;
            await Task.Run(() => cities = connection.Cities.ToList());

            // Add cities to list.
            if (cities != null)
            {
                foreach (City city in cities)
                {
                    // Prepare new ViewModel.
                    city.Neighbors.Clear();
                    foreach (City item in cities)
                    {
                        if(city == item)
                            continue;

                        city.Neighbors.Add(item);
                    }

                    CityViewModel newCityViewModel = new CityViewModel(this, city);
                    newCityViewModel.ChangesMade += this.CityViewModel_ChangesMade;
                    this.ViewModels.Add(newCityViewModel);
                }
            }
        }

        
        #endregion

        #region Event Handlers

        private void CityViewModel_ChangesMade(object sender, EventArgs e)
        {
            // Validate parameters.
            Debug.WriteLine(string.Format("CitiesDesignerViewModel - CityViewModel_ChangesMade: {0}", sender.ToString()));
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

        private void Preferences_PreferencesChanged(object sender, EventArgs<string> e)
        {
            Debug.WriteLine(e.Value);
            
        }

        private async void ValidationTimer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("CitiesDesignerViewModel - ValidationTimer_Tick");

            // Deactivate timer.
            this.validationTimer.Stop();

            // Perform validation.
            await this.performValidation();
        }

        #endregion

        #region Command Handlers

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
                    Title = string.Format("{0} - Cities", this.conductor.Name),
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
            await this.saveAsync();

            // Commit modifications and additions (deletions are handled on-the-fly).
            this.commitHistory();
        }

        protected override async void deleteCommandExecuteHelper(object obj)
        {
            // Get selected city.
            CityViewModel selectedCityViewModel = this.SelectedViewModel;
            City selectedCity = SelectedViewModel.ToCity();

            // Verify the user wants to delete this car.
            ShowMessageBoxMessage message = new ShowMessageBoxMessage();
            message.Message = string.Format(
                                "This will delete the currently selected car:\n\n{0}\n\nAre you sure you want to do this?", selectedCity);
            message.Button = MessageBoxButton.YesNo;
            message.Icon = MessageBoxImage.Exclamation;
            message.Title = this.conductor.Name;
            Messenger.Default.Send<ShowMessageBoxMessage>(message);

            // Check result.
            if (message.Result == MessageBoxResult.Yes)
            {
                // Remove selection and remove ViewModel from list.
                this.SelectedViewModel = null;
                this.ViewModels.Remove(selectedCityViewModel);

                // Disconnect change event. This event should not fire
                // for deleted ViewModels.
                selectedCityViewModel.ChangesMade -= this.CityViewModel_ChangesMade;

                // Remove validation results pertaining to this car, if any.
                for (int i = this.validationResultsValue.Count - 1; i > -1; i--)
                {
                    DataValidationResultViewModel<CityViewModel> result = this.validationResultsValue[i];
                    if (result.Source == selectedCityViewModel)
                        this.validationResultsValue.RemoveAt(i);
                }

                // Remove history item, if applicable.
                //this.conductor.HistoryManager.RemoveHistoryItem(selectedCrewViewModel.UniqueId);

                // Dispose of removed ViewModel.
                selectedCityViewModel.Dispose();

                // Get crew being removed.
                City cityToRemove = selectedCityViewModel.ToCity();
                Site siteToRemove = selectedCityViewModel.ToCity().Site;
                Location locationToRemove = selectedCityViewModel.ToCity().Site.Location;

                // Perform clean-up on city.
                //await carToRemove.RollingStock.Photo.Clear();

                // Remove car from data context. If this is a new crew, there
                // will be no currently assigned ID and nothing else to do.
                if (cityToRemove.ID != 0)
                {
                    this.trackBossConnection.Configuration.AutoDetectChangesEnabled = false; // ???
                    if (this.trackBossConnection.Cities.Remove(cityToRemove) != null)
                    {
                        this.trackBossConnection.Sites.Remove(siteToRemove);
                        this.trackBossConnection.Locations.Remove(locationToRemove);
                    }
                    this.trackBossConnection.Configuration.AutoDetectChangesEnabled = true; // ???
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
            City newCity = City.Create();
            newCity.Site.Location.Sites.Add(newCity.Site);
            //newCity.Site.City = newCity;

            // Add new car to list.
            this.addNewCityHelper(newCity);

            // Queue validation.
            this.validationTimer.Stop();
            this.validationTimer.Start();
        }

        private void SelectCityById(long id)
        {
            foreach (CityViewModel cityViewModel in this.ViewModels)
            {
                if (cityViewModel.ID == id)
                {
                    this.SelectedViewModel = cityViewModel;
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
            long lastSelectedCityId = -1;
            if (this.SelectedViewModel != null)
                lastSelectedCityId = this.SelectedViewModel.ToCity().ID;

            // Perform cleanup.
            this.performViewModelCleanUp();

            // Destroy connection object.
            this.trackBossConnection.Dispose();
            this.trackBossConnection = null;

            // Reinitialize the data set.
            if (this.InitializeCommand.CanExecute(null))
                await this.initializeAsync();

            // Reselect last selected car, if applicable.
            if (lastSelectedCityId != -1)
                SelectCityById(lastSelectedCityId);

            // Clear changes.
            this.hasChanges = false;

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
            
            // If there are cars and none are selected, then
            // select the first one.
            if (this.SelectedViewModel == null && this.ViewModels.Count > 0)
                this.SelectedViewModel = (CityViewModel)this.ViewModelsView.CurrentItem;
        }

        private void updateConfiguration()
        {
            this.configuration.SortType = this.selectedSort;
            this.configuration.SortDirection = this.currentSortDirection;
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
            CityViewModel lastSelectedViewModel = this.SelectedViewModel;

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

        private bool FilterCommandCanExecute(CityFilterType obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private void FilterCommandExecute(CityFilterType obj)
        {
            // Make sure this is allowed.
            if (!this.SortCommand.CanExecute(obj))
                return;
            SelectedFilter = obj;
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

        private bool NewSpurCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private void addNewSpurHelper(Spur newSpur)
        {
            newSpur.Track.Location.Name = "New Spur";
            SpurViewModel newSpurViewModel = new SpurViewModel(this, newSpur);
            SelectedViewModel.SpurViewModels.Add(newSpurViewModel);

            // Create ViewModel and attach event handlers.
            SpurIndustryViewModel superIndustryViewModel = new SpurIndustryViewModel();
            superIndustryViewModel.DisplayText = "New Spur";
            superIndustryViewModel.iconType = IconType.Spur;
            superIndustryViewModel.spurViewModel = newSpurViewModel;

            SelectedViewModel.SpurIndustryViewModels.Add(superIndustryViewModel);

            SelectedViewModel.ToCity().Spurs.Add(newSpur);

            // Select new item.
            this.SelectedViewModel.SelectedSubItem = superIndustryViewModel;

            // Mark designer as having changes.
            this.hasChanges = true;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        private void NewSpurCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.NewSpurCommand.CanExecute(obj))
                return;

            // Create new spur.
            Spur newSpur = Spur.Create(SelectedViewModel.ToCity());
            addNewSpurHelper(newSpur);

            // Queue validation.
            this.validationTimer.Stop();
            this.validationTimer.Start();
        }

        private bool NewIndustryCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.StatusModel.IsBusy;
        }

        private void addNewIndustryHelper(Industry newIndustry)
        {
            newIndustry.Site.Location.Name = "New Industry";
            IndustryViewModel newIndustryViewModel = new IndustryViewModel(this, newIndustry);
            SelectedViewModel.IndustryViewModels.Add(newIndustryViewModel);

            // Create ViewModel and attach event handlers.
            SpurIndustryViewModel superIndustryViewModel = new SpurIndustryViewModel();
            superIndustryViewModel.DisplayText = "New Industry";
            superIndustryViewModel.iconType = IconType.Industry;
            superIndustryViewModel.industryViewModel = newIndustryViewModel;

            SelectedViewModel.SpurIndustryViewModels.Add(superIndustryViewModel);

            SelectedViewModel.ToCity().Industries.Add(newIndustry);

            // Select new item.
            this.SelectedViewModel.SelectedSubItem = superIndustryViewModel;

            // Mark designer as having changes.
            this.hasChanges = true;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        private void NewIndustryCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.NewIndustryCommand.CanExecute(obj))
                return;

            // Create new industry.
            Industry newIndustry = Industry.Create(SelectedViewModel.ToCity());
            newIndustry.Site.Location.Sites.Add(newIndustry.Site);

            addNewIndustryHelper(newIndustry);

            // Queue validation.
            this.validationTimer.Stop();
            this.validationTimer.Start();
        }

        private bool DeleteSubItemCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            if(this.SelectedViewModel == null || this.SelectedViewModel.SpurIndustryViewModels.Count <= 0)
                return false;

            return this.SelectedViewModel.EnableDeleteSubItem;
        }

        private void DeleteSubItemCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.DeleteSubItemCommand.CanExecute(obj))
                return;

            if (SelectedViewModel.SelectedSubItem.iconType == IconType.Spur)
            {
                SelectedViewModel.SpurViewModels.Remove(SelectedViewModel.SpurViewModels.Where(i => i.CurSpur == SelectedViewModel.SelectedSubItem.spurViewModel.CurSpur).Single());
                SelectedViewModel.SpurIndustryViewModels.Remove(SelectedViewModel.SelectedSubItem);
                SelectedViewModel.ToCity().Spurs.Remove(SelectedViewModel.SelectedSubItem.spurViewModel.CurSpur);

                // Get spur being removed.
                Spur spurToRemove = SelectedViewModel.SelectedSubItem.spurViewModel.CurSpur;

                if (spurToRemove.Track.Location.ID != 0)
                {
                    if (this.trackBossConnection.Locations.Remove(spurToRemove.Track.Location) == null)
                    {
                        // TODO: Decide what to do here.
                    }
                }

                // Remove car from data context. If this is a new car, there
                // will be no currently assigned ID and nothing else to do.
                if (spurToRemove.ID != 0)
                {
                    if (this.trackBossConnection.Spurs.Remove(spurToRemove) == null)
                    {
                        // TODO: Decide what to do here.
                    }
                }

            }
            else if (SelectedViewModel.SelectedSubItem.iconType == IconType.Industry)
            {
                SelectedViewModel.IndustryViewModels.Remove(SelectedViewModel.IndustryViewModels.Where(i => i.CurIndustry == SelectedViewModel.SelectedSubItem.industryViewModel.CurIndustry).Single());
                SelectedViewModel.SpurIndustryViewModels.Remove(SelectedViewModel.SelectedSubItem);
                SelectedViewModel.ToCity().Industries.Remove(SelectedViewModel.SelectedSubItem.industryViewModel.CurIndustry);

                // Get spur being removed.
                Industry industryToRemove = SelectedViewModel.SelectedSubItem.industryViewModel.CurIndustry;


                if(industryToRemove.Site.Location.ID != 0)
                {
                    if (this.trackBossConnection.Locations.Remove(industryToRemove.Site.Location) == null)
                    {
                        // TODO: Decide what to do here.
                    }
                }

                // Remove car from data context. If this is a new car, there
                // will be no currently assigned ID and nothing else to do.
                if (industryToRemove.ID != 0)
                {
                    if (this.trackBossConnection.Industries.Remove(industryToRemove) == null)
                    {
                        // TODO: Decide what to do here.
                    }
                }
            }


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

        private void NewGroupValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Invalidate command.
            this.addGroupCommandValue.InvalidateCanExecuteChanged();
        }

        private void initializeNewGroup()
        {
            // Create new object.
            CityGroup newGroup = new CityGroup();

            // Create ViewModel and hook-up.
            this.newGroupValue = new CityGroupViewModel(newGroup);
            this.newGroupValue.PropertyChanged += this.NewGroupValue_PropertyChanged;
        }

        private bool AddGroupCommandCanExecute(object obj)
        {
            // Determine if this should be allowed.
            return this.SelectedViewModel != null && this.newGroupValue != null && !string.IsNullOrWhiteSpace(this.newGroupValue.Name);
        }

        private void AddGroupCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AddGroupCommand.CanExecute(obj))
                return;

            // Unhook changed handler.
            this.newGroupValue.PropertyChanged -= this.NewGroupValue_PropertyChanged;

            // Select for the current car.
            this.SelectedViewModel.CityGroup = this.newGroupValue;

            // Add to main list.
            this.Groups.Add(this.newGroupValue);

            // Prepare next new road.
            this.initializeNewGroup();

            // Notify property.
            this.NotifyPropertyChanged(nameof(CarsDesignerViewModel.NewCarType));
        }

        private void initializeNewSubdivision()
        {
            // Create new object.
            Subdivision newSubdivision = new Subdivision();

            // Create ViewModel and hook-up.
            this.newSubdivisionValue = new SubdivisionViewModel(newSubdivision);
            this.newSubdivisionValue.PropertyChanged += this.NewSubdivisionValue_PropertyChanged;
        }

        private void NewSubdivisionValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Invalidate command.
            this.addSubdivisionCommandValue.InvalidateCanExecuteChanged();
        }

        private bool AddSubdivisionCommandCanExecute(object obj)
        {
            // Determine if this should be allowed.
            return this.SelectedViewModel != null && this.newSubdivisionValue != null && !string.IsNullOrWhiteSpace(this.newSubdivisionValue.Name);
        }

        private void AddSubdivisionCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AddSubdivisionCommand.CanExecute(obj))
                return;

            // Unhook changed handler.
            this.newSubdivisionValue.PropertyChanged -= this.NewSubdivisionValue_PropertyChanged;

            // Select for the current car.
            this.SelectedViewModel.Subdivision = this.newSubdivisionValue;

            // Add to main list.
            this.SubDivisions.Add(this.newSubdivisionValue);

            // Prepare next new road.
            this.initializeNewSubdivision();

            // Notify property.
            this.NotifyPropertyChanged(nameof(CarsDesignerViewModel.NewCarType));
        }


        public void invalidateAllCommands_Industry()
        {
            // Update whether or not this object has changes.
            this.hasChanges = true;

            // Restart validation timer.
            this.validationTimer.Stop();
            this.validationTimer.Start();

            // Invalidate commands.
            this.invalidateAllCommands();
        }

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
            this.newSpurCommandValue.InvalidateCanExecuteChanged();
            this.newIndustryCommandValue.InvalidateCanExecuteChanged();
            this.deleteSubItemCommandValue.InvalidateCanExecuteChanged();
            this.saveAndAddNewCommandValue.InvalidateCanExecuteChanged();
            //this.duplicateCommandValue.InvalidateCanExecuteChanged();
            //this.showLocationSelectionDialogCommandValue.InvalidateCanExecuteChanged();
        }

        protected override void selectionChangeHelper()
        {
            if (this.SelectedViewModel != null)
            {
                SpurIndustryViewModel spurIndustryViewModel;

                SelectedViewModel.SpurIndustryViewModels.Clear();

                if (SelectedFilter == CityFilterType.All || SelectedFilter == CityFilterType.Characteristics)
                {
                    spurIndustryViewModel = new SpurIndustryViewModel();
                    spurIndustryViewModel.DisplayText = "General";
                    spurIndustryViewModel.iconType = IconType.General;
                    SelectedViewModel.SpurIndustryViewModels.Add(spurIndustryViewModel);

                    SelectedViewModel.SelectedSubItem = spurIndustryViewModel;
                }

                City city = this.SelectedViewModel.ToCity();
                if (SelectedFilter == CityFilterType.All || SelectedFilter == CityFilterType.Industries)
                {
                    foreach (var item in city.Industries)
                    {
                        IndustryViewModel newIndustryViewModel = new IndustryViewModel(this, item);

                        spurIndustryViewModel = new SpurIndustryViewModel();
                        spurIndustryViewModel.industryViewModel = newIndustryViewModel;
                        spurIndustryViewModel.DisplayText = item.ToString();
                        spurIndustryViewModel.iconType = IconType.Industry;

                        SelectedViewModel.SpurIndustryViewModels.Add(spurIndustryViewModel);

                        //SelectedViewModel.IndustryViewModels.Add(spurIndustryViewModel);
                    }
                }

                if (SelectedFilter == CityFilterType.All || SelectedFilter == CityFilterType.Spurs)
                { 
                    foreach (var item in city.Spurs)
                    {
                        SpurViewModel newSpurViewModel = new SpurViewModel(this, item);
                        spurIndustryViewModel = new SpurIndustryViewModel();
                        spurIndustryViewModel.spurViewModel = newSpurViewModel;
                        spurIndustryViewModel.DisplayText = item.ToString();
                        spurIndustryViewModel.iconType = IconType.Spur;

                        SelectedViewModel.SpurIndustryViewModels.Add(spurIndustryViewModel);

                        //SelectedViewModel.SpurViewModels.Add(spurIndustryViewModel);
                    }
                }

                if (SelectedFilter == CityFilterType.All)
                {
                    spurIndustryViewModel = new SpurIndustryViewModel();
                    spurIndustryViewModel.DisplayText = "Print Scheme";
                    spurIndustryViewModel.iconType = IconType.PrintScheme;
                    SelectedViewModel.SpurIndustryViewModels.Add(spurIndustryViewModel);
                }

                //SelectedViewModel.SelectedSubItem = SelectedViewModel.SpurIndustryViewModels[0];

            }
        }

        #endregion

        #region Message Handlers

        private void ValidationSourceSelectedMessageHandler(ValidationSourceSelectedMessage message)
        {
            // Validate message.
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // Make sure there is something to do.
            if (message.Handled)
                return;

            // Make sure this object should process the message.
            if (message.Source is CityViewModel)
            {
                // Mark message as handled.
                message.Handled = true;

                // Attempt to locate city. This is technically redundant, but 
                // prevents the case that this object is handling a message
                // for a prior or disposed object (I hope).
                CityViewModel sourceCityViewModel = (CityViewModel)message.Source;
                int index = this.ViewModels.IndexOf(sourceCityViewModel);
                if (index == -1)
                    throw new InvalidOperationException("The city is not in the list.");

                // Select the car.
                this.SelectedViewModel = sourceCityViewModel;

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
            if (message.HistoryItem.ItemType == HistoryItemType.City)
            {
                // Attempt to locate item.
                foreach (CityViewModel cityViewModel in this.ViewModels)
                {
                    if (cityViewModel.UniqueId == (string)message.HistoryItem.UniqueIdentifier)
                    {
                        // Set message as handled.
                        message.Handled = true;

                        // Select item.
                        this.SelectedViewModel = cityViewModel;

                        // We're done.
                        break;
                    }
                }
            }
        }

        #endregion

        #region Properties

        public ICollectionView ViewModelsView
        {
            get { return this.viewModelsViewValue; }
        }

        public ObservableCollection<DataValidationResultViewModel<CityViewModel>> ValidationResults
        {
            get { return this.validationResultsValue; }
        }

        public ObservableCollection<LocationViewModel> Locations
        {
            get { return this.locationsValue; }
        }

        public ICommand SaveAndAddNewCommand
        {
            get { return this.saveAndAddNewCommandValue; }
        }

        public ICommand NewSpurCommand
        {
            get { return this.newSpurCommandValue; }
        }

        public ICommand NewIndustryCommand
        {
            get { return this.newIndustryCommandValue; }
        }

        public ICommand DeleteSubItemCommand
        {
            get { return this.deleteSubItemCommandValue; }
        }

        public RelayCommand<CitySortType> SortCommand
        {
            get { return this.sortCommandValue; }
        }

        public ICommand ReverseSortCommand
        {
            get { return this.reverseSortCommandValue; }
        }

        public RelayCommand<CityFilterType> FilterCommand
        {
            get { return this.filterCommandValue; }
        }

        public IAsyncCommand InitializeCommand
        {
            get { return this.initializeCommandValue; }
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

        public CityFilterType SelectedFilter
        {
            get { return this.selectedFilter; }
            private set
            {
                if (this.selectedFilter != value)
                {
                    this.selectedFilter = value;
                    this.NotifyPropertyChanged();

                    if(this.viewModelsViewValue != null)
                        this.viewModelsViewValue.Refresh();
                }
            }
        }
        public ObservableCollection<CityGroupViewModel> Groups
        {
            get { return this.cityGroupsValue; }
        }

        public ObservableCollection<SubdivisionViewModel> SubDivisions
        {
            get { return this.subDivisionsValue; }
        }

        public ObservableCollection<CommodityViewModel> Commodities
        {
            get { return this.commoditiesValue; }
        }

        public ObservableCollection<RoadViewModel> Roads
        {
            get { return this.roadsValue; }
        }

        public ObservableCollection<CarTypeViewModel> CarTypes
        {
            get { return this.carTypesValue; }
        }

        public ObservableCollection<CarGroupViewModel> CarGroups
        {
            get { return this.carGroupsValue; }
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


        public CityGroupViewModel NewGroup
        {
            get { return this.newGroupValue; }
            set
            {
                if (this.newGroupValue != value)
                {
                    this.newGroupValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public SubdivisionViewModel NewSubdivision
        {
            get { return this.newSubdivisionValue; }
            set
            {
                if (this.newSubdivisionValue != value)
                {
                    this.newSubdivisionValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ICommand AddGroupCommand
        {
            get { return this.addGroupCommandValue; }
        }

        public ICommand AddSubdivisionCommand
        {
            get { return this.addSubdivisionCommandValue; }
        }

        public ObservableCollection<SpurViewModel> Spurs
        {
            get { return this.spursValue; }
        }

        public ObservableCollection<RailroadViewModel> Railroads
        {
            get { return this.railRoadsValue; }
        }

        public ObservableCollection<RailroadViewModel> RailroadsTrack
        {
            get { return this.railRoadsTrackValue; }
        }
        
        public ObservableCollection<Difficulty> KammFactorDifficulties
        {
            get { return kammFactorDifficulties; }
        }
        #endregion
    }
}
