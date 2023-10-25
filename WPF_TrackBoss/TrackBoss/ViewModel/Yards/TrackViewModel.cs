using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackBoss.Data;
using TrackBoss.Data.Enumerations;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.Cabeese;
using static Syncfusion.Windows.Controls.SfNavigator;

namespace TrackBoss.ViewModel.Yards
{
    public class TrackViewModel: ChangeTrackingViewModel, IDisposable
    {
        private string displayText = "";
        private ObservableCollection<string> directions;
        private long directionEnum;
        private long length;

        public Track CurTrack;

        #region Constructors
        public TrackViewModel(Track track)
        {
            CurTrack = track;

            directions = new ObservableCollection<string>();
            directions.Add("No Restriction");
            directions.Add("North");
            directions.Add("South");
            directions.Add("West");
            directions.Add("East");

            directionEnum = (int)track.DirectionEnum;
            length = (int)track.Length;

            DisplayText = track.ToString();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion

        #region Properties
        public int DirectionEnum
        {
            get 
            { 
                return (int)this.directionEnum; 
            }
            set 
            { 
                this.directionEnum = value;
                this.CurTrack.DirectionEnum = value;

                this.NotifyPropertyChanged();
            }
        }

        public int Length
        {
            get
            {
                return (int)this.length;
            }
            set
            {
                this.length = value;
                CurTrack.Length = value;

                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> Directions
        {
            get 
            {
                return this.directions; 
            }
        }

        public string DisplayText
        {
            get { return this.displayText; }
            set
            {
                if (this.displayText != value)
                {
                    this.displayText = value;
                    this.CurTrack.Location.Name = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        #endregion
    }
}
