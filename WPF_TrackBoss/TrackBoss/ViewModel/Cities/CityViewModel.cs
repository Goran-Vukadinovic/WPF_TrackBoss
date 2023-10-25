using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Diagram;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TrackBoss.Data;
using TrackBoss.Data.Enumerations;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.Cabeese;
using TrackBoss.ViewModel.Cars;
using TrackBoss.ViewModel.RollingStocks;
using TrackBoss.ViewModel.Shared;
using TrackBoss.ViewModel.Switchers;
using TrackBoss.Mvvm.Shared.Commands;
using System.Diagnostics;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cities
{
    public class CityViewModel : ChangeTrackingViewModel, IDisposable
    {
        #region Fields

        private City city;

        private NeighborViewModel neighborViewModelValue;

        private ICollection<NeighborViewModel> neighborViewModelValues;

        private SubdivisionViewModel subdivisionViewModelValue;

        private CityGroupViewModel cityGroupViewModelValue;

        private SwitcherViewModel switcherViewModelValue;


        public SiteViewModel Site { get; }

        private ObservableCollection<SpurIndustryViewModel> SpurIndustryViewModelsValue;

        private ObservableCollection<IndustryViewModel> IndustryViewModelsValue;

        private ObservableCollection<SpurViewModel> SpurViewModelsValue;


        private IndustryViewModel industryViewModelValue;

        private SpurViewModel spurViewModelValue;

        private bool useSwitcher;

        private bool useOriginationInstructions;

        private bool useTerminationInstructions;

        private string originationInstructionTitle;

        public string originationInstruction;

        public string terminationInstructionTitle;

        public string terminationInstruction;

        private LocationAssignedInstructionSet originationInstructionSet;
        private LocationAssignedInstructionSet terminationInstructionSet;

        private PhotoViewModel photoViewModelValue;

        private bool disposed;


        private readonly RelayCommand updateSpurStatusCommandValue;
        private readonly RelayCommand updateRailroadStatusCommandValue;
        private readonly RelayCommand updateRailroadTrackStatusCommandValue;
        private readonly RelayCommand updateCartypeStatusCommandValue;
        private readonly RelayCommand updateCommodityStatusCommandValue;
        private readonly RelayCommand updateRoadStatusCommandValue;


        #endregion

        #region Constructors

        private CityViewModel()
        {
            // Prepare dictionaries.

            // Hook-up commands.
            this.updateSpurStatusCommandValue = new RelayCommand(this.UpdateSpurStatusCommandExecute, this.UpdateSpurStatusCommandCanExecute);
            this.updateRailroadStatusCommandValue = new RelayCommand(this.UpdateRailroadStatusCommandExecute, this.UpdateRailroadStatusCommandCanExecute);
            this.updateRailroadTrackStatusCommandValue = new RelayCommand(this.UpdateRailroadTrackStatusCommandExecute, this.UpdateRailroadTrackStatusCommandCanExecute);
            this.updateCartypeStatusCommandValue = new RelayCommand(this.UpdateCartypeStatusCommandExecute, this.UpdateCartypeStatusCommandCanExecute);
            this.updateCommodityStatusCommandValue = new RelayCommand(this.UpdateCommodityStatusCommandExecute, this.UpdateCommodityStatusCommandCanExecute);
            this.updateRoadStatusCommandValue = new RelayCommand(this.UpdateRoadStatusCommandExecute, this.UpdateRoadStatusCommandCanExecute);
        }


        public CityViewModel(City city) : this()
        {
            // Validate parameter.
            if (city == null)
                throw new ArgumentNullException(nameof(city));
            
            // Validate components. All of these are REQUIRED for a city
            // object to be considered valid.
            if (city.Site == null)
                throw new InvalidOperationException("The site object is invalid.");
            if (city.Site.Location == null)
                throw new InvalidOperationException("The site's location object is invalid.");

            // Assign member fields.
            this.city = city;
            this.Site = new SiteViewModel(this.city.Site);
            //Site.Location.ChangesMade += this.ChildViewModel_ChangesMade;

            this.IndustryViewModelsValue = new ObservableCollection<IndustryViewModel>();
            foreach (var item in city.Industries)
            {
                IndustryViewModel industryViewModel = new IndustryViewModel(this.Designer, item);
                IndustryViewModelsValue.Add(industryViewModel);
            }

            this.SpurViewModelsValue = new ObservableCollection<SpurViewModel>();
            foreach (var item in city.Spurs)
            {
                SpurViewModel spurViewModel = new SpurViewModel(this.Designer, item);
                SpurViewModelsValue.Add(spurViewModel);
            }


            if (this.city.Subdivision != null)
                this.subdivisionViewModelValue = new SubdivisionViewModel(this.city.Subdivision);
            if (this.city.CityGroup != null)
                this.cityGroupViewModelValue = new CityGroupViewModel(this.city.CityGroup);
            if (this.city.Neighbor != null)
            {
                this.neighborViewModelValue = new NeighborViewModel(this.city.Neighbor);
                //this.neighborViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;
            }

            if (this.city.Switcher != null)
            {
                this.switcherViewModelValue = new SwitcherViewModel(this.city.Switcher);
                this.switcherViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;
                this.useSwitcher = true;
            }

            if (this.city.Site.Location.LocationAssignedInstructionSets != null)
            {
                foreach (LocationAssignedInstructionSet instructionSet in this.city.Site.Location.LocationAssignedInstructionSets)
                {
                    if (instructionSet.InstructionSet.InstructionSetTypesEnum == (int)InstructionSetType.Origin)
                    {
                        this.useOriginationInstructions = true;

                        this.originationInstructionTitle = instructionSet.InstructionSet.Title;
                        this.originationInstruction = instructionSet.InstructionSet.Instructions;

                        originationInstructionSet = instructionSet;
                    }

                    if (instructionSet.InstructionSet.InstructionSetTypesEnum == (int)InstructionSetType.Termination)
                    {
                        this.useTerminationInstructions = true;

                        this.terminationInstructionTitle = instructionSet.InstructionSet.Title;
                        this.terminationInstruction = instructionSet.InstructionSet.Instructions;

                        terminationInstructionSet = instructionSet;
                    }
                }
            }

            // TODO: What else?

            if (this.city.Neighbors != null)
            {
                this.neighborViewModelValues = new HashSet<NeighborViewModel>();
                foreach (var item in this.city.Neighbors)
                {
                    var itemViewModel = new NeighborViewModel(item);
                    this.neighborViewModelValues.Add(itemViewModel);

                    if (this.Neighbor != null &&
                        itemViewModel.Site.Location.Name == this.Neighbor.Site.Location.Name)
                        Neighbor = itemViewModel;
                }
            }

            if (this.city.Site.Location.Photo != null)
                this.photoViewModelValue = new PhotoViewModel(this.city.Site.Location.Photo);
            else
            {
                Photo photo = new Photo();
                this.city.Site.Location.Photo = photo;
                this.photoViewModelValue = new PhotoViewModel(photo);
            }

            // Hook-up event handlers.
            this.photoViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;

            // Initialize properties to their defaults.
            this.updateDisplayText();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        public CityViewModel(CitiesDesignerViewModel designerViewModel, City city) : this(city)
        {
            // Validate designer.
            if (designerViewModel == null)
                throw new ArgumentNullException(nameof(designerViewModel));

            // Assign member fields.
            this.Designer = designerViewModel;

            this.SpurIndustryViewModelsValue = new ObservableCollection<SpurIndustryViewModel>();
            this.IndustryViewModelsValue = new ObservableCollection<IndustryViewModel>();
            this.SpurViewModelsValue = new ObservableCollection<SpurViewModel>();

            for (int i = 0; i < city.Spurs.Count; i++)
            {
                SpurViewModel spurViewModel = new SpurViewModel(this.Designer, city.Spurs.ToList()[i]);
                SpurViewModelsValue.Add(spurViewModel);
            }

            for (int i = 0; i < city.Industries.Count; i++)
            {
                IndustryViewModel industryViewModel = new IndustryViewModel(this.Designer, city.Industries.ToList()[i]);
                IndustryViewModelsValue.Add(industryViewModel);
            }
        }

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Unhook event handlers.
                    if (this.Site != null)
                        Site.Location.ChangesMade -= this.ChildViewModel_ChangesMade;
                    //if (this.Neighbor != null)
                    //    neighborViewModelValue.Site.Location.ChangesMade -= this.ChildViewModel_ChangesMade;
                    if (this.subdivisionViewModelValue != null)
                        this.subdivisionViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;
                    if (this.cityGroupViewModelValue != null)
                        this.cityGroupViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;

                    if (this.switcherViewModelValue != null)
                        this.switcherViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;

                    // Dispose of child ViewModels which need disposing.
                    // Currently, none require this.                        
                }
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);

            // Unhook event handlers.
            this.photoViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;
            this.photoViewModelValue.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        private void updateDisplayText()
        {
            this.DisplayText = this.city.ToString();
        }


        private bool UpdateSpurStatusCommandCanExecute(object obj)
        {
            // Validate and determine if command is allowed.
            SpurViewModel spur = obj as SpurViewModel;
            return (spur != null);
        }

        private void UpdateSpurStatusCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.UpdateSpurStatusCommand.CanExecute(obj))
                return;

            // Update status of spur.
            SpurViewModel spurToUpdate = obj as SpurViewModel;

            IndustryAssignedSpur assignedSpur = this.industryViewModel.CurIndustry.IndustryAssignedSpurs.FirstOrDefault(x => x.Spur == spurToUpdate.CurSpur);
            if (spurToUpdate.IsSelected && assignedSpur == null)
            {
                IndustryAssignedSpur newAssignedSpur = new
                    IndustryAssignedSpur
                {
                    Industry = this.industryViewModel.CurIndustry,
                    Spur = spurToUpdate.ToSpur(),
                };
                this.industryViewModel.CurIndustry.IndustryAssignedSpurs.Add(newAssignedSpur);
            }
            else if (!spurToUpdate.IsSelected && assignedSpur != null)
                this.industryViewModel.CurIndustry.IndustryAssignedSpurs.Remove(assignedSpur);

            // Make sure to mark this object as having changes.
            this.HasChanges = true;
        }

        private bool UpdateRailroadStatusCommandCanExecute(object obj)
        {
            // Validate and determine if command is allowed.
            RailroadViewModel railRoad = obj as RailroadViewModel;
            return (railRoad != null);
        }

        private void UpdateRailroadStatusCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.UpdateRailroadStatusCommand.CanExecute(obj))
                return;

            // Update status of railroad.
            RailroadViewModel railroadToUpdate = obj as RailroadViewModel;

            IndustryAssignedRailroad assignedRailroad = this.industryViewModel.CurIndustry.IndustryAssignedRailroads.FirstOrDefault(x => x.Railroad == railroadToUpdate.ToRailroad());
            if (railroadToUpdate.IsSelected && assignedRailroad == null)
            {
                IndustryAssignedRailroad newAssignedRailroad = new
                    IndustryAssignedRailroad
                {
                    Industry = this.industryViewModel.CurIndustry,
                    Railroad = railroadToUpdate.ToRailroad(),
                };
                this.industryViewModel.CurIndustry.IndustryAssignedRailroads.Add(newAssignedRailroad);
            }
            else if (!railroadToUpdate.IsSelected && assignedRailroad != null)
                this.industryViewModel.CurIndustry.IndustryAssignedRailroads.Remove(assignedRailroad);

            // Make sure to mark this object as having changes.
            this.HasChanges = true;
        }

        private bool UpdateRailroadTrackStatusCommandCanExecute(object obj)
        {
            // Validate and determine if command is allowed.
            RailroadViewModel railRoad = obj as RailroadViewModel;
            return (railRoad != null);
        }

        private void UpdateRailroadTrackStatusCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.UpdateRailroadTrackStatusCommand.CanExecute(obj))
                return;

            // Update status of railroad.
            RailroadViewModel railroadToUpdate = obj as RailroadViewModel;

            TrackAssignedRailroad assignedRailroad = this.spurViewModel.CurSpur.Track.TrackAssignedRailroads.FirstOrDefault(x => x.Railroad == railroadToUpdate.ToRailroad());
            if (railroadToUpdate.IsSelected && assignedRailroad == null)
            {
                TrackAssignedRailroad newAssignedRailroad = new
                    TrackAssignedRailroad
                {
                    Track = this.spurViewModel.CurSpur.Track,
                    Railroad = railroadToUpdate.ToRailroad(),
                };
                this.spurViewModel.CurSpur.Track.TrackAssignedRailroads.Add(newAssignedRailroad);
            }
            else if (!railroadToUpdate.IsSelected && assignedRailroad != null)
                this.spurViewModel.CurSpur.Track.TrackAssignedRailroads.Remove(assignedRailroad);

            // Make sure to mark this object as having changes.
            this.HasChanges = true;
        }

        private bool UpdateCartypeStatusCommandCanExecute(object obj)
        {
            // Validate and determine if command is allowed.
            CarTypeViewModel carType = obj as CarTypeViewModel;
            return (carType != null);
        }

        private void UpdateCartypeStatusCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.UpdateCartypeStatusCommand.CanExecute(obj))
                return;

            // Update status of cartype.
            CarTypeViewModel cartypeToUpdate = obj as CarTypeViewModel;

            TrackAssignedCarType assignedCartype = this.spurViewModel.CurSpur.Track.TrackAssignedCarTypes.FirstOrDefault(x => x.CarType == cartypeToUpdate.ToCarType());
            if (cartypeToUpdate.IsSelected && assignedCartype == null)
            {
                TrackAssignedCarType newAssignedCartype = new
                    TrackAssignedCarType
                {
                    Track = this.spurViewModel.CurSpur.Track,
                    CarType = cartypeToUpdate.ToCarType(),
                };
                this.spurViewModel.CurSpur.Track.TrackAssignedCarTypes.Add(newAssignedCartype);
            }
            else if (!cartypeToUpdate.IsSelected && assignedCartype != null)
                this.spurViewModel.CurSpur.Track.TrackAssignedCarTypes.Remove(assignedCartype);

            // Make sure to mark this object as having changes.
            this.HasChanges = true;
        }

        private bool UpdateCommodityStatusCommandCanExecute(object obj)
        {
            // Validate and determine if command is allowed.
            CommodityViewModel commodity = obj as CommodityViewModel;
            return (commodity != null);
        }

        private void UpdateCommodityStatusCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.UpdateCommodityStatusCommand.CanExecute(obj))
                return;

            // Update status of commodity.
            CommodityViewModel commodityToUpdate = obj as CommodityViewModel;

            TrackAssignedCommodity assignedCommodity = this.spurViewModel.CurSpur.Track.TrackAssignedCommodities.FirstOrDefault(x => x.Commodity == commodityToUpdate.ToCommodity());
            if (commodityToUpdate.IsSelected && assignedCommodity == null)
            {
                TrackAssignedCommodity newAssignedCommodity = new
                    TrackAssignedCommodity
                {
                    Track = this.spurViewModel.CurSpur.Track,
                    Commodity = commodityToUpdate.ToCommodity(),
                };
                this.spurViewModel.CurSpur.Track.TrackAssignedCommodities.Add(newAssignedCommodity);
            }
            else if (!commodityToUpdate.IsSelected && assignedCommodity != null)
                this.spurViewModel.CurSpur.Track.TrackAssignedCommodities.Remove(assignedCommodity);

            // Make sure to mark this object as having changes.
            this.HasChanges = true;
        }

        private bool UpdateRoadStatusCommandCanExecute(object obj)
        {
            // Validate and determine if command is allowed.
            RoadViewModel road = obj as RoadViewModel;
            return (road != null);
        }

        private void UpdateRoadStatusCommandExecute(object obj)
        {
            // Make sure this is allowed.
            if (!this.UpdateRoadStatusCommand.CanExecute(obj))
                return;

            // Update status of road.
            RoadViewModel roadToUpdate = obj as RoadViewModel;

            TrackAssignedRoad assignedRoad = this.spurViewModel.CurSpur.Track.TrackAssignedRoads.FirstOrDefault(x => x.Road == roadToUpdate.ToRoad());
            if (roadToUpdate.IsSelected && assignedRoad == null)
            {
                TrackAssignedRoad newAssignedRoad = new
                    TrackAssignedRoad
                {
                    Track = this.spurViewModel.CurSpur.Track,
                    Road = roadToUpdate.ToRoad(),
                };
                this.spurViewModel.CurSpur.Track.TrackAssignedRoads.Add(newAssignedRoad);
            }
            else if (!roadToUpdate.IsSelected && assignedRoad != null)
                this.spurViewModel.CurSpur.Track.TrackAssignedRoads.Remove(assignedRoad);

            // Make sure to mark this object as having changes.
            this.HasChanges = true;
        }
        #endregion

        #region Event Handlers

        private void ChildViewModel_ChangesMade(object sender, EventArgs<string> e)
        {
            // Mark this ViewModel as having changes.
            this.HasChanges = true;

            // Raise the changes made event.
            this.OnChangesMade(e.Value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the city data object encapsulated by this ViewModel.
        /// </summary>
        /// <returns>The encapsulated city data object.</returns>
        public City ToCity()
        {
            return this.city;
        }

        public Spur[] GetAssignedSpurs()
        {
            return this.industryViewModel.CurIndustry.IndustryAssignedSpurs.Select(x => x.Spur).ToArray();
        }

        public Railroad[] GetAssignedRailroads()
        {
            return this.industryViewModel.CurIndustry.IndustryAssignedRailroads.Select(x => x.Railroad).ToArray();
        }

        public CarType[] GetAssignedCartypes()
        {
            return this.spurViewModel.CurSpur.Track.TrackAssignedCarTypes.Select(x => x.CarType).ToArray();
        }

        public Commodity[] GetAssignedCommodities()
        {
            return this.spurViewModel.CurSpur.Track.TrackAssignedCommodities.Select(x => x.Commodity).ToArray();
        }

        public Railroad[] GetAssignedRailroadsTrack()
        {
            return this.spurViewModel.CurSpur.Track.TrackAssignedRailroads.Select(x => x.Railroad).ToArray();
        }

        public Road[] GetAssignedRoads()
        {
            return this.spurViewModel.CurSpur.Track.TrackAssignedRoads.Select(x => x.Road).ToArray();
        }
        

        #endregion

        #region Properties

        public CitiesDesignerViewModel Designer { get; }

        public long ID
        {
            get { return this.city.ID; }
        }

        public string UniqueId
        {
            get { return this.city.UniqueId; }
        }

        #region Child ViewModels


        public CityGroupViewModel CityGroup
        {
            get { return this.cityGroupViewModelValue; }
            set
            {
                if (value == null)
                    return;

                if (this.cityGroupViewModelValue != value)
                {
                    this.city.CityGroup = value.ToCityGroup();
                    this.cityGroupViewModelValue = value;

                    // Notify.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public SubdivisionViewModel Subdivision
        {
            get { return this.subdivisionViewModelValue; }
            set
            {
                if (value == null)
                    return;

                if (this.subdivisionViewModelValue != value)
                {
                    this.subdivisionViewModelValue = value;

                    // Notify.
                    this.NotifyPropertyChanged();
                }
            }
        }

        public NeighborViewModel Neighbor
        {
            get
            {
                return this.neighborViewModelValue;
            }
            set
            {
                if (value == null)
                    return;

                if (this.neighborViewModelValue != value)
                {
                    this.city.Neighbor = value.ToCity();
                    this.neighborViewModelValue = value;


                    // Notify.
                    //this.PropertyChanged(this, new PropertyChangedEventArgs("Neighbor"));
                    this.NotifyPropertyChanged();
                }
            }
        }

        public ICollection<NeighborViewModel> Neighbors
        {
            get
            {
                return this.neighborViewModelValues;
            }
        }

        public ObservableCollection<SpurIndustryViewModel> SpurIndustryViewModels
        {
            get { return this.SpurIndustryViewModelsValue; }
        }

        public ObservableCollection<IndustryViewModel> IndustryViewModels
        {
            get { return this.IndustryViewModelsValue; }
        }

        public ObservableCollection<SpurViewModel> SpurViewModels
        {
            get { return this.SpurViewModelsValue; }
        }

        public IndustryViewModel industryViewModel
        {
            get { return this.industryViewModelValue; }
            set { this.industryViewModelValue = value; }
        }

        public SpurViewModel spurViewModel
        {
            get { return this.spurViewModelValue; }
            set { this.spurViewModelValue = value; }
        }

        private string taskStatus = "General";
        public string TaskStatus
        {
            get { return taskStatus; }
            set
            {
                taskStatus = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool IsDivisionpoint
        {
            get { return this.city.DivisionPoint; }
            set
            {
                this.city.DivisionPoint = value;
                // Notify.
                this.NotifyPropertyChanged();
            }
        }

        public bool IsSwitcher
        {
            get { return useSwitcher; }
            set
            {
                this.useSwitcher = value;

                if (this.useSwitcher)
                {
                    this.city.Switcher = Switcher.Create();

                    var enumerator = Designer.PrintersValue.GetEnumerator();
                    enumerator.MoveNext(); // sets it to the first element
                    var firstElement = enumerator.Current;

                    this.city.Switcher.AliasPrinter.Printer = (firstElement as PrinterViewModel).ToPrinter();
                    SwitcherView = new SwitcherViewModel(this.city.Switcher);

                    SwitcherView.ChangesMade += ChildViewModel_ChangesMade;
                    this.NotifyPropertyChanged("SwitcherView");
                }
                else
                    this.city.Switcher = null;

                // Notify.
                this.NotifyPropertyChanged();
            }
        }


        public SwitcherViewModel SwitcherView
        {
            get { return this.switcherViewModelValue; }
            set { this.switcherViewModelValue = value; }
        }

        private void UpdateInstruction(bool bOrigination)
        {
            if(bOrigination)
            {
                if (originationInstructionSet.InstructionSet.Title != this.originationInstructionTitle)
                    originationInstructionSet.InstructionSet.Title = this.originationInstructionTitle;
                if (originationInstructionSet.InstructionSet.Instructions != this.originationInstruction)
                    originationInstructionSet.InstructionSet.Instructions = this.originationInstruction;
            }
            else
            {
                if (terminationInstructionSet.InstructionSet.Title != this.terminationInstructionTitle)
                    terminationInstructionSet.InstructionSet.Title = this.terminationInstructionTitle;
                if (terminationInstructionSet.InstructionSet.Instructions != this.terminationInstruction)
                    terminationInstructionSet.InstructionSet.Instructions = this.terminationInstruction;
            }
        }

        public bool UseOriginationInstructions
        {
            get { return useOriginationInstructions; }
            set
            {
                this.useOriginationInstructions = value;

                if (this.useOriginationInstructions == true)
                {
                    InstructionSet newInstructionSet = new InstructionSet();
                    newInstructionSet.InstructionSetType = InstructionSetType.Origin;

                    LocationAssignedInstructionSet newSet = new LocationAssignedInstructionSet();
                    newSet.InstructionSet = newInstructionSet;
                    newSet.Location = this.city.Site.Location;

                    this.city.Site.Location.LocationAssignedInstructionSets.Add(newSet);
                }
                else
                {
                    if(originationInstructionSet != null)
                        originationInstructionSet.Location = null;
                }

                this.NotifyPropertyChanged();
            }
        }

        public bool UseTerminationInstructions
        {
            get { return useTerminationInstructions; }
            set
            {
                this.useTerminationInstructions = value;

                if (this.useTerminationInstructions == true)
                {
                    InstructionSet newInstructionSet = new InstructionSet();
                    newInstructionSet.InstructionSetType = InstructionSetType.Termination;

                    LocationAssignedInstructionSet newSet = new LocationAssignedInstructionSet();
                    newSet.InstructionSet = newInstructionSet;
                    newSet.Location = this.city.Site.Location;

                    this.city.Site.Location.LocationAssignedInstructionSets.Add(newSet);
                }
                else
                {
                    if(terminationInstructionSet != null)
                        terminationInstructionSet.Location = null;
                }

                this.NotifyPropertyChanged();
            }
        }

        public string OriginationInstructionTitle
        {
            get { return originationInstructionTitle; }
            set
            {
                this.originationInstructionTitle = value;
                UpdateInstruction(true);
                this.NotifyPropertyChanged();
            }
        }

        public string OriginationInstruction
        {
            get { return originationInstruction; }
            set
            {
                this.originationInstruction = value;
                UpdateInstruction(true);
                this.NotifyPropertyChanged();
            }
        }

        public string TerminationInstructionTitle
        {
            get { return terminationInstructionTitle; }
            set
            {
                this.terminationInstructionTitle = value;
                UpdateInstruction(false);
                this.NotifyPropertyChanged();
            }
        }

        public string TerminationInstruction
        {
            get { return terminationInstruction; }
            set
            {
                this.terminationInstruction = value;
                UpdateInstruction(false);
                this.NotifyPropertyChanged();
            }
        }

        public bool EnableDeleteSubItem
        {
            get
            {
                bool ret = false;

                if (this.SelectedSubItem.iconType == IconType.General)
                    ret = false;
                else if (this.SelectedSubItem.iconType == IconType.Industry)
                    ret = true;
                else if (this.SelectedSubItem.iconType == IconType.Spur)
                    ret = true;
                else if (this.SelectedSubItem.iconType == IconType.PrintScheme)
                    ret = false;

                return ret;
            }
        }

        private SpurIndustryViewModel selectedSubItem;
        public SpurIndustryViewModel SelectedSubItem
        {
            get { return selectedSubItem; }
            set
            {
                if (value != null && selectedSubItem != value)
                {
                    selectedSubItem = value;

                    if (selectedSubItem.iconType == IconType.General)
                    {
                        TaskStatus = "General";
                    }
                    else if (selectedSubItem.iconType == IconType.Industry)
                    {
                        TaskStatus = "Refresh";
                        this.industryViewModel = SelectedSubItem.industryViewModel;

                        // Get selected industries.
                        if (this.industryViewModel != null)
                        {
                            Spur[] selectedSpurs = this.GetAssignedSpurs();
                            Debug.WriteLine("Selected spurs count: " + selectedSpurs.Length);

                            // Update spurs list.
                            this.Designer.updateSpursSelection(selectedSpurs);


                            Railroad[] selectedRailroads = this.GetAssignedRailroads();
                            Debug.WriteLine("Selected railroads count: " + selectedRailroads.Length);

                            // Update spurs list.
                            this.Designer.updateRailroadsSelection(selectedRailroads);
                        }

                        TaskStatus = "Industry";
                    }
                    else if (selectedSubItem.iconType == IconType.Spur)
                    {
                        TaskStatus = "Refresh";
                        this.spurViewModel = SelectedSubItem.spurViewModel;

                        if (spurViewModel != null)
                        {
                            CarType[] selectedCartypes = this.GetAssignedCartypes();
                            Debug.WriteLine("Selected cartypes count: " + selectedCartypes.Length);

                            // Update cartype list.
                            this.Designer.updateCartypesSelection(selectedCartypes);


                            Commodity[] selectedCommodity = this.GetAssignedCommodities();
                            Debug.WriteLine("Selected Commodities count: " + selectedCommodity.Length);

                            // Update commodity list.
                            this.Designer.updateCommoditiesSelection(selectedCommodity);


                            Railroad[] selectedRailroads = this.GetAssignedRailroadsTrack();
                            Debug.WriteLine("Selected railroads count: " + selectedRailroads.Length);

                            // Update railroad list.
                            this.Designer.updateRailroadsTrackSelection(selectedRailroads);


                            Road[] selectedRoads = this.GetAssignedRoads();
                            Debug.WriteLine("Selected Roads count: " + selectedRoads.Length);

                            // Update spurs list.
                            this.Designer.updateRoadsTrackSelection(selectedRoads);
                        }

                        TaskStatus = "Spur";
                    }
                    else if (selectedSubItem.iconType == IconType.PrintScheme)
                    {
                        TaskStatus = "PrintScheme";
                    }

                    Designer.invalidateAllCommands_Refresh();//this.NotifyPropertyChanged();
                }
            }
        }

        private int _selectedIndex = 0;
        public int selectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                /*if (_selectedIndex != value && value >= 0)
                {
                    _selectedIndex = value;

                    if (this.SpurIndustryViewModels[_selectedIndex].iconType == IconType.General)
                    {
                        TaskStatus = "General";
                    }
                    else if (this.SpurIndustryViewModels[_selectedIndex].iconType == IconType.Industry)
                    {
                        TaskStatus = "Refresh";
                        this.industryViewModel = this.IndustryViewModels.ToArray()[selectedIndex-1];
                        TaskStatus = "Industry";
                    }
                    else if (this.SpurIndustryViewModels[_selectedIndex].iconType == IconType.Spur)
                    {
                        TaskStatus = "Refresh";
                        this.spurViewModel = this.SpurViewModels.ToArray()[selectedIndex-1];
                        TaskStatus = "Spur";
                    }
                    else if (this.SpurIndustryViewModels[_selectedIndex].iconType == IconType.PrintScheme)
                    {
                        TaskStatus = "PrintScheme";
                    }

                    Designer.invalidateAllCommands_Refresh();//this.NotifyPropertyChanged();
                }*/
            }
        }

        public PhotoViewModel Photo
        {
            get
            {
                return photoViewModelValue;
            }
            set
            {
                photoViewModelValue = value;
            }
        }

        public long? IndustryServicePriority
        {
            get { return this.industryViewModel.CurIndustry.ServicePriority; }
            set
            {
                this.industryViewModel.CurIndustry.ServicePriority = value;
                this.NotifyPropertyChanged();
            }
        }

        public long? SpurServicePriority
        {
            get { return this.spurViewModel.CurSpur.ServicePriority; }
            set
            {
                this.spurViewModel.CurSpur.ServicePriority = value;
                this.NotifyPropertyChanged();
            }
        }

        public ICommand UpdateSpurStatusCommand
        {
            get { return this.updateSpurStatusCommandValue; }
        }

        public ICommand UpdateRailroadStatusCommand
        {
            get { return this.updateRailroadStatusCommandValue; }
        }

        public ICommand UpdateRailroadTrackStatusCommand
        {
            get { return this.updateRailroadTrackStatusCommandValue; }
        }
        

        public ICommand UpdateCartypeStatusCommand
        {
            get { return this.updateCartypeStatusCommandValue; }
        }

        public ICommand UpdateCommodityStatusCommand
        {
            get { return this.updateCommodityStatusCommandValue; }
        }

        public ICommand UpdateRoadStatusCommand
        {
            get { return this.updateRoadStatusCommandValue; }
        }
        

        #endregion

        #endregion
    }
}
