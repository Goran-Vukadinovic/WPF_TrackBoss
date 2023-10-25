using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using TrackBoss.Data;
using TrackBoss.ViewModel.Cabeese;
using TrackBoss.ViewModel.Cars;
using TrackBoss.ViewModel.Cities;
using TrackBoss.ViewModel.Locomotives;
using TrackBoss.ViewModel.Shared;
using TrackBoss.ViewModel.WheelReport;
using TrackBoss.ViewModel.Yards;

namespace TrackBoss.ViewModel.WheelReport
{
    internal class WheelReportViewModel : ChangeTrackingViewModel, IDisposable
    {
        public WheelReportDesignerViewModel Designer { get; }
        private TrackBossEntities connection;
        private bool disposed;

        OverviewViewModel overviewModelValue;
        ObservableCollection<CarViewModel> carViewModelsValue;
        ObservableCollection<CabooseViewModel> cabooseViewModelsValue;
        ObservableCollection<LocomotiveViewModel> locomotiveViewModelsValue;
        ObservableCollection<CityViewModel> cityViewModelsValue;
        ObservableCollection<SpurViewModel> spurViewModelsValue;
        ObservableCollection<YardViewModel> yardViewModelsValue;
        ObservableCollection<YardTrack> yardTracksValue;

        CarViewModel selectedCarViewModelValue = null;
        CabooseViewModel selectedCabooseViewModelValue = null;
        LocomotiveViewModel selectedLocomotiveViewModelValue = null;
        CityViewModel selectedCityViewModelValue = null;
        SpurViewModel selectedSpurViewModelValue = null;
        YardViewModel selectedYardViewModelValue = null;
        YardTrack selectedYardTracksValue = null;

        public CarsDesignerViewModel carsDesignerViewModel;
        public CabeeseDesignerViewModel caboosesDesignerViewModel;
        public LocomotivesDesignerViewModel locomotivesDesignerViewModel;
        public CitiesDesignerViewModel citiesDesignerViewModel;
        public YardsDesignerViewModel yardsDesignerViewModel;

        private WheelReportViewModel()
        {
            // Prepare dictionaries.

            // Hook-up commands.
        }

        public WheelReportViewModel(WheelReportDesignerViewModel designerViewModel, TrackBossEntities trackBossConnection)
        {
            Designer = designerViewModel;
            this.connection = trackBossConnection;

            initializeSupportingLists();
        }

