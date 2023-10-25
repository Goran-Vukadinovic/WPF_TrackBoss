using System;
using TrackBoss.Data;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cars
{
    public class CarGroupViewModel : ChangeTrackingViewModel
    {
        #region Operator Overrides

        public static int Compare(CarGroupViewModel left, CarGroupViewModel right)
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

        public static bool operator ==(CarGroupViewModel left, CarGroupViewModel right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(CarGroupViewModel left, CarGroupViewModel right)
        {
            return !(left == right);
        }

        public static bool operator <(CarGroupViewModel left, CarGroupViewModel right)
        {
            return (CarGroupViewModel.Compare(left, right) < 0);
        }

        public static bool operator >(CarGroupViewModel left, CarGroupViewModel right)
        {
            return (CarGroupViewModel.Compare(left, right) > 0);
        }

        #endregion

        #region Fields

        private CarGroup carGroup;

        #endregion

        #region Constructor(s)

        private CarGroupViewModel()
        {
            // Load defaults.
        }

        public CarGroupViewModel(CarGroup carGroup) : this()
        {
            // Validate parameter.
            if (carGroup == null)
                throw new ArgumentNullException(nameof(carGroup));

            // Assign member fields.
            this.carGroup = carGroup;

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        #endregion

        #region Private Methods

        #endregion

        #region Event Handlers

        #endregion

        #region Public Methods

        #endregion

        #region Override Methods

        public override string ToString()
        {
            if (this.carGroup == null)
                return base.ToString();
            return this.carGroup.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CarGroupViewModel);
        }

        public override int GetHashCode()
        {
            string result = this.carGroup.ToString();
            if (result == null)
                return base.GetHashCode();
            return result.GetHashCode();
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(CarGroupViewModel other)
        {
            if (other == null)
                return false;
            return this.carGroup.ID == other.carGroup.ID;
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            CarGroupViewModel carGroup = obj as CarGroupViewModel;
            if (carGroup == null)
                return -1;

            return string.Compare(this.ToString(), carGroup.ToString());
        }

        #endregion

        #region Properties

        public string Abbreviation
        {
            get { return this.carGroup.Abbreviation; }
            set
            {
                if (this.carGroup.Abbreviation != value)
                {
                    this.carGroup.Abbreviation = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return this.carGroup.Name; }
            set
            {
                if (this.carGroup.Name != value)
                {
                    this.carGroup.Name = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool VendorProvided
        {
            get { return this.carGroup.VendorProvided; }
            set
            {
                if (this.carGroup.VendorProvided != value)
                {
                    this.carGroup.VendorProvided = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
