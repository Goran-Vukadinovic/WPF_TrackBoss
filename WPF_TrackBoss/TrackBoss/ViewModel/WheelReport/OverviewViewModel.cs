using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackBoss.Data;
using TrackBoss.ViewModel.Cars;
using TrackBoss.ViewModel.WheelReport;

namespace TrackBoss.ViewModel.WheelReport
{
    internal class OverviewViewModel : ChangeTrackingViewModel, IDisposable
    {
        private bool disposed;

        ObservableCollection<CarViewModel> carViewModelsValue;

        public OverviewViewModel()
        {
            StartTrackingChanges();
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

        private string _carsOverview;
        public string carsOverview
        {
            get { return _carsOverview; }
            set 
            { 
                _carsOverview = value;
                this.NotifyPropertyChanged();
            }
        }

        private string _caboosesOverview;
        public string caboosesOverview
        {
            get { return _caboosesOverview; }
            set 
            {
                _caboosesOverview = value;
                this.NotifyPropertyChanged();
            }
        }

        private string _locomotivesOverview;
        public string locomotivesOverview
        {
            get { return _locomotivesOverview; }
            set
            { 
                _locomotivesOverview = value;
                this.NotifyPropertyChanged();
            }
        }

        private string _citiesOverview;
        public string citiesOverview
        {
            get { return _citiesOverview; }
            set
            { 
                _citiesOverview = value;
                this.NotifyPropertyChanged();
            }
        }

        private string _yardsOverview;
        public string yardsOverview
        {
            get { return _yardsOverview; }
            set 
            {
                _yardsOverview = value;
                this.NotifyPropertyChanged();
            }
        }

        private string _trainsOverview;
        public string trainsOverview
        {
            get { return _trainsOverview; }
            set {
                _trainsOverview = value;
                this.NotifyPropertyChanged();
            }
        }
    }
}