        private async Task initializeSupportingLists()
        {
            overviewModelValue = new OverviewViewModel();

            carViewModelsValue = new ObservableCollection<CarViewModel>();
            cabooseViewModelsValue = new ObservableCollection<CabooseViewModel>();
            locomotiveViewModelsValue = new ObservableCollection<LocomotiveViewModel>();
            cityViewModelsValue = new ObservableCollection<CityViewModel>();
            spurViewModelsValue = new ObservableCollection<SpurViewModel>();
            yardViewModelsValue = new ObservableCollection<YardViewModel>();
            yardTracksValue = new ObservableCollection<YardTrack>();

            carsDesignerViewModel = new CarsDesignerViewModel();
            // await carsDesignerViewModel.InitializeCommand.ExecuteAsync();
            //carViewModels = carsDesignerViewModel.ViewModels;
            //carsDesignerViewModel.childChangesMade += Designer.WheelReportViewModel_ChangesMade;

            List<Car> cars = null;
            await Task.Run(() => cars = connection.Cars.ToList());
            foreach (Car car in cars)
            {
                CarViewModel carViewModel = new CarViewModel(carsDesignerViewModel, car);
                carViewModelsValue.Add(carViewModel);

                carViewModel.ChangesMade += Designer.WheelReportViewModel_ChangesMade;
            }

            caboosesDesignerViewModel = new CabeeseDesignerViewModel();
            List<Caboose> cabooses = null;
            await Task.Run(() => cabooses = connection.Cabooses.ToList());
            foreach (Caboose caboose in cabooses)
            {
                CabooseViewModel cabooseViewModel = new CabooseViewModel(caboosesDesignerViewModel, caboose);
                cabooseViewModelsValue.Add(cabooseViewModel);

                cabooseViewModel.ChangesMade += Designer.WheelReportViewModel_ChangesMade;
            }

            locomotivesDesignerViewModel = new LocomotivesDesignerViewModel();
            List<Locomotive> locomotives = null;
            await Task.Run(() => locomotives = connection.Locomotives.ToList());
            foreach (Locomotive locomotive in locomotives)
            {
                LocomotiveViewModel locomotiveViewModel = new LocomotiveViewModel(locomotivesDesignerViewModel, locomotive);
                locomotiveViewModelsValue.Add(locomotiveViewModel);

                locomotiveViewModel.ChangesMade += Designer.WheelReportViewModel_ChangesMade;
            }

            citiesDesignerViewModel = new CitiesDesignerViewModel();
            List<City> cities = null;
            await Task.Run(() => cities = connection.Cities.ToList());
            foreach (City city in cities)
            {
                CityViewModel cityViewModel = new CityViewModel(citiesDesignerViewModel, city);
                cityViewModelsValue.Add(cityViewModel);

                cityViewModel.ChangesMade += Designer.WheelReportViewModel_ChangesMade;
            }

            List<Spur> spurs = null;
            await Task.Run(() => spurs = connection.Spurs.ToList());
            foreach (Spur spur in spurs)
            {
                SpurViewModel spurViewModel = new SpurViewModel(citiesDesignerViewModel, spur);
                spurViewModelsValue.Add(spurViewModel);

                spurViewModel.ChangesMade += Designer.WheelReportViewModel_ChangesMade;
            }

            yardsDesignerViewModel = new YardsDesignerViewModel();
            List<Yard> yards = null;
            await Task.Run(() => yards = connection.Yards.ToList());
            foreach (Yard yard in yards)
            {
                YardViewModel yardViewModel = new YardViewModel(yardsDesignerViewModel, yard);
                yardViewModelsValue.Add(yardViewModel);

                yardViewModel.ChangesMade += Designer.WheelReportViewModel_ChangesMade;
            }

            List<YardTrack> yardTracks = null;
            await Task.Run(() => yardTracks = connection.YardTracks.ToList());
            foreach (YardTrack yardTrack in yardTracks)
            {
                if(yardTrack.Yard != null)
                    yardTracksValue.Add(yardTrack);
            }
;
            MakeOverviews();
        }

