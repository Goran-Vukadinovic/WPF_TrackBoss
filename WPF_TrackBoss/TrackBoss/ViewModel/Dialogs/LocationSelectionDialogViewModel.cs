using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Input;
using TrackBoss.Configuration;
using TrackBoss.Configuration.Preferences.Layout;
using TrackBoss.Data;
using TrackBoss.Data.Enumerations;
using TrackBoss.Model.Shared;
using TrackBoss.Mvvm.Shared.Commands;
using TrackBoss.Shared.Enumerations;
using TrackBoss.ViewModel.Locations;
using TrackBoss.ViewModel.RollingStocks;
using TrackBoss.ViewModel.Shared;
using TrackBoss.Windsor;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2021 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Dialogs
{
    public class LocationSelectionDialogViewModel : ViewModelBase
    {
        #region Fields

        private readonly Conductor conductor;

        private readonly StatusModel statusModel;

        private RollingStockViewModel rollingStockViewModelValue;

        private LocationSelectionDialogLayoutConfiguration configuration;

        private readonly RelayCommand applyCommandValue;

        private ObservableCollection<LocationViewModel> locationViewModelsValue;

        private ObservableCollection<LocationStatisticsViewModel> locationsStatisticsViewModelsValue;

        private readonly RelayCommand<LocationSortType> primarySortCommandValue;

        private readonly RelayCommand reversePrimarySortCommandValue;

        private readonly RelayCommand<LocationSortType> secondarySortCommandValue;

        private readonly RelayCommand reverseSecondarySortCommandValue;

        private readonly RelayCommand<LocationSortType> tertiarySortCommandValue;

        private readonly RelayCommand reverseTertiarySortCommandValue;

        private ListCollectionView primaryLocationsListViewValue;

        private ListCollectionView secondaryLocationsListViewValue;

        private ListCollectionView tertiaryLocationsListViewValue;

        private LocationViewModel selectedPrimaryLocationValue;

        private LocationViewModel selectedSecondaryLocationValue;

        private LocationViewModel selectedTertiaryLocationValue;

        private LocationViewModel selectedLocationValue;

        private ObservableCollection<string> primaryLocationTypesValue;
        
        private ObservableCollection<string> secondaryLocationTypesValue;

        private ObservableCollection<string> tertiaryLocationTypesValue;

        private string primaryLocationTypeFilterValue;
        
        private string secondaryLocationTypeFilterValue;

        private string tertiaryLocationTypeFilterValue;

        private string primarySearchTextValue;

        private string secondarySearchTextValue;

        private string tertiarySearchTextValue;

        private LocationSortType primarySelectedSortValue;

        private LocationSortType secondarySelectedSortValue;

        private LocationSortType tertiarySelectedSortValue;

        private ListSortDirection primaryCurrentSortDirectionValue;

        private ListSortDirection secondaryCurrentSortDirectionValue;

        private ListSortDirection tertiaryCurrentSortDirectionValue;

        private bool primaryLocationTypesFilterIsEnabledValue;
        
        private bool secondaryLocationTypesFilterIsEnabledValue;

        private bool tertiaryLocationTypesFilterIsEnabledValue;

        private bool hideInvalidLocationsValue;

        private bool hideFullLocationsValue;

        private string rollingStockIdentifierValue;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Default constructor. Initializes fields and prepares object
        /// for use.
        /// </summary>
        private LocationSelectionDialogViewModel()
        {
            // Prepare fields.
            this.locationViewModelsValue = new ObservableCollection<LocationViewModel>();
            this.primaryLocationTypesValue = new ObservableCollection<string>();
            this.secondaryLocationTypesValue = new ObservableCollection<string>();
            this.tertiaryLocationTypesValue = new ObservableCollection<string>();
            this.locationsStatisticsViewModelsValue = new ObservableCollection<LocationStatisticsViewModel>();

            // Prepare status model and conductor.
            this.conductor = CastleWindsor.Default.Resolve<Conductor>();
            this.statusModel = CastleWindsor.Default.Resolve<StatusModel>();

            // Hookup command handlers.
            this.applyCommandValue = new RelayCommand(this.ApplyCommandExecute, this.ApplyCommandCanExecute);
            this.primarySortCommandValue = new RelayCommand<LocationSortType>(this.PrimarySortCommandExecute, this.PrimarySortCommandCanExecute);
            this.secondarySortCommandValue = new RelayCommand<LocationSortType>(this.SecondarySortCommandExecute, this.SecondarySortCommandCanExecute);
            this.tertiarySortCommandValue = new RelayCommand<LocationSortType>(this.TertiarySortCommandExecute, this.TertiarySortCommandCanExecute);
            this.reversePrimarySortCommandValue = new RelayCommand(this.ReversePrimarySortCommandExecute, this.ReversePrimarySortCommandCanExecute);
            this.reverseSecondarySortCommandValue = new RelayCommand(this.ReverseSecondarySortCommandExecute, this.ReverseSecondarySortCommandCanExecute);
            this.reverseTertiarySortCommandValue = new RelayCommand(this.ReverseTertiarySortCommandExecute, this.ReverseTertiarySortCommandCanExecute);
        }

        /// <summary>
        /// Overload constructor. Initializes this object using the specified
        /// parameters.
        /// </summary>
        /// <param name="locations">List of all locations on the layout.</param>
        public LocationSelectionDialogViewModel(Location[] locations, RollingStockViewModel rollingStock, string rollingStockIdentifier) : this()
        {
            // Validate locations list.
            if (locations == null)
                throw new ArgumentNullException(nameof(locations));

            // Validate rolling stock status.
            if (rollingStock == null)
                throw new ArgumentNullException(nameof(rollingStock));

            // Validate description.
            if (string.IsNullOrEmpty(rollingStockIdentifier))
                throw new InvalidOperationException("The rolling stock identifier is missing or invalid.");

            // Use stored or default layout configuration, whichever is applicable.
            LocationSelectionDialogLayoutConfiguration defaultConfiguration = new LocationSelectionDialogLayoutConfiguration();
            this.configuration = this.conductor.Preferences.LoadLayoutConfiguration<LocationSelectionDialogLayoutConfiguration>(defaultConfiguration.Name);
            if (this.configuration == null)
            {
                this.configuration = defaultConfiguration;
                this.conductor.Preferences.SaveLayoutConfiguration(this.configuration);
            }

            // Set sort from configuration (or default).
            this.primarySelectedSortValue = this.configuration.PrimarySortType;
            this.primaryCurrentSortDirectionValue = this.configuration.PrimarySortDirection;
            this.secondarySelectedSortValue = this.configuration.SecondarySortType;
            this.secondaryCurrentSortDirectionValue = this.configuration.SecondarySortDirection;
            this.tertiarySelectedSortValue = this.configuration.TertiarySortType;
            this.tertiaryCurrentSortDirectionValue = this.configuration.TertiarySortDirection;

            // Set hide location options from configuration (or default).
            this.hideFullLocationsValue = this.configuration.HideFullLocations;
            this.hideInvalidLocationsValue = this.configuration.HideInvalidLocations;

            // Set rolling stock and update display text.
            this.rollingStockIdentifierValue = rollingStockIdentifier;
            this.rollingStockViewModelValue = rollingStock;

            // Load locations and prepare list views for use. This needs to be done
            // after the configuration has been applied.
            this.loadLocations(locations);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the unbound configuration settings.
        /// </summary>
        private void updateConfiguration()
        {
            this.configuration.PrimarySortType = this.primarySelectedSortValue;
            this.configuration.PrimarySortDirection = this.primaryCurrentSortDirectionValue;
            this.configuration.SecondarySortType = this.secondarySelectedSortValue;
            this.configuration.SecondarySortDirection = this.secondaryCurrentSortDirectionValue;
            this.configuration.TertiarySortType = this.tertiarySelectedSortValue;
            this.configuration.TertiarySortDirection = this.tertiaryCurrentSortDirectionValue;
            this.configuration.HideFullLocations = this.hideFullLocationsValue;
            this.configuration.HideInvalidLocations = this.hideInvalidLocationsValue;
        }

        private void updateSort(ListCollectionView listCollectionView, LocationSortType sort, ListSortDirection sortDirection)
        {
            listCollectionView.SortDescriptions.Clear();
            if (sort == LocationSortType.Name)
                listCollectionView.SortDescriptions.Add(new SortDescription("DisplayText", sortDirection));
            else if (sort == LocationSortType.AvailableSpace)
                listCollectionView.SortDescriptions.Add(new SortDescription("AvailableSpace", sortDirection));
            else if (sort == LocationSortType.Type)
            {
                // Sort first by type, then by name.
                listCollectionView.SortDescriptions.Add(new SortDescription("AbsoluteLocationTypeName", sortDirection));
                listCollectionView.SortDescriptions.Add(new SortDescription("DisplayText", sortDirection));
            }
            else
                throw new NotSupportedException("The specified location sort is not supported.");
        }

        /// <summary>
        /// Updates the sorting for the primary locations' collection view.
        /// </summary>
        private void updatePrimarySort()
        {
            this.updateSort(this.primaryLocationsListViewValue, this.PrimarySelectedSort, this.PrimaryCurrentSortDirection);
        }

        /// <summary>
        /// Updates the sorting for the secondary locations' collection view.
        /// </summary>
        private void updateSecondarySort()
        {
            this.updateSort(this.secondaryLocationsListViewValue, this.SecondarySelectedSort, this.SecondaryCurrentSortDirection);
        }

        /// <summary>
        /// Updates the sorting for the tertiary locations' collection view.
        /// </summary>
        private void updateTertiarySort()
        {
            this.updateSort(this.tertiaryLocationsListViewValue, this.TertiarySelectedSort, this.TertiaryCurrentSortDirection);
        }

        private void loadLocations(Location[] locations)
        {
            // Prepare locations.
            foreach (Location location in locations)
                this.locationViewModelsValue.Add(new LocationViewModel(location));

            // Prepare list views.
            this.primaryLocationsListViewValue = new ListCollectionView(this.locationViewModelsValue)
            {
                Filter = this.PrimaryLocationsListViewFilter,
                IsLiveFiltering = true,
            };
            this.secondaryLocationsListViewValue = new ListCollectionView(this.locationViewModelsValue)
            {
                Filter = this.SecondaryLocationsListViewFilter,
                IsLiveFiltering = true,
            };
            this.tertiaryLocationsListViewValue = new ListCollectionView(this.locationViewModelsValue)
            {
                Filter = this.TertiaryLocationsListViewFilter,
                IsLiveFiltering = true,
            };

            // Update sorts and filter lists. The secondary and tertiary lists have
            // to be sorted so that when a selection is made in the preceeding 
            // tier, the sort is already applied when the list is first shown.
            this.updatePrimarySort();
            this.updateSecondarySort();
            this.updateTertiarySort();
            this.refreshPrimaryListComponents();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="locations"></param>
        private void refreshLocationTypes(ObservableCollection<string> list, CollectionView locations)
        {
            List<string> locationTypes = new List<string>();
            foreach (LocationViewModel locationViewModel in locations)
            {
                string kind = locationViewModel.AbsoluteLocationTypeName;
                if (!locationTypes.Contains(kind))
                {
                    Debug.WriteLine($"refreshLocationTypes: adding - {kind}");
                    locationTypes.Add(kind);
                }
            }

            // Sort types.
            locationTypes.Sort();

            // Clear list and add sorted types.
            list.Clear();
            foreach (string locationType in locationTypes)
                list.Add(locationType);
        }

        private void refreshTertiaryListComponents()
        {
            // Refresh filters and search text.
            Debug.WriteLine("refreshTertiaryListComponents:");

            // Store previous selected type.
            string previousSelectedType = this.TertiaryLocationTypeFilter;
            Debug.WriteLine($"    Previous selection: {previousSelectedType}");

            // Remove filter.
            this.tertiaryLocationTypeFilterValue = "";

            // Refresh list.
            this.tertiaryLocationsListViewValue.Refresh();

            // Refresh location types for list.
            this.refreshLocationTypes(this.tertiaryLocationTypesValue, this.tertiaryLocationsListViewValue);

            // Enable/disable location type filter.
            this.TertiaryLocationTypesFilterIsEnabled = this.tertiaryLocationTypesValue.Count > 0;

            // Assign previously selected type if the new list contains it as well.
            if (!string.IsNullOrEmpty(previousSelectedType))
            {
                if (this.tertiaryLocationTypesValue.Contains(previousSelectedType))
                    this.TertiaryLocationTypeFilter = previousSelectedType;
            }
            else
            {
                // If there is only one type in the list, select it.
                if (this.tertiaryLocationTypesValue.Count == 1)
                    this.TertiaryLocationTypeFilter = this.tertiaryLocationTypesValue[0];
            }
        }

        private void refreshSecondaryListComponents()
        {
            // Refresh filters and search text.
            Debug.WriteLine("refreshSecondaryListComponents:");

            // Store previous selected type.
            string previousSelectedType = this.SecondaryLocationTypeFilter;
            Debug.WriteLine($"    Previous selection: {previousSelectedType}");

            // Remove filter.
            this.secondaryLocationTypeFilterValue = "";

            // Refresh list.
            this.secondaryLocationsListViewValue.Refresh();

            // Refresh location types for list.
            this.refreshLocationTypes(this.secondaryLocationTypesValue, this.secondaryLocationsListViewValue);

            // Enable/disable location type filter.
            this.SecondaryLocationTypesFilterIsEnabled = this.secondaryLocationTypesValue.Count > 0;

            // Assign previously selected type if the new list contains it as well.
            if (!string.IsNullOrEmpty(previousSelectedType))
            {
                if (this.secondaryLocationTypesValue.Contains(previousSelectedType))
                    this.SecondaryLocationTypeFilter = previousSelectedType;
            }
            else
            {
                // If there is only one type in the list, select it.
                if (this.secondaryLocationTypesValue.Count == 1)
                    this.SecondaryLocationTypeFilter = this.secondaryLocationTypesValue[0];
            }
        }

        private void refreshPrimaryListComponents()
        {
            // Refresh filters and search text.
            Debug.WriteLine("refreshPrimaryListComponents:");

            // Store previous selected type.
            string previousSelectedType = this.PrimaryLocationTypeFilter;
            Debug.WriteLine($"    Previous selection: {previousSelectedType}");

            // Remove filter.
            this.primaryLocationTypeFilterValue = "";

            // Refresh list.
            this.primaryLocationsListViewValue.Refresh();

            // Refresh location types for list.
            this.refreshLocationTypes(this.primaryLocationTypesValue, this.primaryLocationsListViewValue);

            // Enable/disable location type filter.
            this.PrimaryLocationTypesFilterIsEnabled = this.primaryLocationTypesValue.Count > 0;

            // Assign previously selected type if the new list contains it as well.
            if (!string.IsNullOrEmpty(previousSelectedType))
            {
                if (this.primaryLocationTypesValue.Contains(previousSelectedType))
                    this.PrimaryLocationTypeFilter = previousSelectedType;
            }
            else
            {
                // If there is only one type in the list, select it.
                if (this.primaryLocationTypesValue.Count == 1)
                    this.PrimaryLocationTypeFilter = this.primaryLocationTypesValue[0];
            }
        }

        private void applyFullLocationFilterToLocations()
        {
            this.PrimaryLocationsView.Refresh();
            this.SecondaryLocationsView.Refresh();
            this.TertiaryLocationsView.Refresh();
        }

        private void applyPrimaryLocationSearch()
        {
            this.PrimaryLocationsView.Refresh();
        }

        private void applySecondaryLocationSearch()
        {
            this.SecondaryLocationsView.Refresh();
        }

        private void applyTertiaryLocationSearch()
        {
            this.TertiaryLocationsView.Refresh();
        }

        private void applyPrimaryLocationTypeFilter()
        {
            this.PrimaryLocationsView.Refresh();
        }

        private void applySecondaryLocationTypeFilter()
        {
            this.SecondaryLocationsView.Refresh();
        }

        private void applyTertiaryLocationTypeFilter()
        {
            this.TertiaryLocationsView.Refresh();
        }

        private bool applySearchAndFiltersToLocation(LocationViewModel locationViewModel, string searchText, string locationTypeFilter)
        {
            // Location full or not filter trumps everything else, so do
            // it first.
            if (locationViewModel.AvailableSpace == 0 && this.hideFullLocationsValue)
                return false;

            // Location validity, as defined by multiple things, comes next.
            if (this.hideInvalidLocationsValue)
            {
                // TODO: Add filter for invalid locations.
            }

            // Location type filter trumps search, so do it next.
            if (!string.IsNullOrEmpty(locationTypeFilter))
            {
                // If location type doesn't match, then the location fails.
                if (locationViewModel.AbsoluteLocationTypeName != locationTypeFilter)
                    return false;
            }

            // Make sure this isn't whitespace since this is a "user-enterable" field.
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // We passed the location type filter. If this fails, the location doesn't
                // match.
                string lowercaseSearchText = searchText.ToLower();
                string lowercaseDisplayText = locationViewModel.DisplayText.ToLower();
                if (!lowercaseDisplayText.Contains(lowercaseSearchText))
                    return false;
            }

            // Return default.
            return true;
        }

        private void updateLocationsStatistics()
        {
            // Clear existing statistics.
            this.locationsStatisticsViewModelsValue.Clear();

            // Add based on current selections. If there isn't at least a primary
            // location selected, then assume nothing else is selected.
            if(this.SelectedPrimaryLocation != null)
            {
                // Add primary location.
                this.locationsStatisticsViewModelsValue.Add(new LocationStatisticsViewModel(this.SelectedPrimaryLocation.ToLocation()));

                // Likewise, if a secondary isn't selected, there's nothing else to do.
                if(this.SelectedSecondaryLocation != null)
                {
                    this.locationsStatisticsViewModelsValue.Add(new LocationStatisticsViewModel(this.SelectedSecondaryLocation.ToLocation()));
                    if (this.SelectedTertiaryLocation != null)
                        this.locationsStatisticsViewModelsValue.Add(new LocationStatisticsViewModel(this.SelectedTertiaryLocation.ToLocation()));
                }
            }

            // Force update.
            this.NotifyPropertyChanged(nameof(LocationSelectionDialogViewModel.LocationsStatisticsViewModels));
        }

        #region Collection View Filters

        private bool TertiaryLocationsListViewFilter(object obj)
        {
            // Make sure object is valid.
            LocationViewModel locationViewModel = obj as LocationViewModel;
            if (locationViewModel == null)
                return false;

            // Make sure a secondary location is selected.
            if (this.SelectedSecondaryLocation == null)
                return false;

            // Only want children of the selected secondary location.
            bool initialResult = locationViewModel.Parent == this.SelectedSecondaryLocation;

            // Apply any location type filter or search text.
            bool searchAndFilterResult = this.applySearchAndFiltersToLocation(locationViewModel, this.TertiarySearchText, this.TertiaryLocationTypeFilter);

            // Return result.
            return initialResult && searchAndFilterResult;
        }

        private bool SecondaryLocationsListViewFilter(object obj)
        {
            // Make sure object is valid.
            LocationViewModel locationViewModel = obj as LocationViewModel;
            if (locationViewModel == null)
                return false;

            // Make sure a primary location is selected.
            if (this.SelectedPrimaryLocation == null)
                return false;

            // Only want children of the selected primary location.
            bool initialResult = locationViewModel.Parent == this.SelectedPrimaryLocation;

            // Apply any location type filter or search text.
            bool searchAndFilterResult = this.applySearchAndFiltersToLocation(locationViewModel, this.SecondarySearchText, this.SecondaryLocationTypeFilter);

            // Return result.
            return initialResult && searchAndFilterResult;
        }

        private bool PrimaryLocationsListViewFilter(object obj)
        {
            // Make sure object is valid.
            LocationViewModel locationViewModel = obj as LocationViewModel;
            if (locationViewModel == null)
                return false;

            // We only want top-level locations, but we always want
            // these displayed.
            bool initialResult = locationViewModel.Parent == null;

            // Apply any location type filter or search text.
            bool searchAndFilterResult = this.applySearchAndFiltersToLocation(locationViewModel, this.PrimarySearchText, null);

            // Return result.
            return initialResult && searchAndFilterResult;
        }

        #endregion

        #endregion

        #region Command Handlers

        private void ApplyCommandExecute(object obj)
        {
            // Make sure this can be executed.
            if (!this.applyCommandValue.CanExecute(obj))
                return;

            // TODO: Add actual code.
            throw new NotImplementedException();
        }

        private bool ApplyCommandCanExecute(object obj)
        {
            return this.SelectedLocation != null;
        }

        private bool PrimarySortCommandCanExecute(LocationSortType obj)
        {
            // For now, always allow unless the application is busy.
            return !this.statusModel.IsBusy;
        }

        private void PrimarySortCommandExecute(LocationSortType obj)
        {
            // Make sure this is allowed.
            if (!this.PrimarySortCommand.CanExecute(obj))
                return;

            // Select sort and default order.
            this.PrimarySelectedSort = obj;
            this.PrimaryCurrentSortDirection = ListSortDirection.Ascending;

            // Apply sort.
            this.updatePrimarySort();

            // Update configuration.
            this.updateConfiguration();
        }

        private bool SecondarySortCommandCanExecute(LocationSortType obj)
        {
            // For now, always allow unless the application is busy.
            return !this.statusModel.IsBusy;
        }

        private void SecondarySortCommandExecute(LocationSortType obj)
        {
            // Make sure this is allowed.
            if (!this.SecondarySortCommand.CanExecute(obj))
                return;

            // Select sort and default order.
            this.SecondarySelectedSort = obj;
            this.SecondaryCurrentSortDirection = ListSortDirection.Ascending;

            // Apply sort.
            this.updateSecondarySort();

            // Update configuration.
            this.updateConfiguration();
        }

        private bool TertiarySortCommandCanExecute(LocationSortType obj)
        {
            // For now, always allow unless the application is busy.
            return !this.statusModel.IsBusy;
        }

        private void TertiarySortCommandExecute(LocationSortType obj)
        {
            // Make sure this is allowed.
            if (!this.TertiarySortCommand.CanExecute(obj))
                return;

            // Select sort and default order.
            this.TertiarySelectedSort = obj;
            this.TertiaryCurrentSortDirection = ListSortDirection.Ascending;

            // Apply sort.
            this.updateTertiarySort();

            // Update configuration.
            this.updateConfiguration();
        }

        private bool ReverseTertiarySortCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.statusModel.IsBusy;
        }

        private void ReverseTertiarySortCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.ReverseTertiarySortCommand.CanExecute(obj))
                return;

            // Store any current selection.
            LocationViewModel lastSelectedViewModel = this.SelectedTertiaryLocation;

            // Reverse sort direction.
            if (this.TertiaryCurrentSortDirection == ListSortDirection.Ascending)
                this.TertiaryCurrentSortDirection = ListSortDirection.Descending;
            else
                this.TertiaryCurrentSortDirection = ListSortDirection.Ascending;

            // Apply sort.
            this.updateTertiarySort();

            // Restore any previous selection.
            if (lastSelectedViewModel != null)
                this.SelectedTertiaryLocation = lastSelectedViewModel;

            // Update configuration.
            this.updateConfiguration();
        }

        private bool ReverseSecondarySortCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.statusModel.IsBusy;
        }

        private void ReverseSecondarySortCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.ReverseSecondarySortCommand.CanExecute(obj))
                return;

            // Store any current selection.
            LocationViewModel lastSelectedViewModel = this.SelectedSecondaryLocation;

            // Reverse sort direction.
            if (this.SecondaryCurrentSortDirection == ListSortDirection.Ascending)
                this.SecondaryCurrentSortDirection = ListSortDirection.Descending;
            else
                this.SecondaryCurrentSortDirection = ListSortDirection.Ascending;

            // Apply sort.
            this.updateSecondarySort();

            // Restore any previous selection.
            if (lastSelectedViewModel != null)
                this.SelectedSecondaryLocation = lastSelectedViewModel;

            // Update configuration.
            this.updateConfiguration();
        }

        private bool ReversePrimarySortCommandCanExecute(object obj)
        {
            // For now, always allow unless the application is busy.
            return !this.statusModel.IsBusy;
        }

        private void ReversePrimarySortCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.ReversePrimarySortCommand.CanExecute(obj))
                return;

            // Store any current selection.
            LocationViewModel lastSelectedViewModel = this.SelectedPrimaryLocation;

            // Reverse sort direction.
            if (this.PrimaryCurrentSortDirection == ListSortDirection.Ascending)
                this.PrimaryCurrentSortDirection = ListSortDirection.Descending;
            else
                this.PrimaryCurrentSortDirection = ListSortDirection.Ascending;

            // Apply sort.
            this.updatePrimarySort();

            // Restore any previous selection.
            if (lastSelectedViewModel != null)
                this.SelectedPrimaryLocation = lastSelectedViewModel;

            // Update configuration.
            this.updateConfiguration();
        }

        #endregion

        #region Event Handlers

        #endregion

        #region Public Methods

        #endregion

        #region Properties

        public ICollectionView PrimaryLocationsView
        {
            get => this.primaryLocationsListViewValue;
        }

        public ICollectionView SecondaryLocationsView
        {
            get => this.secondaryLocationsListViewValue;
        }

        public ICollectionView TertiaryLocationsView
        {
            get => this.tertiaryLocationsListViewValue;
        }

        public ObservableCollection<LocationStatisticsViewModel> LocationsStatisticsViewModels
        {
            get => this.locationsStatisticsViewModelsValue;
        }

        public ObservableCollection<string> PrimaryLocationTypes
        {
            get => this.primaryLocationTypesValue;
        }

        public ObservableCollection<string> SecondaryLocationTypes
        {
            get => this.secondaryLocationTypesValue;
        }

        public ObservableCollection<string> TertiaryLocationTypes
        {
            get => this.tertiaryLocationTypesValue;
        }

        public ICommand ApplyCommand
        {
            get => this.applyCommandValue;
        }

        public ICommand PrimarySortCommand
        {
            get => this.primarySortCommandValue;
        }

        public ICommand ReversePrimarySortCommand
        {
            get => this.reversePrimarySortCommandValue;
        }

        public ICommand SecondarySortCommand
        {
            get => this.secondarySortCommandValue;
        }

        public ICommand ReverseSecondarySortCommand
        {
            get => this.reverseSecondarySortCommandValue;
        }

        public ICommand TertiarySortCommand
        {
            get => this.tertiarySortCommandValue;
        }

        public ICommand ReverseTertiarySortCommand
        {
            get => this.reverseTertiarySortCommandValue;
        }

        public RollingStockViewModel RollingStock
        {
            get => this.rollingStockViewModelValue;
        }

        public string RollingStockIdentifier
        {
            get => this.rollingStockIdentifierValue;
        }

        public LocationSortType PrimarySelectedSort
        {
            get { return this.primarySelectedSortValue; }
            private set
            {
                if (this.primarySelectedSortValue != value)
                {
                    this.primarySelectedSortValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public LocationSortType SecondarySelectedSort
        {
            get { return this.secondarySelectedSortValue; }
            private set
            {
                if (this.secondarySelectedSortValue != value)
                {
                    this.secondarySelectedSortValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public LocationSortType TertiarySelectedSort
        {
            get { return this.tertiarySelectedSortValue; }
            private set
            {
                if (this.tertiarySelectedSortValue != value)
                {
                    this.tertiarySelectedSortValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ListSortDirection PrimaryCurrentSortDirection
        {
            get { return this.primaryCurrentSortDirectionValue; }
            private set
            {
                if (this.primaryCurrentSortDirectionValue != value)
                {
                    this.primaryCurrentSortDirectionValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ListSortDirection SecondaryCurrentSortDirection
        {
            get { return this.secondaryCurrentSortDirectionValue; }
            private set
            {
                if (this.secondaryCurrentSortDirectionValue != value)
                {
                    this.secondaryCurrentSortDirectionValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ListSortDirection TertiaryCurrentSortDirection
        {
            get { return this.tertiaryCurrentSortDirectionValue; }
            private set
            {
                if (this.tertiaryCurrentSortDirectionValue != value)
                {
                    this.tertiaryCurrentSortDirectionValue = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool HideFullLocations
        {
            get => this.hideFullLocationsValue;
            set
            {
                if (this.hideFullLocationsValue != value)
                {
                    // Assign value.
                    this.hideFullLocationsValue = value;

                    // Update filtering for locations. Note: under the hood, the
                    // collection view filter methods handle all aspects of this.
                    this.applyFullLocationFilterToLocations();

                    // Update configuration.
                    this.updateConfiguration();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool HideInvalidLocations
        {
            get => this.hideInvalidLocationsValue;
            set
            {
                if (this.hideInvalidLocationsValue != value)
                {
                    // Assign value.
                    this.hideInvalidLocationsValue = value;

                    // Update filtering for locations. Note: under the hood, the
                    // collection view filter methods handle all aspects of this.
                    this.applyFullLocationFilterToLocations();

                    // Update configuration.
                    this.updateConfiguration();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public LocationViewModel SelectedLocation
        {
            get => this.selectedLocationValue;
            set
            {
                if (this.selectedLocationValue != value)
                {
                    this.selectedLocationValue = value;
                    this.applyCommandValue.InvalidateCanExecuteChanged();
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string PrimarySearchText
        {
            get => this.primarySearchTextValue;
            set
            {
                if (this.primarySearchTextValue != value)
                {
                    // Assign value.
                    this.primarySearchTextValue = value;

                    // Apply search to primary locations.
                    this.applyPrimaryLocationSearch();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string SecondarySearchText
        {
            get => this.secondarySearchTextValue;
            set
            {
                if (this.secondarySearchTextValue != value)
                {
                    // Assign value.
                    this.secondarySearchTextValue = value;

                    // Apply search to secondary locations.
                    this.applySecondaryLocationSearch();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string TertiarySearchText
        {
            get => this.tertiarySearchTextValue;
            set
            {
                if (this.tertiarySearchTextValue != value)
                {
                    // Assign value.
                    this.tertiarySearchTextValue = value;

                    // Apply search to tertiary locations.
                    this.applyTertiaryLocationSearch();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string PrimaryLocationTypeFilter
        {
            get => this.primaryLocationTypeFilterValue;
            set
            {
                if (this.primaryLocationTypeFilterValue != value)
                {
                    // Assign value.
                    this.primaryLocationTypeFilterValue = value;

                    // Filter results based on selection.
                    this.applyPrimaryLocationTypeFilter();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string SecondaryLocationTypeFilter
        {
            get => this.secondaryLocationTypeFilterValue;
            set
            {
                if (this.secondaryLocationTypeFilterValue != value)
                {
                    // Assign value.
                    this.secondaryLocationTypeFilterValue = value;

                    // Filter results based on selection.
                    this.applySecondaryLocationTypeFilter();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string TertiaryLocationTypeFilter
        {
            get => this.tertiaryLocationTypeFilterValue;
            set
            {
                if (this.tertiaryLocationTypeFilterValue != value)
                {
                    // Assign value.
                    this.tertiaryLocationTypeFilterValue = value;

                    // Filter results based on selection.
                    this.applyTertiaryLocationTypeFilter();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool PrimaryLocationTypesFilterIsEnabled
        {
            get => this.primaryLocationTypesFilterIsEnabledValue;
            set
            {
                if (this.primaryLocationTypesFilterIsEnabledValue != value)
                {
                    // Assign value.
                    this.primaryLocationTypesFilterIsEnabledValue = value;

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool SecondaryLocationTypesFilterIsEnabled
        {
            get => this.secondaryLocationTypesFilterIsEnabledValue;
            set
            {
                if (this.secondaryLocationTypesFilterIsEnabledValue != value)
                {
                    // Assign value.
                    this.secondaryLocationTypesFilterIsEnabledValue = value;

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool TertiaryLocationTypesFilterIsEnabled
        {
            get => this.tertiaryLocationTypesFilterIsEnabledValue;
            set
            {
                if (this.tertiaryLocationTypesFilterIsEnabledValue != value)
                {
                    // Assign value.
                    this.tertiaryLocationTypesFilterIsEnabledValue = value;

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public LocationViewModel SelectedPrimaryLocation
        {
            get => this.selectedPrimaryLocationValue;
            set
            {
                if (this.selectedPrimaryLocationValue != value)
                {
                    this.selectedPrimaryLocationValue = value;

                    // De-select items in existing list.
                    this.SelectedSecondaryLocation = null;
                    this.SelectedTertiaryLocation = null;

                    // Refresh secondary and tertiary lists.
                    this.refreshSecondaryListComponents();
                    this.refreshTertiaryListComponents();

                    // Update locations statistics list.
                    this.updateLocationsStatistics();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public LocationViewModel SelectedSecondaryLocation
        {
            get => this.selectedSecondaryLocationValue;
            set
            {
                if (this.selectedSecondaryLocationValue != value)
                {
                    this.selectedSecondaryLocationValue = value;

                    // De-select items in existing list.
                    this.SelectedTertiaryLocation = null;

                    // Refresh tertiary list.
                    this.refreshTertiaryListComponents();

                    // Update locations statistics list.
                    this.updateLocationsStatistics();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public LocationViewModel SelectedTertiaryLocation
        {
            get => this.selectedTertiaryLocationValue;
            set
            {
                if (this.selectedTertiaryLocationValue != value)
                {
                    // Assign value.
                    this.selectedTertiaryLocationValue = value;

                    // Update locations statistics list.
                    this.updateLocationsStatistics();

                    // Notify change.
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
