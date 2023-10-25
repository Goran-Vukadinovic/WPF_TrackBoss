using System;
using TrackBoss.Configuration;
using TrackBoss.Data;
using TrackBoss.Shared.Enumerations;
using TrackBoss.Shared.Events;
using TrackBoss.Windsor;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cars
{
    public class CarTypeViewModel : ChangeTrackingViewModel, IEquatable<CarTypeViewModel>, IComparable, IDisposable
    {
        #region Operator Overrides

        public static int Compare(CarTypeViewModel left, CarTypeViewModel right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return 0;
            }
            if (object.ReferenceEquals(left, null))
            {
                return -1;
            }
            return left.CompareTo(right);
        }

        public static bool operator ==(CarTypeViewModel left, CarTypeViewModel right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(CarTypeViewModel left, CarTypeViewModel right)
        {
            return !(left == right);
        }

        public static bool operator <(CarTypeViewModel left, CarTypeViewModel right)
        {
            return (CarTypeViewModel.Compare(left, right) < 0);
        }

        public static bool operator >(CarTypeViewModel left, CarTypeViewModel right)
        {
            return (CarTypeViewModel.Compare(left, right) > 0);
        }

        #endregion

        #region Fields

        private Conductor conductor;

        private CarType carType;

        private CarGroupViewModel carGroupViewModelValue;

        private bool disposed;

        #endregion

        #region Constructor(s)

        private CarTypeViewModel() : base()
        {
            // Initialize fields.
            this.conductor = CastleWindsor.Default.Resolve<Conductor>();
            this.conductor.Preferences.PreferencesChanged += this.Preferences_PreferencesChanged;
        }

        public CarTypeViewModel(CarType carType) : this()
        {
            // Validate parameter.
            if (carType == null)
                throw new ArgumentNullException(nameof(carType));

            // Assign member fields.
            this.carType = carType;
            if(this.carType.CarGroup != null)
                this.carGroupViewModelValue = new CarGroupViewModel(this.carType.CarGroup);

            // Update display text.
            this.updateDisplayText();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        #endregion

        #region Override Methods

        public override string ToString()
        {
            if (this.carType == null)
                return base.ToString();
            return this.carType.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CarTypeViewModel);
        }

        public override int GetHashCode()
        {
            string result = this.carType.ToString();
            if (result == null)
                return base.GetHashCode();
            return result.GetHashCode();
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
                    this.conductor.Preferences.PreferencesChanged -= this.Preferences_PreferencesChanged;

                    // Dispose of child ViewModels that need disposing.
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

        #region IEquatable Implementation

        public bool Equals(CarTypeViewModel other)
        {
            if (other == null)
                return false;
            return this.carType.ID == other.carType.ID;
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            CarTypeViewModel carType = obj as CarTypeViewModel;
            if (carType == null)
                return -1;

            return string.Compare(this.ToString(), carType.ToString());
        }

        #endregion

        #region Event Handlers
        
        private void Preferences_PreferencesChanged(object sender, EventArgs<string> e)
        {
            if (e.Value == nameof(this.conductor.Preferences.CarTypeDisplayOption))
                this.updateDisplayText();
        }

        #endregion

        #region Private Methods

        private void updateDisplayText()
        {
            if (this.conductor.Preferences.CarTypeDisplayOption == CarTypeDisplayOption.Primary)
                this.DisplayText = this.carType.Name;
            else if (this.conductor.Preferences.CarTypeDisplayOption == CarTypeDisplayOption.Secondary)
                this.DisplayText = this.carType.Abbreviation;
            else
                throw new NotSupportedException("The specified car type display option is not supported.");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the car type data object encapsulated by this ViewModel.
        /// </summary>
        /// <returns>The encapsulated car type data object.</returns>
        public CarType ToCarType()
        {
            return this.carType;
        }

        #endregion

        #region Properties

        public string Abbreviation
        {
            get { return this.carType.Abbreviation; }
            set
            {
                if (this.carType.Abbreviation != value)
                {
                    this.carType.Abbreviation = value;
                    this.updateDisplayText();
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return this.carType.Name; }
            set
            {
                if (this.carType.Name != value)
                {
                    this.carType.Name = value;
                    this.updateDisplayText();
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool VendorProvided
        {
            get { return this.carType.VendorProvided; }
            set
            {
                if (this.carType.VendorProvided != value)
                {
                    this.carType.VendorProvided = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        #region Child ViewModels
        
        public CarGroupViewModel CarGroup
        {
            get { return this.carGroupViewModelValue; }
        }

        #endregion

        #endregion
    }
}