        private void MakeOverviews()
        {
            // cars
            var groupcars = carViewModelsValue
                .OrderBy(item => item.RollingStock.Road.Name)
                .GroupBy(item => item.RollingStock.Road.Name);

            overviewModelValue.carsOverview = "";
            string formattedString = "";
            int offlayoutCount = 0;
            int industriesInCount = 0, citiesInCount = 0, yardsInCount = 0;
            foreach (var groupItem in groupcars)
            {
                int loadedCount = 0, emptyCount = 0, availableCount = 0;
                foreach(var carItem in groupItem)
                {
                    if (carItem.LoadsEmpty.Loaded)
                        loadedCount++;
                    else
                        emptyCount++;

                    if(carItem.RollingStock.RollingStockStatus.DwellTimeData.Available)
                        availableCount++;

                    if (carItem.RollingStock.RollingStockStatus.Location.IsSelected)
                        offlayoutCount++;

                    if (carItem.RollingStock.RollingStockStatus.Location.Parent.IsIndustry)
                        industriesInCount++;
                    else if (carItem.RollingStock.RollingStockStatus.Location.Parent.IsCity)
                        citiesInCount++;
                    else if (carItem.RollingStock.RollingStockStatus.Location.Parent.IsYard)
                        yardsInCount++;
                }

                formattedString += string.Format("{0}: ({1}) {2} loaded, {3} empty, {4} available\r\n"
                    , groupItem.Key, groupItem.Count(), loadedCount, emptyCount, availableCount);
            }
            formattedString += String.Format("\r\nOff layout/in maintenance: {0}\r\n", offlayoutCount);
            formattedString += String.Format("In industries: {0}\r\n", industriesInCount);
            formattedString += String.Format("In cities: {0}\r\n", citiesInCount);
            formattedString += String.Format("In yards: {0}\r\n", yardsInCount);

            formattedString += String.Format("\r\n0 in motion, 10 stationary");
            formattedString += String.Format("\r\n0 consisted");

            overviewModelValue.carsOverview = formattedString;

            // cabooses
            var groupcabooses = cabooseViewModelsValue
                .OrderBy(item => item.RollingStock.Road.Name)
                .GroupBy(item => item.RollingStock.Road.Name);

            overviewModelValue.caboosesOverview = "";
            formattedString = "";
            offlayoutCount = 0;
            citiesInCount = 0;
            yardsInCount = 0;
            foreach (var groupItem in groupcabooses)
            {
                int availableCount = 0;
                foreach (var cabooseItem in groupItem)
                {
                    if (cabooseItem.RollingStock.RollingStockStatus.DwellTimeData.Available)
                        availableCount++;

                    if (cabooseItem.RollingStock.RollingStockStatus.Location.IsSelected)
                        offlayoutCount++;

                    if (cabooseItem.RollingStock.RollingStockStatus.Location.Parent.IsCity)
                        citiesInCount++;
                    else if (cabooseItem.RollingStock.RollingStockStatus.Location.Parent.IsYard)
                        yardsInCount++;
                }

                formattedString += string.Format("{0}: ({1}) {2} available\r\n"
                    , groupItem.Key, groupItem.Count(), availableCount);
            }
            formattedString += String.Format("\r\nOff layout/in maintenance: {0}\r\n", offlayoutCount);
            formattedString += String.Format("In cities: {0}\r\n", citiesInCount);
            formattedString += String.Format("In yards: {0}\r\n", yardsInCount);

            formattedString += String.Format("\r\n0 in motion, 10 stationary");
            formattedString += String.Format("\r\n0 consisted");

            overviewModelValue.caboosesOverview = formattedString;

            // locomotives
            var grouplocomotives = locomotiveViewModelsValue
                .OrderBy(item => item.RollingStock.Road.Name)
                .GroupBy(item => item.RollingStock.Road.Name);

            overviewModelValue.locomotivesOverview = "";
            formattedString = "";
            offlayoutCount = 0;
            citiesInCount = 0;
            yardsInCount = 0;
            foreach (var groupItem in grouplocomotives)
            {
                int availableCount = 0;
                foreach (var locomotiveItem in groupItem)
                {
                    if (locomotiveItem.RollingStock.RollingStockStatus.DwellTimeData.Available)
                        availableCount++;

                    if (locomotiveItem.RollingStock.RollingStockStatus.Location.IsSelected)
                        offlayoutCount++;

                    if (locomotiveItem.RollingStock.RollingStockStatus.Location.Parent.IsCity)
                        citiesInCount++;
                    else if (locomotiveItem.RollingStock.RollingStockStatus.Location.Parent.IsYard)
                        yardsInCount++;
                }

                formattedString += string.Format("{0}: ({1}) {2} available\r\n"
                    , groupItem.Key, groupItem.Count(), availableCount);
            }
            formattedString += String.Format("\r\nOff layout/in maintenance: {0}\r\n", offlayoutCount);
            formattedString += String.Format("In cities: {0}\r\n", citiesInCount);
            formattedString += String.Format("In yards: {0}\r\n", yardsInCount);

            formattedString += String.Format("\r\n0 in motion, 10 stationary");
            formattedString += String.Format("\r\n0 consisted");

            overviewModelValue.locomotivesOverview = formattedString;

            // cities
            var groupcities = cityViewModelsValue
                .OrderBy(item => item.DisplayText);

            overviewModelValue.citiesOverview = "";
            formattedString = "";
            int industryCount = 0, spurCunt = 0;
            foreach (var groupItem in groupcities)
            {
                industryCount = groupItem.IndustryViewModels.Count();
                spurCunt = groupItem.SpurViewModels.Count();

                var strIndustryCount = industryCount.ToString();
                strIndustryCount += " " + (industryCount > 1 ? "industries" : "industry");
                var strSpurCount = spurCunt.ToString();
                strSpurCount += " " + (spurCunt > 1 ? "spurs" : "spur");
                formattedString += String.Format("{0}\r\n{1} , {2}\r\n", 
                    groupItem.DisplayText, strIndustryCount, strSpurCount);
            }

            overviewModelValue.citiesOverview = formattedString;

            // yards
            var groupyards = yardViewModelsValue
                .OrderBy(item => item.DisplayText);

            overviewModelValue.yardsOverview = "";
            formattedString = "";
            int yardTrackCount = 0, carCount = 0;
            foreach (var groupItem in groupyards)
            {
                yardTrackCount = groupItem.YARD.YardTracks.Count();
                carCount = groupItem.Cars.Count();

                var strYardTrackCount = yardTrackCount.ToString();
                strYardTrackCount += " " + (yardTrackCount > 1 ? "tracks" : "track");
                var strCarCunt = carCount.ToString();
                strCarCunt += " " + (carCount > 1 ? "cars" : "car");

                formattedString += String.Format("{0}\r\n{1} , {2}\r\n", 
                    groupItem.DisplayText, strYardTrackCount, strCarCunt);
            }
                
            overviewModelValue.yardsOverview = formattedString;

            // Trains
            overviewModelValue.trainsOverview = "(Under Construction)";
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Unhook event handlers.
                    //this.ChangesMade -= this.ChildViewModel_ChangesMade;

                    // Dispose of child ViewModels which need disposing.
                    // Currently, none require this.                        
                }
                this.disposed = true;
            }
        }



        public OverviewViewModel overviewModel
        {
            get { return overviewModelValue; }
        }
        public ObservableCollection<CarViewModel> carViewModels
        {
            get { return this.carViewModelsValue; }
        }

        public ObservableCollection<CabooseViewModel> cabooseViewModels
        {
            get { return this.cabooseViewModelsValue; }
        }

        public ObservableCollection<LocomotiveViewModel> locomotiveViewModels
        {
            get { return this.locomotiveViewModelsValue; }
        }

        public ObservableCollection<CityViewModel> cityViewModels
        {
            get { return this.cityViewModelsValue; }
        }

        public ObservableCollection<SpurViewModel> spurViewModels
        {
            get { return this.spurViewModelsValue; }
        }
        
        public ObservableCollection<YardViewModel> yardViewModels
        {
            get { return this.yardViewModelsValue; }
        }

        public ObservableCollection<YardTrack> yardTracks
        {
            get { return this.yardTracksValue; }
        }


        public CarViewModel selectedCarViewModel
        {
            get { return this.selectedCarViewModelValue; }
            set { this.selectedCarViewModelValue = value; }
        }

        public CabooseViewModel selectedCabooseViewModel
        {
            get { return this.selectedCabooseViewModelValue; }
            set { this.selectedCabooseViewModelValue = value; }
        }

        public LocomotiveViewModel selectedLocomotiveViewModel
        {
            get { return this.selectedLocomotiveViewModelValue; }
            set { this.selectedLocomotiveViewModelValue = value; }
        }

        public CityViewModel selectedCityViewModel
        {
            get { return this.selectedCityViewModelValue; }
            set { this.selectedCityViewModelValue = value; }
        }

        public SpurViewModel selectedSpurViewModel
        {
            get { return this.selectedSpurViewModelValue; }
            set { this.selectedSpurViewModelValue = value; }
        }

        public YardViewModel selectedYardViewModel
        {
            get { return this.selectedYardViewModelValue; }
            set { this.selectedYardViewModelValue = value; }
        }

        public YardTrack selectedYardTrack
        {
            get { return this.selectedYardTracksValue; }
            set { this.selectedYardTracksValue = value; }
        }
        
    }
}
