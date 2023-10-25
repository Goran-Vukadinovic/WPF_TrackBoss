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
using TrackBoss.Data.Enumerations;
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
using TrackBoss.ViewModel.RollingStocks;
using TrackBoss.ViewModel.Shared;
using TrackBoss.ViewModel.Shared.Messages;
using TrackBoss.Windsor;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cabeese
{
    public class CabeeseDesignerViewModel : GenericListBasedViewModel<CabooseViewModel>, IGenericSortEnabledViewModel<RollingStockSortType>, IInitializableViewModel
    {
        #region Fields

        private readonly RelayCommand saveAndAddNewCommandValue;

        private readonly RelayCommand duplicateCommandValue;

        private readonly RelayCommand<RollingStockSortType> sortCommandValue;

        private readonly RelayCommand reverseSortCommandValue;

        private readonly AsyncCommand initializeCommandValue;

        private readonly RelayCommand addCupolaTypeCommandValue;

        private readonly RelayCommand addRoadCommandValue;

        private readonly RelayCommand addOwnerCommandValue;

        private readonly DispatcherTimer validationTimer;

        private Validator<CabooseViewModel> validator;

        private Conductor conductor;

        private List<CabooseViewModel> viewModelsHistory;

        private ObservableCollection<DataValidationResultViewModel<CabooseViewModel>> validationResultsValue;

        private ObservableCollection<ColorItemViewModel> colors;

        private ObservableCollection<long> lengthsValue;

        private ObservableCollection<CupolaTypeViewModel> cupolaTypesValue;

        private ObservableCollection<RoadViewModel> roadsValue;

        private ObservableCollection<OwnerViewModel> ownersValue;

        private ObservableCollection<LocationViewModel> locationsValue;

        private ListCollectionView cupolaTypesViewValue;

        private ListCollectionView roadsViewValue;

        private ListCollectionView ownersViewValue;

        private ListCollectionView lengthsViewValue;

        private ListCollectionView viewModelsViewValue;

        private CupolaTypeViewModel newCupolaTypeValue;

        private RoadViewModel newRoadValue;

        private OwnerViewModel newOwnerValue;

        private RollingStockSortType selectedSort;

        private ListSortDirection currentSortDirection;

        private GridLength currentListSplitterPosition;

        private GridLength currentErrorWindowSplitterPosition;
        
        private bool hasChanges;

        private TrackBossEntities trackBossConnection;

        private CabeeseDesignerLayoutConfiguration configuration;

        private string filterString;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Default constructor. Initializes fields and prepares this object
        /// for use.
        /// </summary>
        public CabeeseDesignerViewModel()
        {
            // Prepare the conductor.
            this.conductor = CastleWindsor.Default.Resolve<Conductor>();
            this.conductor.Preferences.PreferencesChanged += this.Preferences_PreferencesChanged;
            this.validationTimer = new DispatcherTimer(DispatcherPriority.Input);
            this.validationTimer.Interval = new TimeSpan(0, 0, Conductor.DefaultValidationInterval);
            this.validationTimer.Tick += this.ValidationTimer_Tick;

            // Initialize fields.
            this.viewModelsHistory = new List<CabooseViewModel>();
            this.currentListSplitterPosition = new GridLength(Conductor.DefaultDesignerListWidth);
            this.currentErrorWindowSplitterPosition = new GridLength(Conductor.DefaultDesignerErrorWindowHeight);
            this.selectedSort = RollingStockSortType.Number;
            this.currentSortDirection = ListSortDirection.Ascending;
            this.cupolaTypesValue = new ObservableCollection<CupolaTypeViewModel>();
            this.roadsValue = new ObservableCollection<RoadViewModel>();
            this.ownersValue = new ObservableCollection<OwnerViewModel>();
            this.locationsValue = new ObservableCollection<LocationViewModel>();

            // Hookup command handlers.
            this.sortCommandValue = new RelayCommand<RollingStockSortType>(this.SortCommandExecute, this.SortCommandCanExecute);
            this.reverseSortCommandValue = new RelayCommand(this.ReverseSortCommandExecute, this.ReverseSortCommandCanExecute);
            this.initializeCommandValue = new AsyncCommand(this.InitializeCommandExecute, this.InitializeCommandCanExecute);
            this.saveAndAddNewCommandValue = new RelayCommand(this.SaveAndAddNewCommandExecute, this.SaveAndAddNewCommandCanExecute);
            this.duplicateCommandValue = new RelayCommand(this.DuplicateCommandExecute, this.DuplicateCommandCanExecute);
            this.addCupolaTypeCommandValue = new RelayCommand(this.AddCupolaTypeCommandExecute, this.AddCupolaTypeCommandCanExecute);
            this.addRoadCommandValue = new RelayCommand(this.AddRoadCommandExecute, this.AddRoadCommandCanExecute);
            this.addOwnerCommandValue = new RelayCommand(this.AddOwnerCommandExecute, this.AddOwnerCommandCanExecute);

            SelectionChangedCommand = new RelayCommand<ListBox>(SelectionChanged);

            // Load lists.
            this.colors = new ObservableCollection<ColorItemViewModel>();
            this.loadDefaultColors();
            this.loadDefaultLengths();

            // Use stored or default layout configuration, whichever is applicable.
            CabeeseDesignerLayoutConfiguration defaultConfiguration = new CabeeseDesignerLayoutConfiguration();
            this.configuration = this.conductor.Preferences.LoadLayoutConfiguration<CabeeseDesignerLayoutConfiguration>(defaultConfiguration.Name);
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
                Title = string.Format("{0} - Cabooses", this.conductor.Name),
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

        protected override void deleteCommandExecuteHelper(object obj)
        {
            //// TODO: Test. Remove!
            //CabooseDataObject cabooseDataObject = (CabooseDataObject)this.SelectedViewModel.ToCaboose();
            //string xml = GenericSerializer<CabooseDataObject>.Serialize(cabooseDataObject);
            //File.WriteAllText(@"C:\Development\TrackBoss\Test\Output\caboose.xml", xml);
            //return;

            // Get selected caboose.
            CabooseViewModel selectedCabooseViewModel = this.SelectedViewModel;
            Caboose selectedCaboose = SelectedViewModel.ToCaboose();

            // Verify the user wants to delete this caboose.
            ShowMessageBoxMessage message = new ShowMessageBoxMessage();
            message.Message = string.Format(
                                "This will delete the currently selected caboose:\n\n{0}\n\nAre you sure you want to do this?", selectedCaboose);
            message.Button = MessageBoxButton.YesNo;
            message.Icon = MessageBoxImage.Exclamation;
            message.Title = this.conductor.Name;
            Messenger.Default.Send<ShowMessageBoxMessage>(message);

            // Check result.
            if (message.Result == MessageBoxResult.Yes)
            {
                // Remove selection and remove ViewModel from list.
                this.SelectedViewModel = null;
                this.ViewModels.Remove(selectedCabooseViewModel);

                // Disconnect change event. This event should not fire
                // for deleted ViewModels.
                selectedCabooseViewModel.ChangesMade -= this.CabooseViewModel_ChangesMade;

                // Remove validation results pertaining to this caboose, if any.
                for (int i = this.validationResultsValue.Count - 1; i > -1; i--)
                {
                    DataValidationResultViewModel<CabooseViewModel> result = this.validationResultsValue[i];
                    if (result.Source == selectedCabooseViewModel)
                        this.validationResultsValue.RemoveAt(i);
                }

                // Remove history item, if applicable.
                this.conductor.HistoryManager.RemoveHistoryItem(selectedCabooseViewModel.UniqueId);

                // Dispose of removed ViewModel.
                selectedCabooseViewModel.Dispose();

                // Get caboose being removed.
                Caboose cabooseToRemove = selectedCabooseViewModel.ToCaboose();

                // Perform clean-up on caboose.
                //await cabooseToRemove.RollingStock.Photo.Clear();

                // Remove caboose from data context. If this is a new caboose, there
                // will be no currently assigned ID and nothing else to do.
                if (cabooseToRemove.ID != 0)
                {
                    if (this.trackBossConnection.Cabooses.Remove(cabooseToRemove) == null)
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
            // Create new caboose.
            Caboose newCaboose = Caboose.Create();

            // Set default value for dwell time.
            newCaboose.RollingStock.RollingStockStatus.DwellTimeData.CurrentValue = this.conductor.Settings.Operations.GlobalDwellTimeMethodValue;

            // Create placeholder number for caboose.
            newCaboose.RollingStock.Number = "(New Caboose)";

            // Add new caboose to list.
            this.addNewCabooseHelper(newCaboose);

            // Queue validation.
            this.validationTimer.Stop();
            this.validationTimer.Start();
        }

        protected override async void saveCommandExecuteHelper(object obj)
        {
            // Perform save.
            await this.saveAsync();

            // Commit modifications and additions (deletions are handled on-the-fly).
            this.commitHistory();

            // Store selected caboose so it can be reselected after save
            // (if applicable).
            long lastSelectedCabooseId = -1;
            if (this.SelectedViewModel != null)
                lastSelectedCabooseId = this.SelectedViewModel.ToCaboose().ID;

            // Perform cleanup.
            this.performViewModelCleanUp();

            // Destroy connection object.
            this.trackBossConnection.Dispose();
            this.trackBossConnection = null;

            // Reinitialize the data set.
            if (this.InitializeCommand.CanExecute(null))
                await this.initializeAsync();

            // Reselect last selected caboose, if applicable.
            if (lastSelectedCabooseId != -1)
                this.selectCabooseById(lastSelectedCabooseId);

            // Clear changes.
            this.hasChanges = false;

            // Invalidate commands.
            this.invalidateAllCommands();
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

            // Invalidate commands which are selection dependent.
            ((RelayCommand)this.DeleteCommand).InvalidateCanExecuteChanged();
            ((RelayCommand)this.DuplicateCommand).InvalidateCanExecuteChanged();
        }

        #endregion

        #region Command Handlers
        public ObservableCollection<CabooseViewModel> selectedViewModels;
        private void SelectionChanged(ListBox listBox)
        {
            if (selectedViewModels == null)
                selectedViewModels = new ObservableCollection<CabooseViewModel>();

            if (listBox.SelectedItems.Count > 1)
            {
                selectedViewModels.Clear();
                foreach (CabooseViewModel selectedViewModel in listBox.SelectedItems)
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

            // Select for the current caboose.
            this.SelectedViewModel.RollingStock.Owner = this.newOwnerValue;

            // Add to main list.
            this.ownersValue.Add(this.newOwnerValue);

            // Prepare next new owner.
            this.initializeNewOwner();

            // Notify property.
            this.NotifyPropertyChanged(nameof(CabeeseDesignerViewModel.NewOwner));
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

            // Select for the current caboose.
            this.SelectedViewModel.RollingStock.Road = this.newRoadValue;

            // Add to main list.
            this.roadsValue.Add(this.newRoadValue);

            // Prepare next new road.
            this.initializeNewRoad();

            // Notify property.
            this.NotifyPropertyChanged(nameof(CabeeseDesignerViewModel.NewRoad));
        }

        private bool AddCupolaTypeCommandCanExecute(object obj)
        {
            // Determine if this should be allowed.
            return this.SelectedViewModel != null && this.newCupolaTypeValue != null && !string.IsNullOrWhiteSpace(this.newCupolaTypeValue.Name);
        }

        private void AddCupolaTypeCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.AddCupolaTypeCommand.CanExecute(obj))
                return;

            // Unhook changed handler.
            this.newCupolaTypeValue.PropertyChanged -= this.NewCupolaTypeValue_PropertyChanged;

            // Select for the current caboose.
            this.SelectedViewModel.CupolaType = this.newCupolaTypeValue;

            // Add to main list.
            this.cupolaTypesValue.Add(this.newCupolaTypeValue);

            // Prepare next new road.
            this.initializeNewCupolaType();

            // Notify property.
            this.NotifyPropertyChanged(nameof(CabeeseDesignerViewModel.NewCupolaType));
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
            CabooseViewModel lastSelectedViewModel = this.SelectedViewModel;

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

            // If there are cabooses and none are selected, then
            // select the first one.
            if (this.SelectedViewModel == null && this.ViewModels.Count > 0)
                this.SelectedViewModel = (CabooseViewModel)this.ViewModelsView.CurrentItem;
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
            this.StatusModel.SetStatus("Duplicating caboose. Please wait.");

            try
            {
                // Get selected caboose.
                Caboose selectedCaboose = this.SelectedViewModel.ToCaboose();

                // Duplicate.
                Caboose clonedCaboose = await Caboose.Clone(selectedCaboose);
                clonedCaboose.RollingStock.IncrementNumber();

                // Add to list.
                this.addNewCabooseHelper(clonedCaboose);

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

            // Add new caboose.
            this.NewCommand.Execute(obj);
        }

        #endregion

        #region Private Methods

        private bool ViewModelsViewFilter(object obj)
        {
            // Get object.
            CabooseViewModel cabooseViewModel = obj as CabooseViewModel;
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

                string formattedText = cabooseViewModel.ToString(formatString);
                formattedText = formattedText.ToLower();

                string caseInsensitiveFilterString = this.filterString.ToLower();

                return formattedText.Contains(caseInsensitiveFilterString);
            }

            // By default, allow always.
            return true;
        }

        private void addNewCabooseHelper(Caboose newCaboose)
        {
            // Create ViewModel and attach event handlers.
            CabooseViewModel newCabooseViewModel = new CabooseViewModel(this, newCaboose);
            newCabooseViewModel.ChangesMade += this.CabooseViewModel_ChangesMade;

            // Add to list.
            this.ViewModels.Add(newCabooseViewModel);

            // Select new caboose.
            this.SelectedViewModel = newCabooseViewModel;

            // Mark designer as having changes.
            this.hasChanges = true;

            // Invalidate commands.
            this.invalidateAllCommands();
        }

        private void commitHistory()
        {
            // Get list of cabooses with changes. Order them by change date/time stamp.
            // NOTE: not using OrderByDescending because history manager reverses
            // them.
            IEnumerable<CabooseViewModel> modifiedViewModels = this.ViewModels.Where(x => x.HasChanges).OrderBy(x => x.LastChanged);
            foreach (CabooseViewModel viewModel in modifiedViewModels)
            {
                // Create history item.
                HistoryItem newHistoryItem = new HistoryItem
                {
                    UniqueIdentifier = viewModel.UniqueId,
                    ItemType = HistoryItemType.Caboose,
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
                    this.viewModelsViewValue.CustomSort = new CabooseViewModelComparer();
                else
                    this.viewModelsViewValue.CustomSort = new CabooseViewModelReverseComparer();
            }
            else if (this.SelectedSort == RollingStockSortType.CreatedDate)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("ID", this.currentSortDirection));
            else if (this.SelectedSort == RollingStockSortType.Road)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("RollingStock.Road", this.currentSortDirection));
            else if (this.SelectedSort == RollingStockSortType.Type)
                this.viewModelsViewValue.SortDescriptions.Add(new SortDescription("CupolaType", this.currentSortDirection));
            else
                throw new NotSupportedException("The specified reverse sort is unsupported.");
        }

        /// <summary>
        /// Locates a CabooseViewModel by the ID of the caboose is represents.
        /// </summary>
        /// <param name="id">ID of the caboose to select.</param>
        private void selectCabooseById(long id)
        {
            foreach (CabooseViewModel cabooseViewModel in this.ViewModels)
            {
                if (cabooseViewModel.ID == id)
                {
                    this.SelectedViewModel = cabooseViewModel;
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
                                                       "Any caboose with validation errors will NOT be used during build processes. Do you wish to proceed?");
                ShowHasValidationErrorsDialogMessage message = new ShowHasValidationErrorsDialogMessage(errorsDialogViewModel);

                // Display dialog.
                Messenger.Default.Send<ShowHasValidationErrorsDialogMessage>(message);

                // Get whether or not dialog should be shown again. Only do this if
                // the user did not click "Cancel" on the dialog. Setting a setting
                // if the user clicked "Cancel" on something would be weird.
                if (!errorsDialogViewModel.Cancelled)
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
                List<DataValidationResult<CabooseViewModel>> validationResults = new List<DataValidationResult<CabooseViewModel>>();
                foreach (CabooseViewModel cabooseViewModel in this.ViewModels)
                {
                    List<DataValidationResult<CabooseViewModel>> results = this.validator.Validate(cabooseViewModel);
                    validationResults.AddRange(results);
                }

                // Clear any existing results.
                if (this.validationResultsValue != null)
                    this.validationResultsValue.Clear();
                else
                    this.validationResultsValue = new ObservableCollection<DataValidationResultViewModel<CabooseViewModel>>();

                // If there are results, then perform addition.
                if (validationResults.Count > 0)
                {
                    Debug.WriteLine("Adding validation results.");

                    // This MUST happen on the UI thread.
                    Action addResults = () =>
                    {
                        // Add current results.
                        foreach (DataValidationResult<CabooseViewModel> result in validationResults)
                            this.validationResultsValue.Add(new DataValidationResultViewModel<CabooseViewModel>(result));
                    };
                    App.Current.Dispatcher.Invoke(addResults);
                }

                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(CabeeseDesignerViewModel.ValidationResults));
            };
            await Task.Run(performValidation);
        }

        private async Task initializeCabooses(TrackBossEntities connection)
        {
            // Fetch list of cabooses.
            List<Caboose> cabooses = null;
            await Task.Run(() => cabooses = connection.Cabooses.ToList());

            // Add cabooses to list.
            if (cabooses != null)
            {
                foreach (Caboose caboose in cabooses)
                {
                    // Prepare new ViewModel.
                    CabooseViewModel newCabooseViewModel = new CabooseViewModel(this, caboose);
                    newCabooseViewModel.ChangesMade += this.CabooseViewModel_ChangesMade;
                    this.ViewModels.Add(newCabooseViewModel);
                }
            }
        }

        private async Task initializeSupportingLists(TrackBossEntities connection)
        {
            // Fetch owners, roads, caboose types, locations, and commodities.
            List<Owner> owners = null;
            Debug.WriteLine("Initializing owners");
            await Task.Run(() => owners = connection.Owners.ToList());

            List<Road> roads = null;
            Debug.WriteLine("Initializing roads");
            await Task.Run(() => roads = connection.Roads.ToList());

            List<CupolaType> cupolaTypes = null;
            Debug.WriteLine("Initializing caboose types");
            await Task.Run(() => cupolaTypes = connection.CupolaTypes.ToList());

            List<Location> locations = null;
            Debug.WriteLine("Initializing locations");
            await Task.Run(() => locations = this.initializeLocations(connection));
            
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

            // Add cupola types.
            this.cupolaTypesValue = new ObservableCollection<CupolaTypeViewModel>();
            foreach (CupolaType cabooseType in cupolaTypes)
                this.cupolaTypesValue.Add(new CupolaTypeViewModel(cabooseType));
            this.cupolaTypesViewValue = new ListCollectionView(this.cupolaTypesValue);
            this.cupolaTypesViewValue.SortDescriptions.Add(new SortDescription(nameof(CupolaTypeViewModel.DisplayText), ListSortDirection.Ascending));

            // Add locations.
            this.locationsValue = new ObservableCollection<LocationViewModel>();
            foreach (Location location in locations)
                this.locationsValue.Add(new LocationViewModel(location));

            // Initialize new items for direct editing.
            this.initializeNewOwner();
            this.initializeNewRoad();
            this.initializeNewCupolaType();
        }

        private List<Location> initializeLocations(TrackBossEntities connection)
        {
            List<Location> results = new List<Location>();

            foreach (Location location in connection.Locations)
            {
                if (location.IsCity || location.IsYard)
                    results.Add(location);
                else if (location.IsTrack)
                {
                    Track track = location.Tracks.FirstOrDefault();
                    if (track.TrackType == TrackType.Caboose || track.TrackType == TrackType.Staging)
                        results.Add(location);
                }
            }

            return results;
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
                this.StatusModel.SetStatus("Loading cabooses... please wait.");

                // Initialize context object.
                this.trackBossConnection = new TrackBossEntities(Conductor.ConnectionString);

                // Disconnect any prior event handlers attached to the supporting
                // lists.
                Debug.WriteLine("Detaching event handlers from supporting lists.");
                
                // Initialize all supporting lists.
                Debug.WriteLine("Initializing supporting lists");
                await this.initializeSupportingLists(this.trackBossConnection);
                
                // Initialize the list of cabooses.
                Debug.WriteLine("Initializing cabooses");
                await this.initializeCabooses(this.trackBossConnection);

                // Load user lists dependent on rolling stock. This MUST BE
                // done after the cabooses are loaded.
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

                // Select the first caboose.
                this.viewModelsViewValue.MoveCurrentToFirst();

                // Finally, load validation rule set.
                this.StatusModel.SetStatus("Validating cabooses... please wait.");
                await this.initializeValidator();
                await this.performValidation();

                // Must be called because binding occurs before data is present.
                this.NotifyPropertyChanged(nameof(CabeeseDesignerViewModel.ViewModelsView));
                this.NotifyPropertyChanged(nameof(CabeeseDesignerViewModel.ValidationResults));
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
                this.validator = new Validator<CabooseViewModel>(FileUtilities.GetFullpath(SpecialFileName.CabooseValidationRuleSet));
            };
            await Task.Run(initializeValidator);
        }

        private void saveCabooses(TrackBossEntities connection)
        {
            // New cabooses will have changes but will also have an ID of zero.
            IEnumerable<CabooseViewModel> newViewModels = this.ViewModels.Where(x => x.ID == 0);
            IEnumerable<Caboose> allCabooses = this.ViewModels.Select(x => x.ToCaboose());
            IEnumerable<Caboose> newCabooses = newViewModels.Select(x => x.ToCaboose());

            // Add new cabooses.
            foreach (Caboose newCaboose in newCabooses)
                connection.Cabooses.Add(newCaboose);
        }

        private void saveOwners(TrackBossEntities connection)
        {
            // New cabooses will have changes but will also have an ID of zero.
            IEnumerable<Owner> modifiedOwners = this.ownersValue.Where(x => x.HasChanges).Select(x => x.ToOwner());
            IEnumerable<Owner> newOwners = modifiedOwners.Where(x => x.ID == 0);
            modifiedOwners = modifiedOwners.Except(newOwners);

            // Add new owners.
            foreach (Owner newOwner in newOwners)
                connection.Owners.Add(newOwner);
        }

        private void saveRoads(TrackBossEntities connection)
        {
            // New cabooses will have changes but will also have an ID of zero.
            IEnumerable<Road> modifiedRoads = this.roadsValue.Where(x => x.HasChanges).Select(x => x.ToRoad());
            IEnumerable<Road> newRoads = modifiedRoads.Where(x => x.ID == 0);
            modifiedRoads = modifiedRoads.Except(newRoads);

            // Add new owners.
            foreach (Road newRoad in newRoads)
                connection.Roads.Add(newRoad);
        }

        private void saveCupolaTypes(TrackBossEntities connection)
        {
            // New cabooses will have changes but will also have an ID of zero.
            IEnumerable<CupolaType> modifiedCupolaTypes = this.cupolaTypesValue.Where(x => x.HasChanges).Select(x => x.ToCupolaType());
            IEnumerable<CupolaType> newCupolaTypes = modifiedCupolaTypes.Where(x => x.ID == 0);
            modifiedCupolaTypes = modifiedCupolaTypes.Except(newCupolaTypes);

            // Add new owners.
            foreach (CupolaType newCupolaType in newCupolaTypes)
                connection.CupolaTypes.Add(newCupolaType);
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
                    this.saveCupolaTypes(this.trackBossConnection);

                    // Save cabooses.
                    this.saveCabooses(this.trackBossConnection);

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
            foreach (CabooseViewModel cabooseViewModel in this.ViewModels)
            {
                cabooseViewModel.ChangesMade -= this.CabooseViewModel_ChangesMade;
                cabooseViewModel.Dispose();
            }
            this.ViewModels.Clear();

            // Dispose of existing supporting lists.
            this.ownersValue.Clear();
            this.roadsValue.Clear();
            this.cupolaTypesValue.Clear();

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

        private void initializeNewCupolaType()
        {
            // Create new object.
            CupolaType newCupolaType = new CupolaType();

            // Create ViewModel and hook-up.
            this.newCupolaTypeValue = new CupolaTypeViewModel(newCupolaType);
            this.newCupolaTypeValue.PropertyChanged += this.NewCupolaTypeValue_PropertyChanged;
        }

        #endregion

        #region Event Handlers

        private void Preferences_PreferencesChanged(object sender, EventArgs<string> e)
        {
            Debug.WriteLine(e.Value);
        }

        private async void ValidationTimer_Tick(object sender, EventArgs e)
        {
            // Deactivate timer.
            this.validationTimer.Stop();

            // Perform validation.
            await this.performValidation();
        }

        private void CabooseViewModel_ChangesMade(object sender, EventArgs e)
        {
            // Validate parameters.
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

        private void NewCupolaTypeValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Invalidate command.
            this.addCupolaTypeCommandValue.InvalidateCanExecuteChanged();
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
            if (message.Source is CabooseViewModel)
            {
                // Mark message as handled.
                message.Handled = true;

                // Unset any existing filter so all cabeese are visible.
                this.FilterString = null;

                // Attempt to locate caboose. This is technically redundant, but 
                // prevents the case that this object is handling a message
                // for a prior or disposed object (I hope).
                CabooseViewModel sourceCabooseViewModel = (CabooseViewModel)message.Source;
                int index = this.ViewModels.IndexOf(sourceCabooseViewModel);
                if (index == -1)
                    throw new InvalidOperationException("The caboose is not in the list.");

                // Select the caboose.
                this.SelectedViewModel = sourceCabooseViewModel;

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
            if (message.HistoryItem.ItemType == HistoryItemType.Caboose)
            {
                // Attempt to locate item.
                foreach (CabooseViewModel cabooseViewModel in this.ViewModels)
                {
                    if (cabooseViewModel.UniqueId == (string)message.HistoryItem.UniqueIdentifier)
                    {
                        // Set message as handled.
                        message.Handled = true;

                        // Select item.
                        this.SelectedViewModel = cabooseViewModel;

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

        public ObservableCollection<DataValidationResultViewModel<CabooseViewModel>> ValidationResults
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

        public ICollectionView CupolaTypesView
        {
            get { return this.cupolaTypesViewValue; }
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

        public ICommand AddCupolaTypeCommand
        {
            get { return this.addCupolaTypeCommandValue; }
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
                if (this.filterString != value)
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

        public CupolaTypeViewModel NewCupolaType
        {
            get { return this.newCupolaTypeValue; }
            set
            {
                if (this.newCupolaTypeValue != value)
                {
                    this.newCupolaTypeValue = value;
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