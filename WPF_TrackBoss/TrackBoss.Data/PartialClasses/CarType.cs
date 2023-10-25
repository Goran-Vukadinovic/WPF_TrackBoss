using System;

/**
* Programmer: Chad R. Hearn
* Entities:   TrackBoss, LLC 
* Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
*/

namespace TrackBoss.Data
{
    public partial class CarType : IEquatable<CarType>, IComparable
    {
        #region Operator Overrides

        public static int Compare(CarType left, CarType right)
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

        public static bool operator ==(CarType left, CarType right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(CarType left, CarType right)
        {
            return !(left == right);
        }

        public static bool operator <(CarType left, CarType right)
        {
            return (CarType.Compare(left, right) < 0);
        }

        public static bool operator >(CarType left, CarType right)
        {
            return (CarType.Compare(left, right) > 0);
        }

        #endregion

        #region Fields

        #endregion

        #region Object Overrides

        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        /// <returns>The name or abbreviation for this commodity depending on 
        /// which has data. If neither has data the default fully qualified 
        /// name is returned.</returns>
        public override string ToString()
        {
            // Return name or abbreviation.
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            else if (!string.IsNullOrEmpty(this.Abbreviation))
                return this.Abbreviation;

            // Return default.
            return base.ToString();
        }

        /// <summary>
        /// Displays the string representation of this object using the
        /// abbreviated form if requested.
        /// </summary>
        /// <param name="useAbbreviation">Indicates whether or not the
        /// abbreviated form should be used.</param>
        /// <returns>The string representation of this object.</returns>
        public string ToString(bool useAbbreviation)
        {
            // If regular form is requested, then return that.
            if (!useAbbreviation)
                return this.ToString();

            // If no abbreviation is available, use default.
            if (string.IsNullOrEmpty(this.Abbreviation))
                return base.ToString();

            // Return abbreviation.
            return this.Abbreviation;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as CarType);
        }

        public override int GetHashCode()
        {
            // This MUST BE unique across car types.
            return this.ID.GetHashCode();
        }

        #endregion

        #region IEquatable Implementation

        public bool Equals(CarType other)
        {
            if (other == null)
                return false;
            return this.ID == other.ID;
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            CarType carType = obj as CarType;
            if (carType == null)
                return -1;

            return string.Compare(this.ToString(), carType.ToString());
        }

        #endregion

        #region Non-DB Properties

        #endregion
    }
}