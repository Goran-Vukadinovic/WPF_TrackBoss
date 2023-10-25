using System;
using TrackBoss.Data;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cities
{
    public class CityGroupViewModel : ChangeTrackingViewModel, IEquatable<CityGroupViewModel>, IComparable
    {
        #region Operator Overrides

        public static int Compare(CityGroupViewModel left, CityGroupViewModel right)
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

        public static bool operator ==(CityGroupViewModel left, CityGroupViewModel right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(CityGroupViewModel left, CityGroupViewModel right)
        {
            return !(left == right);
        }

        public static bool operator <(CityGroupViewModel left, CityGroupViewModel right)
        {
            return (CityGroupViewModel.Compare(left, right) < 0);
        }

        public static bool operator >(CityGroupViewModel left, CityGroupViewModel right)
        {
            return (CityGroupViewModel.Compare(left, right) > 0);
        }

        #endregion

        #region Fields

        private CityGroup cityGroup;

        private bool disposed;

        #endregion

        #region Constructors

        public CityGroupViewModel(CityGroup CityGroup)
        {
            // Validate parameter.
            if (CityGroup == null)
                throw new ArgumentNullException(nameof(CityGroup));

            // Assign member fields.
            this.cityGroup = CityGroup;

            // Update display text.
            this.updateDisplayText();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        #endregion

        #region Private Methods

        private void updateDisplayText()
        {
            this.DisplayText = this.cityGroup.Name;
        }

        #endregion

        #region Event Handlers

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the city group data object encapsulated by this ViewModel.
        /// </summary>
        /// <returns>The encapsulated city group data object.</returns>
        public CityGroup ToCityGroup()
        {
            return this.cityGroup;
        }

        #endregion

        #region Override Methods

        public override string ToString()
        {
            if (this.cityGroup == null)
                return base.ToString();
            return this.cityGroup.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CityGroupViewModel);
        }

        public override int GetHashCode()
        {
            return this.cityGroup.GetHashCode();
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(CityGroupViewModel other)
        {
            if (other == null)
                return false;
            return this.cityGroup.ID == other.cityGroup.ID;
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            CityGroupViewModel site = obj as CityGroupViewModel;
            if (site == null)
                return -1;

            return string.Compare(this.ToString(), site.ToString());
        }

        #endregion

        #region Properties

        #region Child ViewModels

        #endregion

        public string Abbreviation
        {
            get { return this.cityGroup.Abbreviation; }
            set
            {
                if (this.cityGroup.Abbreviation != value)
                {
                    this.cityGroup.Abbreviation = value;
                    this.updateDisplayText();
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return this.cityGroup.Name; }
            set
            {
                if (this.cityGroup.Name != value)
                {
                    this.cityGroup.Name = value;
                    this.updateDisplayText();
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool VendorProvided
        {
            get { return this.cityGroup.VendorProvided; }
            set
            {
                if (this.cityGroup.VendorProvided != value)
                {
                    this.cityGroup.VendorProvided = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
