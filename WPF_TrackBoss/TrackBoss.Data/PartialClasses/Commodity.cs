using System;

/**
 * Programmer: Chad R. Hearn
 * Entities:   TrackBoss, LLC 
 * Legal:      ©2012-2020 TrackBoss, LLC., all rights reserved.
 */

namespace TrackBoss.Data
{
    public partial class Commodity : IEquatable<Commodity>, IComparable
    {
        #region Operator Overrides

        public static int Compare(Commodity left, Commodity right)
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

        public static bool operator ==(Commodity left, Commodity right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(Commodity left, Commodity right)
        {
            return !(left == right);
        }

        public static bool operator <(Commodity left, Commodity right)
        {
            return (Commodity.Compare(left, right) < 0);
        }

        public static bool operator >(Commodity left, Commodity right)
        {
            return (Commodity.Compare(left, right) > 0);
        }

        #endregion

        #region Object Overrides

        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        /// <returns>The name for this commodity. If no data is present then
        /// the default fully qualified name is returned.</returns>
        public override string ToString()
        {
            // Return name or abbreviation.
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;

            // Return default.
            return base.ToString();
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

        public bool Equals(Commodity other)
        {
            if (other == null)
                return false;
            return this.ID == other.ID;
        }

        #endregion

        #region IComparable

        public int CompareTo(object obj)
        {
            Commodity commodity = obj as Commodity;
            if (commodity == null)
                return -1;

            return string.Compare(this.ToString(), commodity.ToString());
        }

        #endregion
    }
}
