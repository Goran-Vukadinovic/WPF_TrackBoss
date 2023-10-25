using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackBoss.Data;
using TrackBoss.Data.Enumerations;
using TrackBoss.Model.Enumerations;
using TrackBoss.Shared.Events;
using TrackBoss.ViewModel.Cabeese;
using TrackBoss.ViewModel.RollingStocks;
using TrackBoss.ViewModel.Shared;

namespace TrackBoss.ViewModel.Cities
{
    public class SpurViewModel : ChangeTrackingViewModel, IDisposable
    {
        public CitiesDesignerViewModel Designer { get; }
        private DwellTimeDataViewModel dwellTimeDataViewModelValue; 
        private PhotoViewModel photoViewModelValue;

        public Spur CurSpur;
        private ObservableCollection<string> directions;
        private long directionRestrictionEnum;
        private long pointOrientationEnum;
        private long scaleLength;
        private IndustryViewModel industryViewModelValue;

        public SpurViewModel spurViewModel
        {
            get { return this; }
        }

        #region Constructors
        public SpurViewModel(Spur spur)
        {
            CurSpur = spur;

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        public Spur ToSpur()
        {
            return CurSpur;
        }

        public SpurViewModel(CitiesDesignerViewModel designerViewModel, Spur spur)
        {
            CurSpur = spur;
            Designer = designerViewModel;

            directions = new ObservableCollection<string>();
            directions.Add("No Restriction");
            directions.Add("North");
            directions.Add("South");
            directions.Add("West");
            directions.Add("East");

            directionRestrictionEnum = (int)spur.DirectionRestrictionEnum;
            if(spur.PrintOrder != null)
                pointOrientationEnum = (int)spur.PrintOrder;
            DisplayText = spur.ToString();
            ScaleLength = (long)spur.ScaleLength;
            industryViewModelValue = new IndustryViewModel(null, spur.Industry);

            if (this.CurSpur.DwellTimeData == null)
                this.CurSpur.DwellTimeData = new DwellTimeData();
            this.dwellTimeDataViewModelValue = new DwellTimeDataViewModel(this.CurSpur.DwellTimeData);
            this.dwellTimeDataViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;

            this.ChangesMade += this.SpurViewModel_ChangesMade;


            if (this.CurSpur.City.Site.Location.Photo != null)
                this.photoViewModelValue = new PhotoViewModel(this.CurSpur.City.Site.Location.Photo);
            else
            {
                Photo photo = new Photo();
                this.CurSpur.City.Site.Location.Photo = photo;
                this.photoViewModelValue = new PhotoViewModel(photo);
            }

            // Hook-up event handlers.
            this.photoViewModelValue.ChangesMade += this.ChildViewModel_ChangesMade;

            // Begin tracking changes.
            this.StartTrackingChanges();
        }
        #endregion

        #region Private
        private void ChildViewModel_ChangesMade(object sender, EventArgs<string> e)
        {
            // Mark this ViewModel as having changes.
            this.HasChanges = true;

            // Raise the changes made event.
            this.OnChangesMade(e.Value);
        }

        private void SpurViewModel_ChangesMade(object sender, EventArgs e)
        {
            // Validate parameters.
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            // Invalidate commands.
            Designer.invalidateAllCommands_Industry();
        }
        #endregion

        #region Properties
        public ObservableCollection<string> Directions
        {
            get { return this.directions; }
        }

        private string displayText = "";
        public string DisplayText
        {
            get { return CurSpur.Track.Location.Name; }
            set
            {
                if (CurSpur.Track.Location.Name != value)
                {
                    CurSpur.Track.Location.Name = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return this.DisplayText; }
            set
            {
                if (this.DisplayText != value)
                {
                    this.DisplayText = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public int DirectionRestrictionEnum
        {
            get
            {
                return (int)this.directionRestrictionEnum;
            }
            set
            {
                this.directionRestrictionEnum = value;
                CurSpur.DirectionRestrictionEnum = value;

                this.NotifyPropertyChanged();
            }
        }

        public int PointOrientationEnum
        {
            get
            {
                return (int)this.pointOrientationEnum;
            }
            set
            {
                this.pointOrientationEnum = value;
                CurSpur.PrintOrder = value;

                this.NotifyPropertyChanged();
            }
        }

        
        public long ScaleLength
        {
            get
            {
                return (long)this.scaleLength;
            }
            set
            {
                this.scaleLength = value;
                CurSpur.ScaleLength = value;

                this.NotifyPropertyChanged();
            }
        }

        public DwellTimeDataViewModel DwellTimeData
        {
            get
            {
                return dwellTimeDataViewModelValue;
            }
            set
            {
                dwellTimeDataViewModelValue = value;

                this.NotifyPropertyChanged();
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

        public void Dispose()
        {
            this.ChangesMade -= this.SpurViewModel_ChangesMade;
            this.photoViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;
            this.photoViewModelValue.Dispose();
            this.dwellTimeDataViewModelValue.ChangesMade -= this.ChildViewModel_ChangesMade;
        }

        public long PrintOrder
        {
            get
            {
                return (long)CurSpur.PrintOrder;
            }
            set
            {
                CurSpur.PrintOrder = value;
            }
        }

        public bool IsUseKammFactor
        {
            get
            {
                bool bRet = true;
                if (this.CurSpur.KammFactor == null)
                    bRet = false;
                return bRet;
            }
            set
            {
                if (value == true)
                {
                    this.CurSpur.KammFactor = new KammFactor();
                }
                else
                {
                    this.CurSpur.KammFactor = null;
                }

                this.NotifyPropertyChanged();
            }
        }

        public long KammFactorSwitchCount
        {
            get
            {
                if(CurSpur.KammFactor == null)
                    return 0;
                return CurSpur.KammFactor.SwitchCount;
            }
            set
            {
                CurSpur.KammFactor.SwitchCount = value;
                this.NotifyPropertyChanged();
            }
        }

        public long KammFactorIndustryDepth
        {
            get
            {
                if (CurSpur.KammFactor == null)
                    return 0;
                return CurSpur.KammFactor.IndustryDepth;
            }
            set
            {
                CurSpur.KammFactor.IndustryDepth = value;
                this.NotifyPropertyChanged();
            }
        }

        public long KammFactorCarCount
        {
            get
            {
                if (CurSpur.KammFactor == null)
                    return 0;
                return CurSpur.KammFactor.CarCount;
            }
            set
            {
                CurSpur.KammFactor.CarCount = value;
                this.NotifyPropertyChanged();
            }
        }

        public Difficulty KammFactorDifficulty
        {
            get
            {
                if (CurSpur.KammFactor == null)
                    return null;

                return CurSpur.KammFactor.Difficulty;
            }
            set
            {
                CurSpur.KammFactor.Difficulty = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool IsOffSpotPermission
        {
            get
            {
                bool bRet = true;
                if (this.CurSpur.SpurOffSpotPermission == null)
                    bRet = false;
                return bRet;
            }
            set
            {
                if (value == true)
                {
                    this.CurSpur.SpurOffSpotPermission = new SpurOffSpotPermission();
                }
                else
                {
                    this.CurSpur.SpurOffSpotPermission = null;
                }

                this.NotifyPropertyChanged();
            }
        }

        public long? AllowedOverage
        {
            get
            {
                if (this.CurSpur.SpurOffSpotPermission == null)
                    return null;

                return this.CurSpur.SpurOffSpotPermission.CarCount;
            }
            set
            {
                this.CurSpur.SpurOffSpotPermission.CarCount = value;
                this.NotifyPropertyChanged();
            }
        }

        public IndustryViewModel industryViewModel
        {
            get
            {
                return industryViewModelValue;
            }
            set
            {
                industryViewModelValue = value;
                this.NotifyPropertyChanged();
            }
        }

        public string CityName
        {
            get
            {
                return CurSpur.City.Site.Location.Name;
            }
        }

        public long? ServicePriority
        {
            get
            {
                return CurSpur.ServicePriority;
            }
        }
        #endregion
    }
}
