using System;
using TrackBoss.Data;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.ViewModel.Cabeese
{
    public class CupolaTypeViewModel : ChangeTrackingViewModel, IEquatable<CupolaTypeViewModel>, IComparable
    {
        #region Operator Overrides

        public static int Compare(CupolaTypeViewModel left, CupolaTypeViewModel right)
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

        public static bool operator ==(CupolaTypeViewModel left, CupolaTypeViewModel right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(CupolaTypeViewModel left, CupolaTypeViewModel right)
        {
            return !(left == right);
        }

        public static bool operator <(CupolaTypeViewModel left, CupolaTypeViewModel right)
        {
            return (CupolaTypeViewModel.Compare(left, right) < 0);
        }

        public static bool operator >(CupolaTypeViewModel left, CupolaTypeViewModel right)
        {
            return (CupolaTypeViewModel.Compare(left, right) > 0);
        }

        #endregion

        #region Fields

        private CupolaType cupolaType;
        
        #endregion

        #region Constructor(s)

        private CupolaTypeViewModel()
        {
            // Load defaults.
        }

        public CupolaTypeViewModel(CupolaType cupolaType) : this()
        {
            // Validate parameter.
            if (cupolaType == null)
                throw new ArgumentNullException(nameof(cupolaType));

            // Assign member fields.
            this.cupolaType = cupolaType;

            // Update display text.
            this.updateDisplayText();

            // Begin tracking changes.
            this.StartTrackingChanges();
        }

        #endregion

        #region Override Methods

        public override string ToString()
        {
            if (this.cupolaType == null)
                return base.ToString();
            return this.cupolaType.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CupolaTypeViewModel);
        }

        public override int GetHashCode()
        {
            string result = this.cupolaType.ToString();
            if (result == null)
                return base.GetHashCode();
            return result.GetHashCode();
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(CupolaTypeViewModel other)
        {
            if (other == null)
                return false;
            return this.cupolaType.ID == other.cupolaType.ID;
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            CupolaTypeViewModel cupolaType = obj as CupolaTypeViewModel;
            if (cupolaType == null)
                return -1;

            return string.Compare(this.ToString(), cupolaType.ToString());
        }

        #endregion

        #region Private Methods

        private void updateDisplayText()
        {
            this.DisplayText = this.cupolaType.Name;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the cupola type data object encapsulated by this ViewModel.
        /// </summary>
        /// <returns>The encapsulated cupola type data object.</returns>
        public CupolaType ToCupolaType()
        {
            return this.cupolaType;
        }

        #endregion

        #region Properties

        public string Abbreviation
        {
            get { return this.cupolaType.Abbreviation; }
            set
            {
                if (this.cupolaType.Abbreviation != value)
                {
                    this.cupolaType.Abbreviation = value;
                    this.updateDisplayText();
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return this.cupolaType.Name; }
            set
            {
                if (this.cupolaType.Name != value)
                {
                    this.cupolaType.Name = value;
                    this.updateDisplayText();
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool VendorProvided
        {
            get { return this.cupolaType.VendorProvided; }
            set
            {
                if (this.cupolaType.VendorProvided != value)
                {
                    this.cupolaType.VendorProvided = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        
        #endregion
    }
}
