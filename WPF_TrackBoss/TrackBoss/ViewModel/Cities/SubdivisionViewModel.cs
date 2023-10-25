using System;
using TrackBoss.Data;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cities
{
    public class SubdivisionViewModel : ChangeTrackingViewModel, IEquatable<SubdivisionViewModel>, IComparable
    {
        #region Operator Overrides

        public static int Compare(SubdivisionViewModel left, SubdivisionViewModel right)
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

        public static bool operator ==(SubdivisionViewModel left, SubdivisionViewModel right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(SubdivisionViewModel left, SubdivisionViewModel right)
        {
            return !(left == right);
        }

        public static bool operator <(SubdivisionViewModel left, SubdivisionViewModel right)
        {
            return (SubdivisionViewModel.Compare(left, right) < 0);
        }

        public static bool operator >(SubdivisionViewModel left, SubdivisionViewModel right)
        {
            return (SubdivisionViewModel.Compare(left, right) > 0);
        }

        #endregion

        #region Fields

        private Subdivision subdivision;

        private bool disposed;

        #endregion

        #region Constructors

        public SubdivisionViewModel(Subdivision subdivision)
        {
            // Validate parameter.
            if (subdivision == null)
                throw new ArgumentNullException(nameof(subdivision));

            // Assign member fields.
            this.subdivision = subdivision;

            // Update display text.
            this.updateDisplayText();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        #endregion
        
        #region Private Methods

        private void updateDisplayText()
        {
            this.DisplayText = this.subdivision.ToString();
        }

        #endregion

        #region Event Handlers

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the subdivision data object encapsulated by this ViewModel.
        /// </summary>
        /// <returns>The encapsulated subdivision data object.</returns>
        public Subdivision ToSubdivision()
        {
            return this.subdivision;
        }

        #endregion

        #region Override Methods

        public override string ToString()
        {
            if (this.subdivision == null)
                return base.ToString();
            return this.subdivision.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as SubdivisionViewModel);
        }

        public override int GetHashCode()
        {
            return this.subdivision.GetHashCode();
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(SubdivisionViewModel other)
        {
            if (other == null)
                return false;
            return this.subdivision.ID == other.subdivision.ID;
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            SubdivisionViewModel site = obj as SubdivisionViewModel;
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
            get { return this.subdivision.Abbreviation; }
            set
            {
                if (this.subdivision.Abbreviation != value)
                {
                    this.subdivision.Abbreviation = value;
                    this.updateDisplayText();
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return this.subdivision.Name; }
            set
            {
                if (this.subdivision.Name != value)
                {
                    this.subdivision.Name = value;
                    this.updateDisplayText();
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool VendorProvided
        {
            get { return this.subdivision.VendorProvided; }
            set
            {
                if (this.subdivision.VendorProvided != value)
                {
                    this.subdivision.VendorProvided = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
