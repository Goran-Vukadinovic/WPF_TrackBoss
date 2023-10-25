using Syncfusion.UI.Xaml.Diagram;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TrackBoss.Configuration;
using TrackBoss.Data;
using TrackBoss.Data.Enumerations;
using TrackBoss.Mvvm.Shared.Commands;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.Cars;
using TrackBoss.ViewModel.RollingStocks;
using TrackBoss.ViewModel.Shared;
using TrackBoss.ViewModel.Switchers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace TrackBoss.ViewModel.Yards
{
    public class YardItem
    {
        private IconType _iconType = IconType.Unknown;
        public IconType iconType
        {
            get { return this._iconType; }
            set
            {
                _iconType = value;

                string iconRelativePath = "";
                if (_iconType == IconType.MultiSwitcher)
                    iconRelativePath = "/Resources/Icons/Large/Black/multi-switcher.png";
                else if (_iconType == IconType.Track)
                    iconRelativePath = "/Resources/Icons/Large/Black/track.png";
                else if (_iconType == IconType.General)
                    iconRelativePath = "/Resources/Icons/Large/Black/settings.png";
                else if (_iconType == IconType.PrintScheme)
                    iconRelativePath = "/Resources/Icons/Large/Black/print.png";

                // Return Uri of icon.
                ImageUri = new Uri(iconRelativePath, UriKind.Relative);
            }
        }

        private Uri _ImageUri;
        public Uri ImageUri
        {
            get { return this._ImageUri; }
            set
            {
                _ImageUri = value;
            }
        }

        private string displayText = "";
        public string DisplayText
        {
            get { return this.displayText; }
            set
            {
                if (this.displayText != value)
                {
                    this.displayText = value;
                }
            }
        }

        private YardTrack yardTrack;
        public YardTrack YardTrack
        {
            get
            {
                return yardTrack;
            }
            set
            {
                yardTrack = value;
            }
        }

        private MultiSwitcher multiSwitcher;
        public MultiSwitcher MultiSwitcher
        {
            get
            {
                return multiSwitcher;
            }
            set
            {
                multiSwitcher = value;
            }
        }
    }

    public class YardViewModel : ChangeTrackingViewModel, IDisposable
    {
        #region Fields
        private Yard yard;

        private bool disposed;

        private TrackBossEntities trackBossConnection;

        private ObservableCollection<Car> cars;
        private string percent = "";
        private ObservableCollection<RollingStock> rollingStocks;
        private ObservableCollection<YardItem> yardItemsValue;
        private bool useSwitcher;

        private SwitcherViewModel switcherViewModelValue;

        private TrackViewModel trackViewModelValue;
        private MultiSwitcherViewModel multiSwitcherViewModelValue;


        private readonly RelayCommand updateRoadStatusCommandValue;
        private readonly RelayCommand updateCartypeStatusCommandValue;
        #endregion

        #region Constructors
        private YardViewModel()
        {
            // Prepare dictionaries.

            // Hook-up commands.
            this.updateRoadStatusCommandValue = new RelayCommand(this.UpdateRoadStatusCommandExecute, this.UpdateRoadStatusCommandCanExecute);
            this.updateCartypeStatusCommandValue = new RelayCommand(this.UpdateCartypeStatusCommandExecute, this.UpdateCartypeStatusCommandCanExecute);
        }

        public YardViewModel(Yard yard) : this()
        {
            // Validate parameter.
            if (yard == null)
                throw new ArgumentNullException(nameof(yard));

            // Validate components. All of these are REQUIRED for a city
            // object to be considered valid.
            if (yard.Site == null)
                throw new InvalidOperationException("The site object is invalid.");
            if (yard.Site.Location == null)
                throw new InvalidOperationException("The site's location object is invalid.");

            // Assign member fields.
            this.yard = yard;
            this.Site = new SiteViewModel(this.yard.Site);

            if(this.yard.Switcher != null)
            {
                this.switcherViewModelValue = new SwitcherViewModel(this.yard.Switcher);
                this.switcherViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;
                SwitcherView.ChangesMade += ChildViewModel_ChangesMade;
                this.useSwitcher = true;
            }

            yardItemsValue = new ObservableCollection<YardItem>();
            rollingStocks = new ObservableCollection<RollingStock>();
            cars = new ObservableCollection<Car>();

            // Initialize context object.
            this.trackBossConnection = new TrackBossEntities(Conductor.ConnectionString);
            initializeSupportingLists(this.trackBossConnection);


            // Initialize properties to their defaults.
            this.updateDisplayText();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        private async Task initializeSupportingLists(TrackBossEntities connection)
        {
            List<Car> carsArray = null;
            await Task.Run(() => carsArray = connection.Cars.ToList());

            // Add roll stocks.
            List<RollingStock> rollingStocksArray = null;
            await Task.Run(() => rollingStocksArray = connection.RollingStocks.ToList());

            foreach (Car car in carsArray)
            {
                foreach (var item in yard.YardTracks)
                {
                    var displayText = item.ToString();
                    if (car.RollingStock.RollingStockStatus.Location.Name.Equals(displayText))
                    {
                        this.cars.Add(car);
                    }
                }
            }

            percent = string.Format("{0}% ({1}/{2})", (cars.Count * 100 )/ carsArray.Count, cars.Count, carsArray.Count);
        }

        public YardViewModel(YardsDesignerViewModel designerViewModel, Yard yard) : this(yard)
        {
            // Validate designer.
            if (designerViewModel == null)
                throw new ArgumentNullException(nameof(designerViewModel));

            // Assign member fields.
            this.Designer = designerViewModel;
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
                    if(this.TrackViewModel != null)
                        this.TrackViewModel.ChangesMade -= ChildViewModel_ChangesMade;
                    if (this.switcherViewModelValue != null)
                        this.switcherViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;
                }
                this.disposed = true;
            }
        }
        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion

        #region public Methods
        public Yard ToYard()
        {
            return this.yard;
        }

        public Road[] GetAssignedRoads()
        {
            Track track = this.YardItems[_selectedIndex].YardTrack.Track;
            return track.TrackAssignedRoads.Select(x => x.Road).ToArray();
        }

        public CarType[] GetAssignedCartypes()
        {
            Track track = this.YardItems[_selectedIndex].YardTrack.Track;
            return track.TrackAssignedCarTypes.Select(x => x.CarType).ToArray();
        }
        #endregion

        #region Private Methods
        private void updateDisplayText()
        {
            this.DisplayText = this.yard.ToString();
        }

        private void UpdateSupportedList()
        {
            Road[] selectedRoads = this.GetAssignedRoads();
            Debug.WriteLine("Selected Roads count: " + selectedRoads.Length);

            // Update spurs list.
            this.Designer.updateRoadsTrackSelection(selectedRoads);


            CarType[] selectedCartypes = this.GetAssignedCartypes();
            Debug.WriteLine("Selected Cartypes count: " + selectedCartypes.Length);

            // Update spurs list.
            this.Designer.updateCartypesSelection(selectedCartypes);
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
            Track track = this.YardItems[_selectedIndex].YardTrack.Track;
            TrackAssignedRoad assignedRoad = track.TrackAssignedRoads.FirstOrDefault(x => x.Road == roadToUpdate.ToRoad());
            if (roadToUpdate.IsSelected && assignedRoad == null)
            {
                TrackAssignedRoad newAssignedRoad = new
                    TrackAssignedRoad
                {
                    Track = track,
                    Road = roadToUpdate.ToRoad(),
                };
                track.TrackAssignedRoads.Add(newAssignedRoad);
            }
            else if (!roadToUpdate.IsSelected && assignedRoad != null)
                track.TrackAssignedRoads.Remove(assignedRoad);

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

            Track track = this.YardItems[_selectedIndex].YardTrack.Track;
            TrackAssignedCarType assignedCartype = track.TrackAssignedCarTypes.FirstOrDefault(x => x.CarType == cartypeToUpdate.ToCarType());
            if (cartypeToUpdate.IsSelected && assignedCartype == null)
            {
                TrackAssignedCarType newAssignedCartype = new
                    TrackAssignedCarType
                {
                    Track = track,
                    CarType = cartypeToUpdate.ToCarType(),
                };
                track.TrackAssignedCarTypes.Add(newAssignedCartype);
            }
            else if (!cartypeToUpdate.IsSelected && assignedCartype != null)
                track.TrackAssignedCarTypes.Remove(assignedCartype);

            // Make sure to mark this object as having changes.
            this.HasChanges = true;
        }
        #endregion

        #region Properties
        public YardsDesignerViewModel Designer { get; }

        public SiteViewModel Site { get; }

        public ObservableCollection<YardItem> YardItems
        {
            get { return this.yardItemsValue; }
        }

        public bool IsSwitcher
        {
            get { return useSwitcher; }
            set
            {
                this.useSwitcher = value;

                if (this.useSwitcher)
                {
                    this.yard.Switcher = Switcher.Create();

                    var enumerator = Designer.PrintersValue.GetEnumerator();
                    enumerator.MoveNext(); // sets it to the first element
                    var firstElement = enumerator.Current;

                    this.yard.Switcher.AliasPrinter.Printer = (firstElement as PrinterViewModel).ToPrinter();
                    SwitcherView = new SwitcherViewModel(this.yard.Switcher);

                    SwitcherView.ChangesMade += ChildViewModel_ChangesMade;
                    this.NotifyPropertyChanged("SwitcherView");
                }
                else
                    this.yard.Switcher = null;

                // Notify.
                this.NotifyPropertyChanged();
            }
        }

        public long ID
        {
            get { return this.yard.ID; }
        }

        public string Name
        {
            get
            {
                return this.yard.Site.Location.Name;
            }
            set
            {
                this.yard.Site.Location.Name = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool? PassengerStation
        {
            get
            {
                return this.yard.PassengerStation;
            }
            set 
            {
                this.yard.PassengerStation = value;
                this.NotifyPropertyChanged();
            }
        }

        public City City
        {
            get
            {
                return this.yard.City;
            }
            set
            {
                this.yard.City = value;
                this.NotifyPropertyChanged();
            }
        }
        public Yard YARD
        {
            get { return yard; }
            set
            {
                yard = value;
                this.NotifyPropertyChanged(); 
            }
        }

        private int _selectedIndex = 0;
        public int selectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex != value && value >= 0)
                {
                    _selectedIndex = value;

                    if (this.YardItems[_selectedIndex].iconType == IconType.General)
                    {
                        TaskStatus = "General";

                        rollingStocks.Clear();
                    }
                    else if (this.YardItems[_selectedIndex].iconType == IconType.Track)
                    {
                        TaskStatus = "Refresh";
                        Track track = this.YardItems[_selectedIndex].YardTrack.Track;
                        this.trackViewModelValue = new TrackViewModel(track);
                        this.trackViewModelValue.ChangesMade += ChildViewModel_ChangesMade;

                        if (track.TrackTypeName.Equals("Thru"))
                            TaskStatus = "ThruTrack";
                        else if (track.TrackTypeName.Equals("Departure"))
                            TaskStatus = "DepartureTrack";
                        else if (track.TrackTypeName.Equals("Arrival"))
                            TaskStatus = "ArrivalTrack";
                        else if (track.TrackTypeName.Equals("Block"))
                        {
                            TaskStatus = "BlockTrack";
                        }
                        else if (track.TrackTypeName.Equals("Caboose"))
                            TaskStatus = "CabooseTrack";
                        else if (track.TrackTypeName.Equals("Engine Service"))
                            TaskStatus = "EngineServiceTrack";
                        else if (track.TrackTypeName.Equals("Interchange"))
                        {
                            TaskStatus = "InterchangeTrack";
                        }
                        else if (track.TrackTypeName.Equals("Passenger"))
                            TaskStatus = "PassengerTrack";
                        else if (track.TrackTypeName.Equals("Staging"))
                            TaskStatus = "StagingTrack";
                        else if (track.TrackTypeName.Equals("Storage"))
                            TaskStatus = "StorageTrack";
                        else if (track.TrackTypeName.Equals("Train Select"))
                            TaskStatus = "TrainAssignedTrack";


                        UpdateSupportedList();
                    }
                    else if (this.YardItems[_selectedIndex].iconType == IconType.MultiSwitcher)
                    {
                        TaskStatus = "Refresh";

                        MultiSwitcher multiSwitcher = YardItems[_selectedIndex].MultiSwitcher;
                        this.multiSwitcherViewModelValue = new MultiSwitcherViewModel(this.Designer, multiSwitcher);
                        this.multiSwitcherViewModelValue.ChangesMade += ChildViewModel_ChangesMade;
                        TaskStatus = "MultiSwitcher";
                    }
                    else if (this.YardItems[_selectedIndex].iconType == IconType.PrintScheme)
                    {
                        TaskStatus = "PrintScheme";
                    }

                    Designer.invalidateAllCommands_Refresh(); //this.NotifyPropertyChanged();
                }
            }
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

        public TrackViewModel TrackViewModel
        {
            get 
            { 
                return trackViewModelValue; 
            }
            set 
            { 
                trackViewModelValue = value; 
            }
        }

        public MultiSwitcherViewModel MultiSwitcherViewModel
        {
            get
            {
                return multiSwitcherViewModelValue;
            }
            set
            {
                multiSwitcherViewModelValue = value;
            }
        }

        public ObservableCollection<RollingStock> RollingStocks
        {
            get
            {
                return this.rollingStocks;
            }
        }

        public ObservableCollection<Car> Cars
        {
            get
            {
                return this.cars;
            }
        }

        public string Percent
        {
            get
            {
                return this.percent;
            }
        }

        public SwitcherViewModel SwitcherView
        {
            get { return this.switcherViewModelValue; }
            set { this.switcherViewModelValue = value; }
        }

        public bool EnableDeleteSubItem
        {
            get
            {
                bool ret = false;

                if (this.YardItems[_selectedIndex].iconType == IconType.General)
                    ret = false;
                else if (this.YardItems[_selectedIndex].iconType == IconType.PrintScheme)
                    ret = false;
                else
                    ret = true;

                return ret;
            }
        }

        public ICommand UpdateRoadStatusCommand
        {
            get { return this.updateRoadStatusCommandValue; }
        }

        public ICommand UpdateCartypeStatusCommand
        {
            get { return this.updateCartypeStatusCommandValue; }
        }

        #endregion
    }
}
